using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.StoredProcedures
{
    public abstract class SqlGenerator : ICodeGenerator
    {

        public TSqlObject Table { get; private set; } = null;

        public string Author { get; private set; } = "Author";

        public IEnumerable<string> GrantExecuteTo { get; private set; } = new List<string>();


        public SqlGenerator(TSqlObject table, string author = null, IEnumerable<string> grantExecuteTo = null)
        {
            this.Table = table;
            this.Author = author ?? this.Author;
            this.GrantExecuteTo = grantExecuteTo ?? this.GrantExecuteTo;
        }
               
        public abstract string Generate();

    }
}
