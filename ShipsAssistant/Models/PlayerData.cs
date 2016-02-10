using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipsApi.Models
{
    class PlayerData
    {
        //store stuff like list of player owned ships here
        private static List<PlayerShip> SetEliteShips(List<PlayerShip> playerShips)
        {
            List<PlayerShip> results = new List<PlayerShip>();

            HashSet<long> ownedShips = new HashSet<long>(playerShips.Select(x => x.ID));


            bool ownsAll;
            foreach (PlayerShip ship in playerShips)
            {
                ownsAll = true;
                foreach (long id in ship.NextShipIDs)
                {
                    ownsAll = ownsAll && ownedShips.Contains(id);
                }
                if (ownsAll)
                {
                    ship.IsElite = true;
                }
                else
                {
                    ship.IsElite = false;
                }
                results.Add(ship);
            }

            return results;
        }

        //get player's ships
        private static List<Ship> GetShips(long playerId)
        {
            string appKey = @"11749197d5c8ca823ed4beb2199922aa";
            string url = @"https://api.worldofwarships.com/wows/ships/stats/";
            string.Format(url, appKey);
            var restClient = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddParameter("application_id", appKey);
            request.AddParameter("account_id", playerId.ToString());
            request.AddParameter("in_garage", "1");
            var response = restClient.Execute(request);
            string tmp = response.Content;


            //File.WriteAllText(@"D:\projects\tmp\ships.json", response.Content);

            List<Ship> results = new List<Ship>();
            //String tmp = File.ReadAllText(@"D:\projects\tmp\zigships.json");

            JObject d = JObject.Parse(tmp);
            //Dictionary<string, JObject> ships = d["data"].First()[0].First().ToObject<Dictionary<string, JObject>>();

            foreach (var s in d["data"].First().First())
            {
                try
                {
                    Ship ship = new Ship();  ;// = GetShip(s["ship_id"].ToString());
                    //ship.Experience = Convert.ToInt32(s["pvp"]["xp"].ToString());
                    results.Add(ship);
                }
                catch
                { }
            }
            return results;
        }





    }
}
