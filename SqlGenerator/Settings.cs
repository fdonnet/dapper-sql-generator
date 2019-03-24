using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class Settings
    {
        //Generation options
        public bool GenerateDeleteSP { get; set; } = true;
        public bool GenerateInsertSP { get; set; } = true;

        public string AuthorName { get; set; } = "MSSQL-Dapper Generator";
        public string EntitiesNamespace { get; set; } = "Project.Entities";

        //Grant roles
        public string[] SelectedRolesForDeleteSP { get; set; } = null;
        public string[] SelectedRolesForInsertSP { get; set; } = null;
        public string[] SelectedRolesForBulkInsertSP { get; set; } = null;
        public string[] SelectedRolesForUpdateSP { get; set; } = null;
        public string[] SelectedRolesForSelectAllSP { get; set; } = null;
        public string[] SelectedRolesForSelectByPKSP { get; set; } = null;
        public string[] SelectedRolesForSelectByUKSP { get; set; } = null;
        public string[] SelectedRolesForTableType { get; set; } = null;
    }
}
