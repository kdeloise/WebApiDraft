using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApi.BLL.BM;
using WebApi.DAL.Entities;

namespace WebApi.BLL.Interfaces
{
    public interface IMaterialServices
    {
        IEnumerable<Material> GetMaterialsByTheFilters(MaterialCategories category, double minSize, double maxSize);
        MaterialCategories GetCategoryOfMaterial(string fileName);
        int GetActualVersion(string fileName);
        //string GetMaterialByNameJSON(string fileName);
        public Material GetMaterialByName(string fileName);
        //string GetListOfMaterialsJSON();
        IEnumerable<Material> GetListOfMaterials();
#nullable enable
        bool CheckFilesInDB(string? fileName);
        bool ValidateOfCategory(MaterialCategories category);        
        string GetPathOfMaterialByTheVersionAndName(string fileName, int version);
        Task AddNewMaterialToDB(Material material, MaterialFileBM fileMaterialBM);
        Task AddNewMaterialVersionToDb(MaterialFileBM fileMaterialBM, int version);
        IEnumerable<Material> GetInfoByTheFiltersFromDb(MaterialCategories category, double minSize, double maxSize);
        FileStream DownloadMaterialByName(string fileName);
        FileStream DownloadMaterialByNameAndVersion(string fileName, int version);
        void ChangeCategoryOfFile(string fileName, MaterialCategories category);
    }
}
