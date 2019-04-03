using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.DotNetClient
{
    public class CsDbContextGeneratorSettings : CsCodeGeneratorSettings
    {
        public string ConnectionStringName { get; set; } = "Default";

        public string ClassName { get; set; } = "DbContext";

    }
}
