using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LiteDB
{
    public enum dbMode
    {
        OPEN = 1,
        CREATE_AND_OPEN = 2
    }

    public class DbLite : IDB
    {
        public string Model { set; get; }
        public bool Opened { set; get; } = false;

        private LiteEngine _engine = null;

        public bool isOpen() { return Opened; }

        public DbLite(string model_name, dbMode mode_type)
        {
            Model = model_name;
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, model_name + ".db"); // Path.Combine(Path.GetTempPath(), "litedb_paging.db");                  
            //File.Delete(filename);

            switch (mode_type)
            {
                case dbMode.OPEN:
                    if (!File.Exists(filename)) return;

                    var disk2 = new FileDiskService(filename, new LiteDB.FileOptions
                    {
                        FileMode = LiteDB.FileMode.Exclusive,
                        Journal = false
                    });

                    //_engine = new LiteEngine(disk, cacheSize: 50000);
                    _engine = new LiteEngine(disk2);

                    Opened = true;
                    break;
                case dbMode.CREATE_AND_OPEN:

                    var disk = new FileDiskService(filename, new LiteDB.FileOptions
                    {
                        FileMode = LiteDB.FileMode.Exclusive,
                        Journal = false
                    });

                    //_engine = new LiteEngine(disk, cacheSize: 50000);
                    _engine = new LiteEngine(disk);

                    Opened = true;
                    break;
            }
        }

        public string[] InsertBulk(IEnumerable<BsonDocument> docs)
        {
            if (!Opened) return new string[] { };

            // create indexes before
            //_engine.EnsureIndex(_LITEDB_CONST.COLLECTION_NAME, _LITEDB_CONST.FIELD_DATE_CREATE);

            // bulk data insert
            string[] rs = _engine.InsertReturnIDs(_LITEDB_CONST.COLLECTION_NAME, docs);
            return rs;
        }

        public long Count()
        {
            if (!Opened) return 0;
            return _engine.Count(_LITEDB_CONST.COLLECTION_NAME);
        }

        public BsonDocument FindById(string _id)
        {
            if (!Opened) return null;
            var result = _engine.FindById(_LITEDB_CONST.COLLECTION_NAME, new BsonValue(new ObjectId(_id)));
            return result;
        }

        public bool RemoveById(string _id)
        {
            if (!Opened) return false;
            var result = _engine.Delete(_LITEDB_CONST.COLLECTION_NAME, new BsonValue(new ObjectId(_id)));
            return result;
        }

        //public IEnumerable<BsonDocument> Select(string input)
        //{
        //    long k = Count();
        //    if (!Opened || string.IsNullOrEmpty(input)) return new List<BsonDocument>() { };
        //    return new List<BsonDocument>() { };
        //}

        private Query convertQuery(string _field, string _operator, string _value)
        {
            Query _query = null;
            switch (_operator)
            {
                case "all":            //int order = Ascending)
                    _query = Query.All();
                    break;
                case "all_field":      //string field, int order = Ascending)
                    _query = Query.All();
                    break;
                case "eq":             //string field, BsonValue value)
                    _query = Query.All();
                    break;
                case "lt":             //string field, BsonValue value)
                    _query = Query.All();
                    break;
                case "lte":            //string field, BsonValue value)
                    _query = Query.All();
                    break;
                case "gt":             //string field, BsonValue value)
                    _query = Query.All();
                    break;
                case "gte":            //string field, BsonValue value)
                    _query = Query.All();
                    break;
                case "between":        //string field, BsonValue start, BsonValue end, bool startEquals = true, bool endEquals = true)
                    _query = Query.All();
                    break;
                case "start_with":     //string field, string value)
                    _query = Query.All();
                    break;
                case "contains":       //string field, string value)
                    _query = Query.All();
                    break;
                case "not":            //string field, BsonValue value)
                    _query = Query.All();
                    break;
                case "not_query":      //Query query, int order = Query.Ascending)
                    _query = Query.All();
                    break;
                case "in_bson_array":  //string field, BsonArray value)
                    _query = Query.All();
                    break;
                case "in_array":       //string field, params BsonValue[] values)
                    _query = Query.All();
                    break;
                case "in_ienumerable": //string field, IEnumerable<BsonValue> values)      
                    _query = Query.All();
                    break;
            }
            return _query;
        }

        public string Select(string input)
        {
            long _total = Count();
            if (!Opened)
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The model " + Model + " is closed" });
            string msg_field_null = JsonConvert.SerializeObject(new
            {
                ok = false,
                total = _total,
                count = 0,
                msg = "The data of QueryString is NULL. It has format: model=test&action=select&skip=0&limit=10&_op.1=eq&_id.1=8499849689d044a7a5b0ffe9&_andor.1=and&_op.2=eq&___dc.2=20180303"
            });
            if (string.IsNullOrEmpty(input)) return msg_field_null;

            //model=test&action=select&skip=0&limit=10&_op.1=eq&_id.1=b67cb5e92b6c45e0bab345b2&_andor.1=and&_op.2=eq&___dc.2=20180303
            input = "model=test&action=select&skip=0&limit=10&" +
                "f.1=_id&o.1=eq&v.1=b67cb5e92b6c45e0bab345b2" +
                "&or=2&" +
                "f.2=___dc&o.2=eq&v.2=20180303";

            var a = input.Split('&')
                .Select(x => x.Split('='))
                .Where(x => x.Length > 1)
                .Select(x => new { key = x[0], value = x[1] })
                .ToArray();

            if (a.Length != a.Select(x => x.key).Distinct().ToArray().Length)
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The keys of QueryString duplicated" });

            string _or = a.Where(x => x.key == "or").Select(x => x.value).SingleOrDefault();
            var ws = a
                .Where(x => x.key.Contains('.') && Microsoft.VisualBasic.Information.IsNumeric(x.key.Split('.')[1]))
                .Select(x => new QueryItem(x.key.Split('.')[0], int.Parse(x.key.Split('.')[1]), x.value, _or))
                .ToArray();

            int[] aCmdOR = ws.Where(x => x.isOr).Select(x => x.cmd).Distinct().ToArray();
            List<Query> lsAnd = new List<Query>() { };
            List<Query> lsOr = new List<Query>() { };
            for (int i = 0; i < aCmdOR.Length; i++)
            {
                var ai = ws.Where(x => x.cmd == aCmdOR[i]).ToArray();
                string _field = ai.Where(x => x.field == "f").Select(x => x.value).SingleOrDefault();
                string _operator = ai.Where(x => x.field == "o").Select(x => x.value).SingleOrDefault();
                string _value = ai.Where(x => x.field == "v").Select(x => x.value).SingleOrDefault();
                if (string.IsNullOrEmpty(_field) || string.IsNullOrEmpty(_operator)) return msg_field_null;
                lsOr.Add(convertQuery(_field, _operator, _value));
            }

            var jobject = JsonConvert.DeserializeObject<JObject>(input);
            string skip = jobject.getValue("skip");
            string limit = jobject.getValue("limit");

            long _skip = 0;
            long _limit = 0;

            long.TryParse(skip, out _skip);
            long.TryParse(limit, out _limit);

            if (_skip < 0) _skip = _LITEDB_CONST._SKIP;
            if (_limit <= 0) _limit = _LITEDB_CONST._LIMIT;

            var result = _engine.Find("col", Query.Not(_LITEDB_CONST.FIELD_ID, 0));

            return JsonConvert.SerializeObject(new { ok = true, total = _total, count = 0, items = new int[] { } });
        }

        public IEnumerable<BsonDocument> Fetch(long skip, long limit)
        {
            long k = Count();
            if (!Opened || skip >= k) return new List<BsonDocument>() { };

            //Query _query = Query.EQ(LiteEngine.COLUMN_ID, 22);
            //var result = _engine.FindSort(
            //    "col",
            //    _query,
            //    "$.name",
            //    Query.Ascending,
            //    skip,
            //    limit);

            ////var result = _engine.Find("col", _query)
            ////    .OrderBy(x => x["name"].AsString)
            ////    .Skip(skip)
            ////    .Take(limit);

            var result = _engine.Find("col", Query.Not(_LITEDB_CONST.FIELD_ID, 0))
                .Skip(skip)
                .Take(limit);

            return result;
        }

        public bool Close()
        {
            _engine.Dispose();
            return true;
        }

        public bool Delete(string _id)
        {
            return _engine.Delete(_LITEDB_CONST.FIELD_ID, _id);
        }
    }
}
