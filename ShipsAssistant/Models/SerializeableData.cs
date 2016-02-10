using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipsAssistant.Models
{
    //not sure how to make this base class not  seem weird
    class SerializeableData<T>
    {
        private string _nameFormat = @"{0}_data.json";
        //private T _data = default(T); 
        public T Data { get; set; }
        
        private string DataPath { get; set; }
        private string Name { get; set; }
        private string DataFileName {
            get
            {
                DataCreatedAt = DateTime.Now.ToUniversalTime();
                string dataFileName;
                if (!string.IsNullOrEmpty(DataPath))
                {
                    return Path.Combine(DataPath, Name);
                }
                else
                {
                    return Name; 
                }
            }
            set { }
        }
        DateTime DataCreatedAt { get; set; }


        public SerializeableData(string name,  string path = "")
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new Exception("Name required for serialization");
            }
            Name = name;
            DataPath = path; 
        }

        public void Serialize()
        {
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(DataFileName, json);
        }

        public void Deserialize()
        {
            T data; 
            if(TryDeserialize(out data))
            {
                Data = data; 
            }
        }

        public bool TryDeserialize(out T results)
        {
            bool parsed = false;
            if (!File.Exists(DataFileName))
            {
                results = default(T);             
            }
            try
            {
                string json = File.ReadAllText(DataFileName);
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
