using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.UI
{
    public class GeneratorGlobalSettings
    {
        public string AuthorName { get; set; } = "MSSQL-Dapper Generator";
        public string EntitiesNamespace { get; set; } = "Project.Entities";
        public string[] SelectedMSSQLRolesForExecute { get; set; }

    }
}
