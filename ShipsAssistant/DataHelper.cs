using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipsAssistant
{
    public static class DataHelper
    {
        //organize?
        public static class FileMetaData
        {

            public static string SHIPS = "ships_data.json";

        }

        public static void Serialize<T>(string  filePath, T data)
        {
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText(filePath, json);
        }
                
        public static bool TryDeserialize<T>(string filePath, out T results)
        {
            bool parsed = false;
            if (!File.Exists(filePath))
            {
                results = default(T);
            }
            try
            {
                string json = File.ReadAllText(filePath);
                results = JsonConvert.DeserializeObject<T>(json);
                parsed = true;
            }
            catch
            {
                results = default(T);
            }
            return parsed;
        }
    }
}
