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

        private Query convertQuery() {
            return null;
        }

        public string Select(string input)
        {
            long _total = Count();
            if (!Opened)
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The model " + Model + " is closed" });
            if (string.IsNullOrEmpty(input))
                return JsonConvert.SerializeObject(new { ok = false, total = _total, count = 0, msg = "The data of QueryString is NULL. It has format: model=test&action=select&skip=0&limit=10&_op.1=eq&_id.1=8499849689d044a7a5b0ffe9&_andor.1=and&_op.2=eq&___dc.2=20180303" });
            
            //model=test&action=select&skip=0&limit=10&_op.1=eq&_id.1=b67cb5e92b6c45e0bab345b2&_andor.1=and&_op.2=eq&___dc.2=20180303
            input = "model=test&action=select&skip=0&limit=10&"+
                "f.1=_id&o.1=eq&v.1=b67cb5e92b6c45e0bab345b2"+
                "&or.2=true&"+
                "f.2=___dc&o.2=eq&v.2=20180303";

            var a = input.Split('&')
                .Select(x => x.Split('='))
                .Where(x => x.Length > 1)
                .Select(x => new { key = x[0], value = x[1] })
                .Where(x => x.key.Contains('.') && Microsoft.VisualBasic.Information.IsNumeric(x.key.Split('.')[1]))
                .ToArray();

            if (a.Length != a.Select(x => x.key).Distinct().ToArray().Length)
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The keys of QueryString duplicated" });
            
            var ws = a.Select(x => new QueryItem() { field = x.key.Split('.')[0], value = x.value, cmd = int.Parse(x.key.Split('.')[1]) }).ToArray();

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
