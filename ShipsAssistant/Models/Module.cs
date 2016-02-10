using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipsApi.Models
{
    class Module
    {
        //remove these two properties when UI can display stuff from ModuleAttributes
        public decimal Range { get; set; }
        public decimal Speed { get; set; }

        public long ID { get; set; }
        public string Type { get; set; }
        public String Name { get; set; }
        //might not ever use this price but i guess store it just in case; 
        public int PriceCredits { get; set; }
        public Dictionary<String, Decimal> ModuleAttributes { get; set; }


        public Module()
        {
            ModuleAttributes = new Dictionary<string, decimal>();
        }



        public Module(JToken moduleToken)
        {
            ModuleAttributes = new Dictionary<string, decimal>();

            this.ID = Convert.ToInt64(moduleToken["module_id"]);
            this.Type = moduleToken["type"].ToString();
            this.Name = moduleToken["name"].ToString();
            this.PriceCredits = Convert.ToInt32(moduleToken["price_credit"].ToString());
            foreach (JProperty prop in moduleToken["profile"][this.Type])
            {
                decimal val;
                if (decimal.TryParse(prop.Value.ToString(), out val))
                {
                    this.ModuleAttributes[prop.Name] = val;
                }
            }
            //remove this switch statement when possible, all attributes should come from dictionary
            switch (this.Type)
            {
                case "fire_control":
                case "torpedoes":
                    this.Range = this.ModuleAttributes["distance"];
                    break;
                case "engine":
                    this.Speed = this.ModuleAttributes["max_speed"];
                    break;
                default:
                    break;
            }
        }
    }
}
