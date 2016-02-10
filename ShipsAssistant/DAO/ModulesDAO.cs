using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShipsApi.Models;
using ShipsAssistant.DAO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ShipsApi.DAO
{
    //Modules are currently only loaded when ships are reloaded from the api


    class ModulesData
    {
        public DateTime DataCreatedAt { get; set; }
        public Dictionary<long, Module> AllModules { get; set; }
        public List<string> AllTypes { get; set; }
        public List<string> AllModuleProperties { get; set; }
        public ModulesData() { }

        //deserializes json returned by api 
        //should i use this? 
        public static ModulesData FromJson(string json)
        {
            ModulesData results = new ModulesData();

            return results; 
        }
    }

    class ModulesDAO
    {
        private string dataFileName = @"modles.data";

        #region properties

        private ModulesData _data = null;
        private ModulesData Data
        {
            get
            {
                if (_data == null)
                {                    
                    _data = LoadData(true);                    
                    if (_data == null)
                    {
                        throw new Exception("Can't load module data");
                    }
                    else
                    {
                        SerializeModules();
                    }
                }
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        public List<Module> AllModules
        {
            get
            {
                return Data.AllModules.Values.OrderBy(x => x.Name).ToList();
            }
            private set { }
        }

        public Dictionary<long,  Module> Modules
        {
            get
            {
                return Data.AllModules;
            }
            private set { }
        }
		
        public List<String> AllTypes
        {
            get
            {
                return Data.AllTypes;
            }
            private set { }
        }

        #endregion
        //Modules are currently only loaded when ships are reloaded from the api
        public ModulesDAO ()
        {
            Console.WriteLine(); 
        }
                
        private ModulesData LoadData(bool forceApi = false)
        {
            ModulesData results = null;
            if (!forceApi)
            {
                results = DeserializeModules();
            }
            //set range for max data age
            if (forceApi ||
               results == null ||
               results.DataCreatedAt == DateTime.MaxValue)
            {

                /*
                //put this somewhere safe sometime
                string key = @"11749197d5c8ca823ed4beb2199922aa";
                ApiDAO api = new ApiDAO(key);
                */


                //fix
                //move to apidao
                results = GetModules(); 
           
            }
            return results;
        }

        #region old shit to remove


        //add some stuff to config files later
        //get this list from https://api.worldofwarships.com/wows/encyclopedia/info/?application_id=demo

        private static List<string> ModuleTypes = new List<string> {
            "engine",
            "torpedo_bomber",
            "fighter",
            "hull",
            "artillery",
            "torpedoes",
            "fire_control",
            "flight_control",
            "dive_bomber" };

        private static string ModuleDirectory = @"D:\projects\tmp\"; 
        private static string ModuleFileFormat = @"{0}_modules.json";

        #endregion
        
        private ModulesData GetModules()
        {
            ModulesData results = new ModulesData();

            results.AllModules = new Dictionary<long, Module>();
            string key = File.ReadAllText("api.secret");// @"11749197d5c8ca823ed4beb2199922aa";
            ApiDAO api = new ApiDAO(key);
            List<Module> typeResults = new List<Module>();
            foreach ( string type in ModuleTypes)
            {                
                string json = api.GetModules(type);
                typeResults = GetModules(json);
                foreach(Module m in typeResults)
                {
                    results.AllModules[m.ID] = m;
                }
            }
            results.AllTypes = ModuleTypes;
            results.DataCreatedAt = DateTime.Now.ToUniversalTime();
            HashSet<String> propNames = new HashSet<string>(); 
            foreach(Module m in results.AllModules.Values)
            {
                foreach(string name in m.ModuleAttributes.Keys)
                {
                    if(!propNames.Contains(name))
                    {
                        propNames.Add(name); 
                    }
                }
            }

            results.AllModuleProperties = propNames.ToList(); 
            return results; 
        }

        private List<Module> GetModules(String json)
        {
            List<Module> results = new List<Module>();
            JObject parsedJson = JObject.Parse(json);
            Dictionary<string, JObject> modules = parsedJson["data"].ToObject<Dictionary<string, JObject>>();

            foreach(JToken m in parsedJson["data"].Values())
            {
                Module module = new Module(m);
                results.Add(module);                
            }           
                       
            return results;
        }

        #region Serialization

        private void SerializeModules()
        {
            if (Data.DataCreatedAt.Equals(DateTime.MinValue))
            {
                Data.DataCreatedAt = DateTime.Now.ToUniversalTime();
            }
            string json = JsonConvert.SerializeObject(Data);
            File.WriteAllText(dataFileName, json);
        }

        private ModulesData DeserializeModules(string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                filePath = dataFileName;
            }
            if (!File.Exists(filePath))
            {
                return null;
            }
            ModulesData results = new ModulesData();
            string json = File.ReadAllText(filePath);
            results = JsonConvert.DeserializeObject<ModulesData>(json);
            return results;
        }

        #endregion
        
    }
}
