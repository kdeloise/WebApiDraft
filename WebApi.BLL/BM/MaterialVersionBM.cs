using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.BLL.BM
{
    public class MaterialVersionBM
    {
        public string Path { get; set; }
        public int VersionNumber { get; set; }
        public double MetaFileSize { get; set; }
        public DateTime MetaDateTime { get; set; }
    }
}
