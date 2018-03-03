using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace curl
{
    interface IDB
    {
        string[] InsertBulk(IEnumerable<BsonDocument> docs);
        long Count();
        IEnumerable<BsonDocument> Fetch(long skip, long limit);
        BsonDocument FindById(string _id);
        bool RemoveById(string _id);
        bool isOpen();
        bool Close();
        bool Delete(string _id);
    }

    public enum dbMode
    {
        OPEN = 1,
        CREATE_AND_OPEN = 2
    }

    public class dbLite : IDB
    {
        public string Model { set; get; }
        public bool Opened { set; get; } = false;

        private LiteEngine _engine = null;
        
        public bool isOpen() { return Opened; }

        public dbLite(string model_name, dbMode mode_type)
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

        public bool Close() {
            _engine.Dispose();
            return true;
        }

        public bool Delete(string _id) {
            return _engine.Delete(_LITEDB_CONST.FIELD_ID, _id);
        }
    }
}
