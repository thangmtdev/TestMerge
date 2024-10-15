using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DREXCreateFunctionForTrussLink.Data
{
    public class ProjectModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Revit_name { get; set; }
        public string Imported_at { get; set; }
        public string Exported_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}