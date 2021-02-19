using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebApi.BLL.Interfaces;

namespace WebApi.BLL.Services
{
    public class FileManager : IFileManager
    {
        private string filesDir { get; set; }

        public FileManager(IConfiguration configuration)
        {
            filesDir = configuration["FilesDir"];
            Directory.CreateDirectory(filesDir);
        }

        public async Task<string> SaveFile(byte[] fileBytes, string fileName)
        {
            var path = filesDir + fileName;
            await File.WriteAllBytesAsync(path, fileBytes);
            return path;
        }
    }
}
