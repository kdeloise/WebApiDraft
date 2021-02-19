using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.DAL.Entities
{
    public class MaterialVersion
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int VersionNumber { get; set; }
        public double MetaFileSize { get; set; }
        public DateTime MetaDateTime { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; }
    }
}
