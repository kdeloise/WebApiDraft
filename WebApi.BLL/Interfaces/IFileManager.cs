using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.BLL.Interfaces
{
    public interface IFileManager
    {
        Task<string> SaveFile(byte[] fileBytes, string fileName);
    }
}
