using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ShipsApi.Models
{
    class Ship
    {
        public bool IsPremium { get; set; }
        public long ID { get; set; }
        public String Name { get; set; }
        public String Nation { get; set; }
        public String Type { get; set; }
        public int Tier { get; set; }
        //Add to player ship data
        //public int Experience { get; set; }
        public List<Module> Modules { get; set; }
        //total amount of experience needed to level up (derrived property)
        public int ExperienceMax { get; set; }
        public List<long> NextShipIDs { get; set; }

        //new properties to integrate
        public int PriceCredits { get; set; }
        public List<long> ModuleIDs { get; set; }
        public int PriceGold { get; set; }
        public Dictionary<String, Dictionary<String, decimal>> ShipAttributes { get; set; }

        public Dictionary<long, int> NextShips { get; set; }

        //delete these and get data from dictionary 
        public Decimal ArtilleryMin { get; set; }
        public Decimal ArtilleryMax { get; set; }
        public Decimal SpeedMin { get; set; }
        public Decimal SpeedMax { get; set; }
        public Decimal TorpRangeMax { get; set; }
        public Decimal TorpRangeMin { get; set; }
        public Decimal AARangeMax { get; set; }
        public Decimal AARangeMin { get; set; }
                
        public Ship()
        {
            ShipAttributes = new Dictionary<String, Dictionary<String, decimal>>(); 
        }

        public Ship(String id, JToken shipToken)
        {
            ShipAttributes = new Dictionary<String, Dictionary<String, decimal>>();

            IsPremium = bool.Parse(shipToken["is_premium"].ToString());
            ID = Convert.ToInt64(id);
            Name = shipToken["name"].ToString(); 
            Nation = shipToken["nation"].ToString();
            Type = shipToken["type"].ToString();
            Tier = int.Parse(shipToken["tier"].ToString());
            PriceCredits = int.Parse(shipToken["price_credit"].ToString());
            PriceGold = int.Parse(shipToken["price_gold"].ToString());
            
            ModuleIDs = new List<long>(); 
            if (shipToken["modules"].Count() > 0)
            {
                Dictionary<string, JArray> modules = shipToken["modules"].ToObject<Dictionary<string, JArray>>();
                foreach (var key in modules.Keys)
                {
                    foreach (var module in modules[key])
                    {
                        ModuleIDs.Add(Convert.ToInt64(module));                        
                    }                    
                }                
            }

            ShipAttributes = new Dictionary<String, Dictionary<String, decimal>>();
            foreach (JProperty category in shipToken["default_profile"])
            {
                Dictionary<string, decimal> attributes = GetCategoryAttributes(category);
                ShipAttributes.Add(category.Name, attributes);
            }

            //following mainly used for determining ship progression
            ExperienceMax = 0;
            NextShips = new Dictionary<long, int>(); 
            if (shipToken["next_ships"].Count() > 0)
            {
                Dictionary<string, string> ships = shipToken["next_ships"].ToObject<Dictionary<string, string>>();
                foreach (var key in ships.Keys)
                {                    
                    long nextId = Convert.ToInt64(key);
                    int exp = Convert.ToInt32(ships[key].ToString());
                    NextShips[nextId] = exp;                     
                    ExperienceMax += exp;
                }                
            }

            if (shipToken["modules_tree"].Count() > 0)
            {
                foreach (var n in shipToken["modules_tree"])
                {
                    int exp = Convert.ToInt32(n.First()["price_xp"].ToString());
                    ExperienceMax += exp;                   
                }
            }



            Console.WriteLine();
        }


        private Dictionary<string, decimal> GetCategoryAttributes(JProperty category, string namePrefix = "")
        {
            Dictionary<String, decimal> results = new Dictionary<string, decimal>();
            foreach (JProperty attribute in category.Value)
            {
                string attrName = attribute.Name;
                if (!String.IsNullOrEmpty(namePrefix))
                {
                    attrName = String.Format("{0}-{1}", namePrefix, attrName);
                }
                //has sub items
                if (attribute.Value.Count() > 0)
                {
                    Dictionary<string, decimal> subAttirubtes = GetCategoryAttributes(attribute, attrName);
                    foreach (string key in subAttirubtes.Keys)
                    {
                        results.Add(key, subAttirubtes[key]);
                    }
                }
                else
                {
                    decimal value;
                    if (!attribute.Name.Contains("_id") &&
                        decimal.TryParse(attribute.Value.ToString(), out value))
                    {
                        results.Add(attrName, value);
                    }
                }
            }
            return results;
        }


        public Ship(string id, JObject data)
        {
            ShipAttributes = new Dictionary<String,Dictionary<string, decimal>>();

            NextShipIDs = new List<long>();

            ExperienceMax = 0;
            IsPremium = Convert.ToBoolean(data["is_premium"]);
            if(!IsPremium)
            {
                CalculateExperience(data); 
            }
            Modules = new List<Module>();
            AddModules(data);



            Type = data["type"].ToString();
            
            ID = Convert.ToInt64(id); 
            Tier = Convert.ToInt32(data["tier"].ToString());

            Name = data["name"].ToString();
            Nation = data["nation"].ToString();            
        }


        private void AddModules(JObject data)
        {            
            if (data["modules"].Count() > 0)
            {
                Dictionary<string, JArray> modules = data["modules"].ToObject<Dictionary<string, JArray>>();
                
                foreach (var key in modules.Keys)
                {
                    foreach(var module in modules[key])
                    {
                        Module m = new Module();
                        m.ID = Convert.ToInt64(module);
                        m.Type = key;
                        Modules.Add(m);
                    }
                    //Console.WriteLine(key + " " + modules[key]);
                }
                /*
                foreach (var n in data["modules_tree"])
                {
                    Module m = new Module();
                    m.ID = Convert.ToInt64(n.First()["module_id"].ToString());
                    m.Type = n.First()["type"].ToString();
                    Modules.Add(m);                 
                }
                */
            }
        }



        private void CalculateExperience(JObject data)
        {
            if (data["next_ships"].Count() > 0)
            {

                Dictionary<string, string> ships = data["next_ships"].ToObject<Dictionary<string, string>>();

                foreach (var key in ships.Keys)
                {
                    //Console.WriteLine(key + " " + ships[key]);
                    long id = Convert.ToInt64(key);
                    NextShipIDs.Add(id); 
                    int exp = Convert.ToInt32(ships[key].ToString());
                    ExperienceMax += exp;
                }

                foreach (var n in data["next_ships"])
                {
                    
                }
            }

            if (data["modules_tree"].Count() > 0)
            {
                foreach (var n in data["modules_tree"])
                {
                    int exp = Convert.ToInt32(n.First()["price_xp"].ToString());
                    ExperienceMax += exp; 
                    //int exp = Convert.ToInt32(n.First().ToString());
                    //ExperienceMax += exp;
                }
            }

        }
    }
}
