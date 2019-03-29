using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.StoredProcedures
{
    public class SqlGeneratorSettings
    {
        public string[] GrantExecuteToRoles { get; set; } = new string[0];
        public string FieldNamesExcluded { get; set; } = null;

    }
}
