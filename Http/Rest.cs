using System.Linq;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace curl
{
    public class Rest
    {
        public const long _LIMIT = 10;
        public const long _SKIP = 0;

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

        //http://127.0.0.1:8888?model=test&action=getbyid&_id=5744a604f1fa4b04a82cb94a
        public static string removebyid(message m)
        {
            string json = "{}";
            try
            {
                var jobject = JsonConvert.DeserializeObject<JObject>(m.input);
                string id = jobject.getValue(_LITEDB_CONST.FIELD_ID);

                if (id.Length == 24)
                {
                    IDB db = dbi.Get(m.model);
                    {
                        bool result = db.RemoveById(id);
                        json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""remove"":" + result.ToString().ToLower() + @", ""item"":""" + id + @"""}";
                    }
                }
                else
                    json = JsonConvert.SerializeObject(new { ok = false, output = "The field _id should be 24 hex characters" });
            }
            catch (Exception ex)
            {
                json = JsonConvert.SerializeObject(new { ok = false, output = ex.Message });
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
            var jobject = JsonConvert.DeserializeObject<JObject>(m.input);
            string json = @"{""ok"":false,""total"":-1,""count"":-1,""msg"":""Can not find model [" + m.model + @"]""}";
            string skip = jobject.getValue("skip");
            string limit = jobject.getValue("limit");

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

        public static string select(message m)
        {
            if (string.IsNullOrEmpty(m.query_string))
                return JsonConvert.SerializeObject(new { ok = false, output = "The data of QueryString is NULL. It has format: model=test&action=select&skip=0&limit=10&_op.1=eq&_id.1=8499849689d044a7a5b0ffe9&_andor.1=and&_op.2=eq&___dc.2=20180303" });

            var jobject = JsonConvert.DeserializeObject<JObject>(m.input);
            string json = @"{""ok"":false,""total"":-1,""count"":-1,""msg"":""Can not find model [" + m.model + @"]""}";

            IDB db = dbi.Get(m.model);
            if (db != null)
            {
                var result = db.Select(m.query_string).Select(x => x.toJson).ToArray();
                json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + result.Length.ToString() + @",""items"":[" + string.Join(",", result) + @"]}";
            }
            return json;
        }

        //http://127.0.0.1:8888?model=test&action=getbyid&_id=5744a604f1fa4b04a82cb94a
        public static string getbyid(message m)
        { 
            string json = "{}";
            try
            {
                var jobject = JsonConvert.DeserializeObject<JObject>(m.input);
                string id = jobject.getValue(_LITEDB_CONST.FIELD_ID);

                if (id.Length == 24)
                {
                    IDB db = dbi.Get(m.model);
                    {
                        var result = db.FindById(id);
                        json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + (result == null ? "0" : "1") + @", ""items"":" + (result == null ? "null" : result.toJson) + @"}";
                    }
                }
                else
                    json = JsonConvert.SerializeObject(new { ok = false, output = "The field _id should be 24 hex characters" });
            }
            catch (Exception ex) {
                json = JsonConvert.SerializeObject(new { ok = false, output = ex.Message });
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
