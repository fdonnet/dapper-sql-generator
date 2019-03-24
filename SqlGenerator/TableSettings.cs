using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class TableSettings : Settings
    {
        public string TableName = new Guid().ToString();

        public void CopyValueFromGlobalSettings(Settings globalSettings)
        {
            var clone = (Settings)globalSettings.Clone();

            GenerateDeleteSP = clone.GenerateDeleteSP;
            GenerateInsertSP = clone.GenerateInsertSP;

            AuthorName = clone.AuthorName;
            EntitiesNamespace = clone.EntitiesNamespace;

            SelectedRolesForDeleteSP = clone.SelectedRolesForDeleteSP;
            SelectedRolesForInsertSP = clone.SelectedRolesForInsertSP;
            SelectedRolesForBulkInsertSP = clone.SelectedRolesForBulkInsertSP;
            SelectedRolesForUpdateSP = clone.SelectedRolesForUpdateSP;
            SelectedRolesForSelectAllSP = clone.SelectedRolesForSelectAllSP;
            SelectedRolesForSelectByPKSP = clone.SelectedRolesForSelectByPKSP;
            SelectedRolesForSelectByUKSP = clone.SelectedRolesForSelectByUKSP;
            SelectedRolesForTableType = clone.SelectedRolesForTableType;

        }
    }
}
