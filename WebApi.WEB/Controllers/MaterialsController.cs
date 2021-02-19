using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebApi.BLL.BM;
using WebApi.DAL.Entities;
using WebApi.BLL.Interfaces;

namespace WebApi.WEB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialServices _materialServices;

        public MaterialsController(IMaterialServices materialServices)
        {
            _materialServices = materialServices;
        }

        [HttpGet]
        [Route("wtf")]
        public IActionResult WTF()
        {
            return Ok("Что за шляпа");
        }

        [HttpPost]
        [Authorize(Roles = "admin, writer")]
        public async Task<IActionResult> AddNewMaterial(IFormFile file, MaterialCategories category)
        {
            if (_materialServices.CheckFilesInDB(file.FileName))
            {
                return BadRequest($"File: {file.FileName} already exists");
            }
            if (_materialServices.ValidateOfCategory(category) == false)
            {
                return BadRequest($"Error category. (Presentation, Application, Other)");
            }

            byte[] fileBytes;

            await using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            MaterialFileBM fileMaterialBM = new MaterialFileBM
            {
                FileName = file.FileName,
                FileBytes = fileBytes,
                FileSize = file.Length
            };

            Material material = new Material
            {
                MaterialName = file.FileName,
                Category = category,
                ActualVersion = 1,
                Versions = new List<MaterialVersion>()
            };

            try
            {
                await _materialServices.AddNewMaterialToDB(material, fileMaterialBM);
                return Ok($"Material {file.FileName} has been added successfully");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("version")]
        [Authorize(Roles = "admin, writer")]
        public async Task<IActionResult> AddNewVersionMaterial(IFormFile file)
        {
            int version;

            if (_materialServices.CheckFilesInDB(file.FileName))
            {
                version = _materialServices.GetActualVersion(file.FileName) + 1;
            }
            else
                return BadRequest($"File: {file} don't have in DB yet");

            byte[] fileBytes;

            await using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            MaterialFileBM fileMaterialBM = new MaterialFileBM
            {
                FileName = file.FileName,
                FileBytes = fileBytes,
                FileSize = file.Length
            };

            try
            {
                await _materialServices.AddNewMaterialVersionToDb(fileMaterialBM, version);
                return Ok($"Material {file.FileName}v.{version} has been added successfully");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("info/{category}/{minSize}/{maxSize}")]
        [Authorize(Roles = "admin, reader")]
        public IEnumerable<Material> GetFiltersInfo(MaterialCategories category, double minSize, double maxSize)
        {
            return _materialServices.GetInfoByTheFiltersFromDb(category, minSize, maxSize);
        }

        [HttpGet]
        [Route("info/all")]
        [Authorize(Roles = "admin, reader")]
        public ActionResult<IEnumerable<Material>> GetAllMaterialsInfo()
        {
            if (_materialServices.CheckFilesInDB(null))
            {
                return Ok(_materialServices.GetListOfMaterials());
            }
            else
                return Ok($"DB is empty");
        }

        [HttpGet]
        [Route("info/{name}")]
        [Authorize(Roles = "admin, reader")]
        public ActionResult<Material> GetInfo(string name)
        {
            if (_materialServices.CheckFilesInDB(name))
            {
                return Ok(_materialServices.GetMaterialByName(name));
            }
            else
                return BadRequest($"File: {name} does not exists in DB");

        }

        [HttpGet]
        [Route("{name}")]
        [Authorize(Roles = "admin, reader")]
        public IActionResult GetActualMaterial(string name)
        {
            try
            {
                return File(_materialServices.DownloadMaterialByName(name), "application/octet-stream", name);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("{name}/{version}")]
        [Authorize(Roles = "admin, reader")]
        public IActionResult GetMaterialByVersion(string name, int version)
        {
            try
            {
                return File(_materialServices.DownloadMaterialByNameAndVersion(name, version), "application/octet-stream", name);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch]
        [Authorize(Roles = "admin, writer")]
        public IActionResult ChangeCategory(string name, MaterialCategories category)
        {
            if (_materialServices.CheckFilesInDB(name) && _materialServices.ValidateOfCategory(category) == true)
            {
                _materialServices.ChangeCategoryOfFile(name, category);
                return Ok($"Category of File: {name} has been changed to {category}");
            }
            else
                return BadRequest($"File: {name} does not exists or Error: {category}");
        }

    }
}
