using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
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
    class ShipsData
    {
        public DateTime DataCreatedAt { get; set; }
        public Dictionary<long, Ship> AllShips { get; set; }
        public List<String> AllTiers { get; set; }
        public List<String> AllTypes { get; set; }
        public List<String> AllNations { get; set; }
        public ShipsData() { }
    }

    //load shit from api, persist on disk, read from disk if it exists
    class ShipsDAO
    {
        //by default serialization is done in program directory
        private string dataFileName = @"ships.data";

        #region Properties
         
        private ShipsData _data = null; 
        private ShipsData Data
        {
            get
            {
                if(_data == null)
                {
                    _data = LoadData(true);
                    if (_data == null )
                    {
                        throw new Exception("Can't load ship data");
                    }
                    else
                    {
                        SerializeShips();
                    }
                }
                return _data; 
            }
            set
            {
                _data = value; 
            }
        }       

        public List<Ship> AllShips
        {
            get
            {
                return Data.AllShips.Values.OrderBy(x => x.Name).ToList();
            }
            private set { }
        }

        public List<String> AllTiers
        {
            get
            {
                return Data.AllTiers;
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

        public List<String> AllNations
        {
            get
            {
                return Data.AllNations;
            }
            private set { }
        }

        public Dictionary<long, Ship> Ships
        {
            get
            {
                return Data.AllShips;
            }
            private set { }
        }
       
        #endregion

        public ShipsDAO() { }
                
        private ShipsData LoadData(bool forceApi = false)        
        {
            ShipsData results = null;
            if (!forceApi)
            {
                results = DeserializeShips(); 
            }
            //set range for max data age
            if (forceApi ||
               results == null ||
               results.DataCreatedAt == DateTime.MaxValue)
            {
                //put this somewhere safe sometime
                string key = File.ReadAllText("api.secret"); 
                ApiDAO api = new ApiDAO(key);
                results = api.GetAllShips();                               
                   
            } 
            return results; 
        }

        #region Serialization

        private void SerializeShips()
        {
            if(Data.DataCreatedAt.Equals(DateTime.MinValue))
            {
                Data.DataCreatedAt = DateTime.Now.ToUniversalTime();
            }
           
            string json = JsonConvert.SerializeObject(Data);
            File.WriteAllText(dataFileName, json);
           
        }

        private ShipsData DeserializeShips(string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                filePath = dataFileName;
            }
            if (!File.Exists(filePath))
            {
                return null;
            }
            ShipsData results = new ShipsData();
            string json = File.ReadAllText(filePath);
            results = JsonConvert.DeserializeObject<ShipsData>(json);
            return results;
        }

        #endregion
       
        public Ship GetShip(string id)
        {
            long tmp = Convert.ToInt64(id);
            return GetShip(tmp); 
        }
        
        public Ship GetShip(long id)
        {
            return Ships[id]; 
        }
               
    }
}
