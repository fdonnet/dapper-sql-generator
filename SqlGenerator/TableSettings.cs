using Newtonsoft.Json;
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

    /// <summary>
    /// Extension to clone global settings in table settings 
    /// (return a new table settings object filled with global settings info) 
    /// </summary>
    public static class TableSettingsExt
    {
        /// <summary>
        /// Deep clone via JSON => LOL
        /// </summary>
        /// <param name="tableSettings"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        public static TableSettings CopyFromGlobalSettings(this TableSettings tableSettings, Settings globalSettings)
        {
            string tmp = JsonConvert.SerializeObject(globalSettings);
            return JsonConvert.DeserializeObject<TableSettings>(tmp);
        }
    }
}
