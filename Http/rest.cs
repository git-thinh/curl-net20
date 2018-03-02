using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace curl
{
    public class message
    {
        [JsonIgnore]
        public JObject jobject { set; get; }

        public string model { set; get; }
        public string action { set; get; }

        public string input { set; get; }
        public string output { set; get; }
    }

    public static class rest
    {
        const string ___input = "[###]";
        const string ___output = "[$$$]";
        private static IDictionary<string, IDB> dicDB = new Dictionary<string, IDB>() { };

        public static void Load()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string[] files = Directory.GetFiles(path, "*.db").Select(x => Path.GetFileName(x)).ToArray();
            foreach (string m in files)
            {
                string mi = m.Substring(0, m.Length - 3).ToLower();
                IDB db = new dbLite(mi, dbMode.OPEN);
                if (db.isOpen())
                    dicDB.Add(mi, db);
            }
        }

        public static string Query(string paraJson)
        {
            Console.WriteLine(paraJson);
            message[] a = convertMessage(paraJson);
            return Query(a);
        }

        public static string Query(message[] a)
        {
            StringBuilder bi = new StringBuilder("[");
            for (int i = 0; i < a.Length; i++)
            {
                message m = a[i];
                string _in = m.input;
                string rs = "{}";
                switch (m.action)
                {
                    case "create":
                        rs = _create(m);
                        break;
                    case "insert":
                        rs = _insert(m);
                        break;
                    case "query":
                        rs = _query(m);
                        break;
                    case "getbyid":
                        rs = _getbyid(m);
                        break;
                    case "fetch":
                        rs = _fetch(m);
                        break;
                    case "count":
                        rs = _count(m.model);
                        break;
                }
                m.input = ___input;
                m.output = ___output;
                string ji = JsonConvert.SerializeObject(m);
                ji = ji.Replace(@"""" + ___output + @"""", rs).Replace(@"""" + ___input + @"""", _in);
                bi.Append(ji);
                if (i > 0 && i != a.Length - 1) bi.Append(",");
            }
            bi.Append("]");

            return bi.ToString();
        }

        // { "model":"test", "action":"create", "indexes":"key", "data":[{"key":"value1", "key2":"tiếng việt"}] }
        private static string _create(message m)
        {
            string json = "{}";
            string indexes = getValue(m.jobject, "indexes");

            IDB db = new dbLite(m.model, dbMode.CREATE_AND_OPEN);
            if (db.isOpen()) dicDB.Add(m.model, db);

            if (!string.IsNullOrEmpty(m.input))
                json = _insert(new message() { action = "insert", model = m.model, input = m.input });
            else
                json = @"{""ok"":true,""total"":0,""output"":""Create database " + m.model + @" successfully.""}";

            return json;
        }

        // { "model":"test", "action":"create", "indexes":"key", "data":[{"key":"value1", "key2":"tiếng việt"}] }
        private static string _insert(message m)
        {
            string json = "{}";

            var ips = JsonConvert.DeserializeObject<JObject[]>(m.input);
            if (ips.Length > 0)
            {
                IDB db = null;
                if (dicDB.TryGetValue(m.model, out db) && db != null)
                {
                    string[] IDs = db.InsertBulk(convertBsonDocument(ips, true));
                    json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + IDs.Length.ToString() + @",""ids"":" + JsonConvert.SerializeObject(IDs) + @"}";
                }
            }

            return json;
        }

        //http://127.0.0.1:8888/?model=test&action=fetch
        private static string _fetch(message m)
        {
            string json = "{}";
            string skip = getValue(m.jobject, "skip");
            string limit = getValue(m.jobject, "limit");

            int _skip = 0;
            int _limit = 0;

            if (int.TryParse(skip, out _skip) && int.TryParse(limit, out _limit) && _skip >= 0 && _limit > 0)
            {
                IDB db = null;
                if (dicDB.TryGetValue(m.model, out db) && db != null)
                {
                    var result = db.Fetch(_skip, _limit).Select(x => x.toJson).ToArray(); ;
                    if (result.Length > 0)
                        json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + result.Length.ToString() + @",""items"":[" + string.Join(",", result) + @"]}";
                }
            }

            return json;
        }

        private static string _query(message m)
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
        private static string _getbyid(message m)
        {
            string json = "{}";
            string ___id = getValue(m.jobject, _LITEDB_CONST.FIELD_ID);

            long id = -1;
            if (long.TryParse(___id, out id) && id > 0)
            {
                IDB db = null;
                if (dicDB.TryGetValue(m.model, out db) && db != null)
                {
                    var result = db.Select(LiteDB.Query.EQ(_LITEDB_CONST.FIELD_ID, id));
                    json = @"{""ok"":true,""count"":" + result.Count().ToString() + @", ""items"":""" + JsonConvert.SerializeObject(result) + @"""}";
                }
            }

            return json;
        }

        private static string _count(string model)
        {
            string json = @"{""ok"":false,""count"":""-1""}";
            IDB db = null;
            if (dicDB.TryGetValue(model, out db) && db != null)
            {
                json = @"{""ok"":true,""count"":""" + db.Count() + @"""}";
            }
            return json;
        }

        private static string _import_file(message m) {
            string json = "{}";
            return json;
        }

        #region [ FUNCTION ]

        private static message[] convertMessage(string paraJson)
        {
            var it = JsonConvert.DeserializeObject<JObject[]>(paraJson);
            if (it != null)
                return it.Select(x => new message()
                {
                    jobject = x,
                    model = getValue(x, "model").ToLower(),
                    action = getValue(x, "action"),
                    input = getValue(x, "data", true),
                    output = ___output
                }).ToArray();
            return new message[] { };
        }

        private static string getValue(JObject it, string name, bool parseJson = false)
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

        private static IEnumerable<BsonDocument> convertBsonDocument(JObject[] a, bool update_DateTime_Changed = false)
        {
            //IList<BsonDocument> ls = new List<BsonDocument>() { };

            for (int i = 0; i < a.Length; i++)
            {
                var ps = a[i]
                    .Properties()
                    .Select(x => new { name = x.Name, value = x.Value.ToString(), _type = x.Value.Type })
                    .ToArray();
                var doc = new BsonDocument();

                for (int j = 0; j < ps.Length; j++)
                {
                    switch (ps[j]._type)
                    {
                        case JTokenType.Date:
                            doc[ps[j].name] = 0;
                            break;
                        case JTokenType.Float:
                            doc[ps[j].name] = Convert.ToDouble(ps[j].value);
                            break;
                        case JTokenType.Guid:
                            doc[ps[j].name] = 0;
                            break;
                        case JTokenType.Integer:
                            doc[ps[j].name] = Convert.ToInt32(ps[j].value);
                            break;
                        case JTokenType.String:
                            doc[ps[j].name] = ps[j].value;
                            break;
                        case JTokenType.TimeSpan:
                            doc[ps[j].name] = 0;
                            break;
                        default:
                            doc[ps[j].name] = 0;
                            break;
                    }
                }

                if (update_DateTime_Changed)
                {
                    doc[_LITEDB_CONST.FIELD_DATE_CREATE] = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
                    doc[_LITEDB_CONST.FIELD_TIME_CREATE] = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                }

                yield return doc;
                //ls.Add(doc);
            }

            //return ls;
        }

        #endregion
    }
}
