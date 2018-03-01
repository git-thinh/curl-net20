using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace curl
{
    public static class rest
    {
        private static IDictionary<string, IDB> dicDB = new Dictionary<string, IDB>() { };

        public static void Load() {

        }

        public static string Query(string paraJson)
        {
            string json = "{}";
            Console.WriteLine(paraJson);

            var it = JsonConvert.DeserializeObject<JObject>(paraJson);

            string model = GetValue(it, "model");
            string action = GetValue(it, "action");
            string data = GetValue(it, "data", true);
                        
            switch (action)
            {
                case "create":
                    json = _create(model, it, paraJson, data);
                    break;
                case "insert":
                    break;
                case "select":
                    break;
            }
            
            return json;
        }

        // { "model":"test", "action":"create", "indexes":"key", "data":[{"key":"value1", "key2":"tiếng việt"}] }
        private static string _create(string model, JObject it, string jsonPara, string data)
        {
            string json = "{}";

            string indexes = GetValue(it, "indexes");

            IDB db = new dbLite();
            db.Init(model);
            dicDB.Add(model, db);

            return json;
        }

        private static string _insert()
        {
            string json = "{}";
            return json;
        }

        private static string _select()
        {
            string json = "{}";
            return json;
        }

        private static string GetValue(JObject it, string name, bool parseJson = false)
        {
            string val = "";
            JProperty pro = it.Properties().Where(i => i.Name == name).SingleOrDefault();
            if (pro != null)
            {
                if (parseJson)
                {
                    val = JsonConvert.SerializeObject(pro.Value);
                }
                else
                    val = (string)pro.Value;
            }
            return val;
        }

    }
}
