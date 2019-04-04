using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator
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
                LoadAsScriptBackedModel = false,
                ModelStorageType = DacSchemaModelStorageType.File
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
        /// Get all roles from a model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="excludeSysSchema"></param>
        /// <returns></returns>
        public static IEnumerable<TSqlObject> GetAllRoles(this TSqlModel model, bool excludeSysSchema = true)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var roles = model.GetObjects(DacQueryScopes.All, ModelSchema.Role);
            if (roles != null)
            {
                if (excludeSysSchema)
                    roles = roles.Where(curRole => !curRole.Name.Parts[0].ToLower().Contains("db_") && curRole.Name.Parts[0].ToLower() != "public");
            }
            return roles;
        }


        /// <summary>
        /// Get all columns from a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<TSqlObject> GetAllColumns(this TSqlObject table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            var columns = table.GetReferenced(Table.Columns);
            return columns;
        }


        /// <summary>
        /// Translate a SQL data type to a .NET type
        /// </summary>
        /// <param name="sqlDataTypeName"></param>
        /// <returns></returns>
        public static string GetDotNetDataType_SystemDataSqlTypes(string sqlDataTypeName)
        {
            if (sqlDataTypeName == null) throw new ArgumentNullException(nameof(sqlDataTypeName));
            switch (sqlDataTypeName.ToLower())
            {
                case "bigint":
                    return "SqlInt64";
                case "binary":
                case "image":
                case "varbinary":
                    return "SqlBinary";
                case "bit":
                    return "SqlBoolean";
                case "char":
                    return "SqlString";
                case "datetime":
                case "smalldatetime":
                    return "SqlDateTime";
                case "decimal":
                case "money":
                case "numeric":
                    return "SqlDecimal";
                case "float":
                    return "SqlDouble";
                case "int":
                    return "SqlInt32";
                case "nchar":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return "SqlString";
                case "real":
                    return "SqlSingle";
                case "smallint":
                    return "SqlInt16";
                case "tinyint":
                    return "SqlByte";
                case "uniqueidentifier":
                    return "SqlGuid";
                case "date":
                    return "SqlDateTime";

                default:
                    return null;
            }
        }

        /// <summary>
        /// Translate a SQL data type to a .NET type
        /// </summary>
        /// <param name="sqlDataTypeName"></param>
        /// <returns></returns>
        public static string GetDotNetDataType_SystemDataDbTypes(string sqlDataTypeName)
        {
            if (sqlDataTypeName == null) throw new ArgumentNullException(nameof(sqlDataTypeName));
            switch (sqlDataTypeName.ToLower())
            {
                case "bigint":
                    return "DbType.Int64"; 
                case "binary":
                case "image":
                case "varbinary":
                    return "DbType.Binary";
                case "bit":
                    return "DbType.Boolean";
                case "char":
                    return "DbType.String";
                case "datetime":
                case "smalldatetime":
                    return "DbType.DateTime";
                case "decimal":
                case "money":
                case "numeric":
                    return "DbType.Decimal";
                case "float":
                    return "DbType.Double";
                case "int":
                    return "DbType.Int32";
                case "nchar":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return "DbType.String";
                case "real":
                    return "DbType.Single";
                case "smallint":
                    return "DbType.Int16";
                case "tinyint":
                    return "DbType.Byte";
                case "uniqueidentifier":
                    return "DbType.Guid";
                case "date":
                    return "DbType.DateTime";

                default:
                    return null;
            }
        }


        /// <summary>
        /// Translate a SQL data type to a .NET type
        /// </summary>
        /// <param name="sqlDataTypeName"></param>
        /// <returns></returns>
        public static string GetDotNetDataType(string sqlDataTypeName, bool nullable = false)
        {
            if (sqlDataTypeName == null) throw new ArgumentNullException(nameof(sqlDataTypeName));
            switch (sqlDataTypeName.ToLower())
            {
                case "bigint":
                    return "long" + (nullable ? "?" : "");
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
                    return "System.DateTime" + (nullable ? "?" : "");
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
                    return "short" + (nullable ? "?" : "");
                case "tinyint":
                    return "byte" + (nullable ? "?" : "");
                case "uniqueidentifier":
                    return "System.Guid" + (nullable ? "?" : "");
                case "date":
                    return "System.DateTime" + (nullable ? "?" : "");

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
            if (column == null) throw new ArgumentNullException(nameof(column));
            SqlDataType sdt = column.GetReferenced(Column.DataType).First().GetProperty<SqlDataType>(DataType.SqlDataType);
            if (withLength)
            {
                int length = column.GetProperty<int>(Column.Length);
                bool isMax = column.GetProperty<bool>(Column.IsMax);
                return length == 0 && !isMax
                    ? sdt.ToString().ToUpper()
                    : length == 0 ? sdt.ToString().ToUpper() + "(MAX)" : sdt.ToString().ToUpper() + "(" + length + ")";
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
            if (table == null) throw new ArgumentNullException(nameof(table));
            TSqlObject pk = table.GetReferencing(PrimaryKeyConstraint.Host, DacQueryScopes.UserDefined).FirstOrDefault();
            if (pk != null)
            {
                var columns = pk.GetReferenced(PrimaryKeyConstraint.Columns);
                if (columns != null)
                {
                    return columns;
                }
            }
            return new TSqlObject[0];
        }

        /// <summary>
        /// Get uk(s) with attached column(s) from a table. Based on unique constrains finding
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<TSqlObject>> GetUniqueKeysWithColumns(this TSqlObject table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            IEnumerable<TSqlObject> uks = table.GetReferencing(UniqueConstraint.Host, DacQueryScopes.UserDefined);
 
            if (uks != null)
            {
                foreach (var uk in uks)
                {
                    var columns = uk.GetReferenced(UniqueConstraint.Columns);

                    if (columns != null)
                    {
                        yield return columns;
                    }
                }
            }

        }

        
        /// <summary>
        /// Check if a column is nullable
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool IsColumnNullable(this TSqlObject column)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            bool result = column.GetProperty<bool>(Column.Nullable);
            return result;
        }

        /// <summary>
        /// Check if a column is identity
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool IsColumnIdentity(this TSqlObject column)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            bool result = column.GetProperty<bool>(Column.IsIdentity);
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

