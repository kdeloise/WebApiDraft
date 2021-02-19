using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.BLL.BM;
using WebApi.BLL.Interfaces;
using WebApi.BLL.Services;
using WebApi.DAL.EF;
using WebApi.DAL.Entities;

namespace WebApi.BLL.Services
{
    public class MaterialServices : IMaterialServices
    {
        private readonly MaterialsDbContext _context;
        private readonly IFileManager _fileManager;

        public MaterialServices(MaterialsDbContext context, IFileManager fileManager)
        {            
            _context = context;
            _fileManager = fileManager;
        }

        //----------------------------------------DB_SERVICES---------------------------------------------//

        public IEnumerable<Material> GetMaterialsByTheFilters(MaterialCategories category, double minSize, double maxSize)
        {
            var result = new List<Material>();
            var materials = GetListOfMaterials().Where(x => x.Category == category);

            foreach (var mat in materials)
            {
                if (_context.MaterialVersions.Count(x => x.MaterialId == GetMaterialIdByName(mat.MaterialName)
                                                    && minSize <= x.MetaFileSize
                                                    && x.MetaFileSize <= maxSize) > 0)
                    result.Add(mat);
            }
            

            return result;
        }

        public MaterialCategories GetCategoryOfMaterial(string fileName)
        {
            return _context.Materialss.Where(x => x.MaterialName == fileName)
                                                  .First(x => x.Category != 0).Category;
        }

        public int GetActualVersion(string fileName)
        {
            return GetMaterialByName(fileName).ActualVersion;
        }

        //public string GetMaterialByNameJSON(string fileName)
        //{
        //    var material = _context.Materialss.First(x => x.MaterialName == fileName);
        //    var Id = GetMaterialIdByName(fileName);
        //    var materialVersions = _context.MaterialVersions.First(x => x.MaterialId == Id && x.VersionNumber == material.ActualVersion);
        //    var materialJSON = Newtonsoft.Json.JsonConvert.SerializeObject(
        //        new
        //        {
        //            material.MaterialName,
        //            material.Category,
        //            material.ActualVersion,
        //            materialVersions.MetaFileSize,
        //            materialVersions.MetaDateTime                    
        //        });

        //    return materialJSON;
        //}

        public Material GetMaterialByName(string fileName)
        {
            var material = _context.Materialss.First(x => x.MaterialName == fileName);
            var Id = GetMaterialIdByName(fileName);
            var materialVersions = _context.MaterialVersions.Where(x => x.MaterialId == Id);
            material.Versions = new List<MaterialVersion> 
            { _context.MaterialVersions.First(x => x.MaterialId == GetMaterialIdByName(material.MaterialName)) };
            return material;
        }

        //public string GetListOfMaterialsJSON()
        //{
        //    var materials = _context.Materialss.ToList();
        //    var materialsJSON = Newtonsoft.Json.JsonConvert.SerializeObject(materials);
        //    return materialsJSON;
        //}

        public IEnumerable<Material> GetListOfMaterials()
        {
            int i = 0;
            var materials = _context.Materialss.ToList();
            var materialsVersion = _context.MaterialVersions.ToList();
            foreach (var mat in materials)
            {
                i++; 
            }
            while(--i > 0)
            {
                materials[i].Versions = new List<MaterialVersion> 
                (_context.MaterialVersions.Where(x => x.MaterialId == GetMaterialIdByName(materials[i].MaterialName)).ToList());
            }
            return materials;
        }



#nullable enable
        public bool CheckFilesInDB(string? fileName)
        {
            return ((fileName != null)
                ? _context.Materialss.Count(x => x.MaterialName == fileName) > 0
                : _context.Materialss.Count() > 0);
        }

        public bool ValidateOfCategory(MaterialCategories category)
        {
            return Enum.IsDefined(typeof(MaterialCategories), category);
        }

        public int GetMaterialIdByName(string fileName)
        {
            return _context.Materialss.First(x => x.MaterialName == fileName).Id;
        }

        public string GetPathOfMaterialByTheVersionAndName(string fileName, int version)
        {
            var material = GetMaterialByName(fileName);
            var Id = GetMaterialIdByName(fileName);
            var materialVersions = _context.MaterialVersions.Where(x => x.MaterialId == Id);
            material.Versions = materialVersions.ToList();
            return material.Versions
                    .First(x => x.VersionNumber == version).Path;
        }

        //----------------------------------------------------------------------------------------------------------//

        public async Task AddNewMaterialToDB(Material material, MaterialFileBM fileMaterialBM)
        {
            var path = await _fileManager.SaveFile(fileMaterialBM.FileBytes, fileMaterialBM.FileName);

            var materialVersion = new MaterialVersion
            {
                Material = material,
                MetaDateTime = DateTime.Now,
                MetaFileSize = fileMaterialBM.FileSize,
                VersionNumber = 1, 
                Path = path
            };

            material.ActualVersion = 1;
            material.Versions.Add(materialVersion);

            await _context.MaterialVersions.AddAsync(materialVersion);
            await _context.Materialss.AddAsync(material);
            await _context.SaveChangesAsync();
        }

        public async Task AddNewMaterialVersionToDb(MaterialFileBM fileMaterialBM, int version)
        {
            var path = await _fileManager.SaveFile(fileMaterialBM.FileBytes, fileMaterialBM.FileName);
            //var category = GetCategoryOfMaterial(fileMaterialBM.FileName);

            Material newFile = GetMaterialByName(fileMaterialBM.FileName);

            var materialVersion = new MaterialVersion
            {
                Material = newFile,
                MetaDateTime = DateTime.Now,
                MetaFileSize = fileMaterialBM.FileSize,
                VersionNumber = version,
                Path = path
            };

            newFile.ActualVersion = version;
            newFile.Versions.Add(materialVersion);

            await _context.MaterialVersions.AddAsync(materialVersion);
            //await _context.AddAsync(newFile);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<Material> GetInfoByTheFiltersFromDb(MaterialCategories category, double minSize, double maxSize)
        {
            //var materials = new List<Material>();
            var filtersMat = GetMaterialsByTheFilters(category, minSize, maxSize);

            //foreach (var mat in filtersMat)
            //{
            //    materials.Add(new Material
            //    {
            //        MaterialName = mat.MaterialName,
            //        Category = mat.Category,
            //        metaFileSize = mat.metaFileSize,
            //        versionNumber = mat.versionNumber,
            //        metaDateTime = mat.metaDateTime,
            //        path = mat.path
            //    });
            //}

            return filtersMat;
        }

        public FileStream DownloadMaterialByName(string fileName)
        {
            var actualVersion = GetActualVersion(fileName);
            var path = GetPathOfMaterialByTheVersionAndName(fileName, actualVersion);

            return (new FileStream(path, FileMode.Open));
        }

        public FileStream DownloadMaterialByNameAndVersion(string fileName, int version)
        {            
            var path = GetPathOfMaterialByTheVersionAndName(fileName, version);

            return (new FileStream(path, FileMode.Open));
        }

        public void ChangeCategoryOfFile(string fileName, MaterialCategories category)
        {
            var material = GetMaterialByName(fileName);

            material.Category = category;
            _context.Materialss.Update(material);
            _context.SaveChangesAsync();
        }
    }


}
