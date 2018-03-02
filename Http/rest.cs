using System.Linq;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace curl
{
    public class Rest
    {
        // { "model":"test", "action":"create", "indexes":"key", "data":[{"key":"value1", "key2":"tiếng việt"}] }
        public static string create(message m)
        {
            string json = "{}";
            string indexes = m.jobject.getValue("indexes");

            bool isOk = dbi.Create(m.model);

            if (isOk && !string.IsNullOrEmpty(m.input))
                json = insert(new message() { action = "insert", model = m.model, input = m.input });
            else
                json = @"{""ok"":true,""total"":0,""output"":""Create database " + m.model + @" successfully.""}";

            return json;
        }

        // { "model":"test", "action":"create", "indexes":"key", "data":[{"key":"value1", "key2":"tiếng việt"}] }
        public static string insert(message m)
        {
            string json = "{}";

            var ips = JsonConvert.DeserializeObject<JObject[]>(m.input);
            if (ips.Length > 0)
            {
                IDB db = dbi.Get(m.model);
                if (db != null)
                {
                    string[] IDs = db.InsertBulk(ips.convertBsonDocument(true));
                    json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + IDs.Length.ToString() + @",""ids"":" + JsonConvert.SerializeObject(IDs) + @"}";
                }
            }

            return json;
        }

        public static string count(string model)
        {
            string json = @"{""ok"":false,""model"":""" + model + @""",""count"":-1}";
            IDB db = dbi.Get(model);
            if (db != null)
                json = @"{""ok"":true,""model"":""" + model + @""",""count"":" + db.Count().ToString() + @"}";
            return json;
        }

        //http://127.0.0.1:8888/?model=test&action=fetch
        public static string fetch(message m)
        {
            string json = "{}";
            string skip = m.jobject.getValue("skip");
            string limit = m.jobject.getValue("limit");

            int _skip = 0;
            int _limit = 0;

            if (int.TryParse(skip, out _skip) && int.TryParse(limit, out _limit) && _skip >= 0 && _limit > 0)
            {
                IDB db = dbi.Get(m.model);
                if (db != null)
                {
                    var result = db.Fetch(_skip, _limit).Select(x => x.toJson).ToArray(); ;
                    if (result.Length > 0)
                        json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + result.Length.ToString() + @",""items"":[" + string.Join(",", result) + @"]}";
                }
            }

            return json;
        }

        public static string query(message m)
        {
            string json = "{}";

            //var skip = Convert.ToInt32(input);
            //var limit = 10; 
            //var result = test.Fetch(skip, limit); 

            //foreach (var doc in result)
            //{
            //    Console.WriteLine(
            //        doc["_id"].AsString.PadRight(6) + " - " +
            //        doc["name"].AsString.PadRight(30) + "  -> " +
            //        doc["age"].AsInt32);
            //}

            return json;
        }

        //http://127.0.0.1:8888?model=test&action=getbyid&___id=20180302091035965
        public static string getbyid(message m)
        {
            string json = "{}";
            string ___id = m.jobject.getValue(_LITEDB_CONST.FIELD_ID);

            long id = -1;
            if (long.TryParse(___id, out id) && id > 0)
            {
                IDB db = dbi.Get(m.model);
                {
                    var result = db.Select(LiteDB.Query.EQ(_LITEDB_CONST.FIELD_ID, id));
                    json = @"{""ok"":true,""count"":" + result.Count().ToString() + @", ""items"":""" + JsonConvert.SerializeObject(result) + @"""}";
                }
            }

            return json;
        }

        
        public static string import_file(message m) {
            string json = "{}";
            return json;
        }

    }
}
