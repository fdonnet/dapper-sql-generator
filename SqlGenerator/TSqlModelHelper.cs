using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;

namespace SqlGenerator
{
    public static class TSqlModelHelper
    {
        
        /// <summary>
        /// Get SQL database model from a .dacpac file
        /// </summary>
        /// <param name="dacpacFileName"></param>
        /// <returns></returns>
        public static TSqlModel LoadModel(string dacpacFileName)
        {
            ModelLoadOptions options = new ModelLoadOptions()
            {
                LoadAsScriptBackedModel = true,
                ModelStorageType = DacSchemaModelStorageType.Memory
            };
            return TSqlModel.LoadFromDacpac(dacpacFileName, options);
        }
        

        /// <summary>
        /// Get all tables from a model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="excludeSysSchema"></param>
        /// <returns></returns>
        public static IEnumerable<TSqlObject> GetAllTables(this TSqlModel model, bool excludeSysSchema = true)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var tables = model.GetObjects(DacQueryScopes.All, ModelSchema.Table);
            if (tables != null)
            {
                if (excludeSysSchema)
                    tables = tables.Where(currTable => currTable.Name.Parts[0].ToLower() != "sys");
            }
            return tables;
        }


        /// <summary>
        /// Get all columns from a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<TSqlObject> GetAllColumns(this TSqlObject table)
        {
            var columns = table.GetReferenced(Table.Columns);
            return columns;
        }


        /// <summary>
        /// Translate a SQL data type to a .NET type
        /// </summary>
        /// <param name="sqlDataTypeName"></param>
        /// <returns></returns>
        public static string GetDotNetDataType(string sqlDataTypeName, bool nullable = false)
        {
            switch (sqlDataTypeName.ToLower())
            {
                case "bigint":
                    return "Int64" + (nullable ? "?" : "");
                case "binary":
                case "image":
                case "varbinary":
                    return "byte[]";
                case "bit":
                    return "bool" + (nullable ? "?" : "");
                case "char":
                    return "char" + (nullable ? "?" : "");
                case "datetime":
                case "smalldatetime":
                    return "DateTime" + (nullable ? "?" : "");
                case "decimal":
                case "money":
                case "numeric":
                    return "decimal" + (nullable ? "?" : "");
                case "float":
                    return "double" + (nullable ? "?" : "");
                case "int":
                    return "int" + (nullable ? "?" : "");
                case "nchar":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return "string";
                case "real":
                    return "single" + (nullable ? "?" : "");
                case "smallint":
                    return "Int16" + (nullable ? "?" : "");
                case "tinyint":
                    return "byte" + (nullable ? "?" : "");
                case "uniqueidentifier":
                    return "Guid" + (nullable ? "?" : "");
                case "date":
                    return "DateTime" + (nullable ? "?" : "");

                default:
                    return null;
            }
        }
        

        /// <summary>
        /// Get the data type and its size (if applicable) from a table column 
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string GetColumnSqlDataType(this TSqlObject column, bool withLength = true)
        {
            SqlDataType sdt = column.GetReferenced(Column.DataType).First().GetProperty<SqlDataType>(DataType.SqlDataType);
            if (withLength)
            {
                int length = column.GetProperty<int>(Column.Length);
                bool isMax = column.GetProperty<bool>(Column.IsMax);
                if (length == 0 && !isMax) return sdt.ToString().ToUpper();
                else if (length == 0) return sdt.ToString().ToUpper() + "(MAX)";
                else return sdt.ToString().ToUpper() + "(" + length + ")";
            }
            else
            {
                return sdt.ToString().ToUpper();
            }
        }


        /// <summary>
        /// Get primary key column(s) from a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<TSqlObject> GetPrimaryKeyColumns(this TSqlObject table)
        {
            List<TSqlObject> pkColumns = new List<TSqlObject>();

            if (table != null)
            {
                TSqlObject pk = table.GetReferencing(PrimaryKeyConstraint.Host, DacQueryScopes.UserDefined).FirstOrDefault();
                if (pk != null)
                {
                    var columns = pk.GetReferenced(PrimaryKeyConstraint.Columns);
                    if (columns != null)
                    {
                        pkColumns.AddRange(columns);
                    }
                }
            }
            return pkColumns;
        }


        public static bool IsColumnNullable(this TSqlObject column)
        {
            bool result = column.GetProperty<bool>(Column.Nullable);
            return result;
        }

        
        /// <summary>
        /// Transforms a string in the form 'foo_bar' into a string in the form 'FooBar'
        /// </summary>
        /// <param name="the_string"></param>
        /// <returns></returns>
        public static string PascalCase(this string the_string)
        {
            // If there are 0 or 1 characters, just return the string.
            if (the_string == null) return the_string;
            if (the_string.Length < 2) return the_string.ToUpper();

            // Split the string into words.
            string[] words = the_string.Split(
                new char[] { '_' },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = "";
            foreach (string word in words)
            {
                result +=
                    word.Substring(0, 1).ToUpper() +
                    word.Substring(1);
            }

            return result;
        }


    }
}

