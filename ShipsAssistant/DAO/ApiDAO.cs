using Newtonsoft.Json.Linq;
using RestSharp;
using ShipsApi.DAO;
using ShipsApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipsAssistant.DAO
{
    //put serializable shit in here
    //maybe make a better data encapsulation class with generic types and its own serialize and deserialize static methods? 
    //make this class static in case multiple apis are used, better yet, make it so only one api is used
    class ApiData
    {
        public String Version { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string> ModuleTypes { get; set; }
        public ApiData() { }
    }


    //make this only return json strings and make each DAO create the objects 
    class ApiDAO
    {
        private ApiData Data { get; set; }

        string _baseUrl = @"https://api.worldofwarships.com/wows";
        private string ApiKey { get; set; }
        private RestClient Client { get; set; }

        public ApiDAO(string apiKey)
        {
            if(String.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Api key required to make api calls");
            }
            ApiKey = apiKey;
            Client = new RestClient(_baseUrl);

            CheckForUpdate(); 
        }

        private void CheckForUpdate()
        {            
            ApiData fresh = GetApiDetails();

            if(fresh != null)
            {
                Data = fresh;                
            }
            else
            {
                Console.WriteLine("Offline!!! Fuck comcast!");
            }
          
            //compare fresh to deserialized shit 
            Console.WriteLine();             
        }

        //maybe be more than one useful endpoint to get updated details
        private ApiData GetApiDetails()
        {
            return null; 
            //move this shit to constants 
            string UPDATED = "ships_updated_at";
            string VERSION = "game_version";
            string MODULE_TYPES = "ship_modules";


            ApiData results = new ApiData(); 
            string endPoint = @"/encyclopedia/info/";
            var request = new RestRequest(endPoint, Method.GET);
            request.AddParameter("application_id", ApiKey);

            IRestResponse response;
            try
            {
                response = Client.Execute(request);
            }
            catch
            {
                return null; 
            }
            
            string json = response.Content;
            JObject parsedJson = JObject.Parse(json);
            var apiData = parsedJson["data"];
                       
            long epochSeconds = Convert.ToInt64(apiData[UPDATED]);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            results.LastUpdated = epoch.AddSeconds(epochSeconds);
            results.Version = apiData[VERSION].ToString();
            results.ModuleTypes = new List<string>();
            
            foreach(JProperty t in apiData[MODULE_TYPES])
            {
                results.ModuleTypes.Add(t.Name);                 
            }

            return results; 
        }


        #region Ships

        //add error handling 
        public ShipsData GetAllShips()
        {
            string endPoint = @"encyclopedia/ships/";            
            var request = new RestRequest(endPoint, Method.GET);
            request.AddParameter("application_id", ApiKey);
            var response = Client.Execute(request);
            string json = response.Content;
        
            return GetAllShips(json);
        }
        
        private ShipsData GetAllShips(string json)
        {            
            ShipsData results = new ShipsData();

            JObject d = JObject.Parse(json);

            Dictionary<string, JToken> ships2 = d["data"].ToObject<Dictionary<string, JToken>>();

            
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            foreach (string key in ships2.Keys)
            {
                Ship ship = new Ship(key, ships2[key]);
                Console.WriteLine("CheckShip");
            }
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////            


            Dictionary<string, JObject> ships = d["data"].ToObject<Dictionary<string, JObject>>();

            HashSet<String> allTiers = new HashSet<string>();
            HashSet<String> allTypes = new HashSet<string>();
            HashSet<String> allNations = new HashSet<string>();
            Dictionary<long, Ship> allShips = new Dictionary<long, Ship>();

            foreach (var key in ships.Keys)
            {
                Ship ship = new Ship(key, ships[key]);
                ship = SetShipAttributes(ship);
                allShips[ship.ID] = ship;
                if (!allTiers.Contains(ship.Tier.ToString()))
                    allTiers.Add(ship.Tier.ToString());
                if (!allTypes.Contains(ship.Type))
                    allTypes.Add(ship.Type);
                if (!allNations.Contains(ship.Nation))
                    allNations.Add(ship.Nation);

            }
            results.AllShips = allShips; 
            results.AllNations = allNations.ToList();
            results.AllNations.Sort(); 
            results.AllTypes = allTypes.ToList();
            results.AllTypes.Sort(); 
            results.AllTiers = allTiers.ToList();
            results.AllTiers.Sort();
            results.DataCreatedAt = DateTime.Now.ToUniversalTime(); 
            return results;
        }
        
        //this method needs work, it also might not belong here..... 
        private Ship SetShipAttributes(Ship ship)
        {
            Dictionary<long, Module> modules = Modules.Modules;
            ship.ArtilleryMax = 0;
            ship.ArtilleryMin = 0;
            decimal minRange = int.MaxValue;
            decimal maxRange = int.MinValue;
            decimal minSpeed = int.MaxValue;
            decimal maxSpeed = int.MinValue;
            decimal maxTorpRange = int.MinValue;

            //need to relode modules, looks like there's new ships ???? 



            foreach (Module module in ship.Modules)
            {
                if (module.Type.Equals("fire_control"))
                {
                    Module m = modules[module.ID];
                    //Console.WriteLine(module.ID + " " + m.Range);
                    minRange = Math.Min(minRange, m.Range);
                    maxRange = Math.Max(maxRange, m.Range);
                }
                else if (module.Type.Equals("engine"))
                {
                    Module m = modules[module.ID];
                    //Console.WriteLine(module.ID + " " + m.Range);
                    minSpeed = Math.Min(minSpeed, m.Speed);
                    maxSpeed = Math.Max(maxSpeed, m.Speed);
                }
                else if (module.Type.Equals("torpedoes"))
                {
                    Module m = modules[module.ID];
                    //Console.WriteLine(module.ID + " " + m.Range);
                    maxTorpRange = Math.Max(maxTorpRange, m.Range);
                }


            }
            if (maxRange != int.MinValue)
                ship.ArtilleryMax = maxRange;
            if (minRange != int.MaxValue)
                ship.ArtilleryMin = minRange;

            if (maxTorpRange != int.MinValue)
                ship.TorpRangeMax = maxTorpRange;
            else
                ship.TorpRangeMax = 0;




            ship.SpeedMax = maxSpeed;
            ship.SpeedMin = minSpeed;

            return ship;
        }
        
        #endregion


        #region Modules

        //move this somewhere else
        private ModulesDAO _modules = null;
        private ModulesDAO Modules
        {
            get
            {
                if (_modules == null)
                {
                    _modules = new ModulesDAO();
                }
                return _modules;
            }
            set { }
        }

        //type parameter requires by api
        public string GetModules(String moduleType)
        {
            string endPoint = @"encyclopedia/modules/";
            var request = new RestRequest(endPoint, Method.GET);
            request.AddParameter("application_id", ApiKey);
            request.AddParameter("type", moduleType);
            var response = Client.Execute(request);
            string json = response.Content;

            return json;
        }

        public ModulesData GetAllModules()
        {
            ModulesData results = new ModulesData();

            string typeBatch;
            foreach (string typeName in Data.ModuleTypes)
            {
                typeBatch = GetModules(typeName);
            }

            throw new NotImplementedException();


            return results;
        }



        #endregion

    }
}
