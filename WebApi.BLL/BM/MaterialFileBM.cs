using System;
using System.Collections.Generic;
using System.Text;
using WebApi.DAL.Entities;

namespace WebApi.BLL.BM
{
    public class MaterialFileBM
    {
        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }
        public long FileSize { get; set; }
    }
}
