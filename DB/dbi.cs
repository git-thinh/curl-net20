using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LiteDB
{
    public static class dbi
    {
        const string ___input = "[###]";
        const string ___output = "[$$$]";
        private static Type m_type = Type.GetType("curl.Rest");

        private static IDictionary<string, IDB> dicDB = new Dictionary<string, IDB>() { };
        public static void Init()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            string[] files = Directory.GetFiles(path, "*.db").Select(x => Path.GetFileName(x)).ToArray();
            foreach (string m in files)
            {
                string mi = m.Substring(0, m.Length - 3).ToLower();
                IDB db = new DbLite(mi, dbMode.OPEN);
                if (db.isOpen())
                    dicDB.Add(mi, db);
            }
        }

        public static string[] model_getAll()
        {
            return dicDB.Keys.ToArray();
        }

        public static DbLite Get(string model)
        {
            if (dicDB.ContainsKey(model))
                return (DbLite)dicDB[model];
            return null;
        }

        public static bool Open(string model)
        {
            IDB db = new DbLite(model, dbMode.OPEN);
            if (db.isOpen())
            {
                if (!dicDB.ContainsKey(model))
                    dicDB.Add(model, db);
                return true;
            }
            return false;
        }

        private static long _total(string model)
        {
            IDB db = Get(model);
            if (db != null)
                return db.Count();
            return 0;
        }

        public static string Excute(string messageJson)
        {
            Console.WriteLine(messageJson);
            Message[] a = convertMessage(messageJson);
            return Excute(a);
        }

        public static string Excute(Message[] a)
        {
            StringBuilder bi = new StringBuilder("[");
            for (int i = 0; i < a.Length; i++)
            {
                Message m = a[i];
                string _in = m.input;
                string rs = "{}";

                MethodInfo method = m_type.GetMethod(m.action, BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    rs = (string)method.Invoke(null, new object[] { m });

                    m.input = ___input;
                    m.output = ___output;
                    string ji = JsonConvert.SerializeObject(m);
                    ji = ji.Replace(@"""" + ___output + @"""", rs).Replace(@"""" + ___input + @"""", _in);
                    bi.Append(ji);
                    if (i > 0 && i != a.Length - 1) bi.Append(",");
                }
                else
                {
                    rs = JsonConvert.SerializeObject(new { ok = false, total = _total(m.model), output = "The rest service [" + m.model + "|" + m.action + "] can not find." });
                }
            }
            bi.Append("]");
            return bi.ToString();
        }

        public static bool Create(string model)
        {
            IDB db = new DbLite(model, dbMode.CREATE_AND_OPEN);
            if (db.isOpen())
            {
                if (!dicDB.ContainsKey(model))
                    dicDB.Add(model, db);
                return true;
            }
            return false;
        }

        public static void Close(string model)
        {
        }

        public static void CloseAll()
        {
        }

        #region [ FUNCTION ]

        private static Message[] convertMessage(string paraJson)
        {
            var it = JsonConvert.DeserializeObject<JObject[]>(paraJson);
            if (it != null)
                return it.Select(x => new Message()
                {
                    //jobject = x,
                    model = x.getValue("model").ToLower(),
                    action = x.getValue("action"),
                    input = x.getValue("data", true),
                    output = ___output
                }).ToArray();
            return new Message[] { };
        }

        public static string getValue(this JObject it, string name, bool serializeToJson = false)
        {
            string val = "";
            JProperty pro = it.Properties().Where(i => i.Name == name).SingleOrDefault();
            if (pro != null)
            {
                if (serializeToJson)
                    val = JsonConvert.SerializeObject(pro.Value);
                else
                    val = (string)pro.Value;
            }
            return val;
        }

        public static IEnumerable<BsonDocument> convertBsonDocument(this JObject[] a, bool update_DateTime_Changed = false)
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
