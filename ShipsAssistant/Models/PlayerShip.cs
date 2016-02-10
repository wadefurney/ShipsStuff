using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipsApi.Models
{
    class PlayerShip: Ship
    {
        //Contains stuff specific to a player owned ship

        public List<Module> OwnedModules { get; set; }
        public bool IsElite { get; set; }

        /*
        public int ExperiencePercentage
        {
            get
            {
                if (IsPremium || ExperienceMax == 0)
                {
                    return 100;
                }
                else
                {
                    float exp = ((float)Experience / (float)ExperienceMax);
                    if (exp > 1f)
                    {
                        return 100;
                    }
                    else
                    {
                        return (int)(exp * 100);
                    }
                }
            }
            set { }
        }
        */

            /*
        public PlayerShip(string id, JObject data) : base(id, data)
        {
            if (data["experience"] != null)
                Experience = Convert.ToInt32(data["experience"].ToString());
        }
        */

    }
}
