using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.DotNetClient
{
    public class CsEntityClassGeneratorSettings : CsCodeGeneratorSettings
    {
        public bool ImplementICloneable {get;set;} = true;
        public string ImplementCustomInterfaceNames { get; set; } = null;
        public bool StandardRequieredDecorator { get; set; } = true;
        public bool StandardStringLengthDecorator { get; set; } = true;
        public bool StandardJsonIgnoreDecorator { get; set; } = false;
        public string FieldNamesWithJsonIgnoreDecorator { get; set; } = null;

        public Dictionary<string, string> FieldNameCustomTypes { get; set; } = null;
        public Dictionary<string, string> FieldNameCustomDecorators { get; set; } = null;
    }
}
