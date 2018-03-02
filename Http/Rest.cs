using System.Linq;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace curl
{
    public class Rest
    {
        const long _LIMIT = 10;
        const long _SKIP = 0;

        // { "model":"test", "action":"create", "data":[{"key":"value1", "key2":"tiếng việt"}] }
        public static string create(message m)
        {
            string json = "{}";
            //string indexes = m.jobject.getValue("indexes");

            bool isOk = dbi.Create(m.model);

            if (isOk && !string.IsNullOrEmpty(m.input))
                json = insert(new message() { action = "insert", model = m.model, input = m.input });
            else
                json = @"{""ok"":true,""total"":0,""output"":""Create database " + m.model + @" successfully.""}";

            return json;
        }

        // { "model":"test", "action":"create", "data":[{"key":"value1", "key2":"tiếng việt"}] }
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
                    json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + IDs.Length.ToString() + @",""items"":" + JsonConvert.SerializeObject(IDs) + @"}";
                }
            }

            return json;
        }

        //http://127.0.0.1:8888/?model=test&action=count
        public static string count(message m)
        {
            string json = @"{""ok"":false,""model"":""" + m.model + @""",""count"":-1}";
            IDB db = dbi.Get(m.model);
            if (db != null)
                json = @"{""ok"":true,""model"":""" + m.model + @""",""count"":" + db.Count().ToString() + @"}";
            return json;
        }

        //http://127.0.0.1:8888/?model=test&action=fetch
        //http://127.0.0.1:8888/?model=test&action=fetch&skip=0&limit=10
        public static string fetch(message m)
        {
            string json = @"{""ok"":false,""total"":-1,""count"":-1,""msg"":""Can not find model [" + m.model + @"]""}";
            string skip = m.jobject.getValue("skip");
            string limit = m.jobject.getValue("limit");

            long _skip = 0;
            long _limit = 0;

            long.TryParse(skip, out _skip);
            long.TryParse(limit, out _limit);

            if (_skip < 0) _skip = _SKIP;
            if (_limit <= 0) _limit = _LIMIT;

            IDB db = dbi.Get(m.model);
            if (db != null)
            {
                var result = db.Fetch(_skip, _limit).Select(x => x.toJson).ToArray();
                json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + result.Length.ToString() + @",""items"":[" + string.Join(",", result) + @"]}";
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

        //http://127.0.0.1:8888?model=test&action=getbyid&_id=20180302091035965
        public static string getbyid(message m)
        {
            string json = "{}";
            string id = m.jobject.getValue(_LITEDB_CONST.FIELD_ID);

            if (!string.IsNullOrEmpty(id))
            {
                IDB db = dbi.Get(m.model);
                {
                    var result = db.FindById(id);
                    json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + (result == null ? "0" : "1") + @", ""items"":" + (result == null ? "null" : result.toJson) + @"}";
                }
            }

            return json;
        }


        public static string import_file(message m)
        {
            string json = "{}";
            return json;
        }

    }
}
