using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.DAL.Entities
{
    public enum MaterialCategories
    {
        Presentation = 1,
        Application = 2,
        Other = 3
    }

    public class Material
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public MaterialCategories Category { get; set; }
        public int ActualVersion { get; set; }
        public List<MaterialVersion> Versions { get; set; }
    }
}
