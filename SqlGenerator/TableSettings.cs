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

    }
}
