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

        public string UpdateByIDs(IEnumerable<BsonDocument> docs)
        {
            if (!Opened)
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The model " + Model + " is closed" });
            long _total = Count();

            var rs = new UpdateResult();
            foreach (var doc in docs)
            {
                string id = doc[_LITEDB_CONST.FIELD_ID].AsString;
                var _doc_old = FindById(id);
                if (_doc_old != null && RemoveById(id))
                {
                    int k = _engine.InsertWithID(_LITEDB_CONST.COLLECTION_NAME, doc);
                    if (k == 1)
                        rs.listID_Success.Add(id);
                    else
                    {
                        _engine.InsertWithID(_LITEDB_CONST.COLLECTION_NAME, _doc_old);
                        rs.listID_Fail.Add(id);
                    }
                }
                else
                    rs.listID_Fail.Add(id);
            }

            string json = @"{""ok"":true,""total"":" + _total.ToString() + @",""update"":{""ok"":" +
                JsonConvert.SerializeObject(rs.listID_Success) + @", ""fail"":" + JsonConvert.SerializeObject(rs.listID_Fail) + @"}}";
            return json;
        }

        private QueryBuilder convertQuery(string _field, string _operator, string _value)
        {
            _operator = _operator.ToLower();
            bool _ok = false;
            Query _query = null;
            string _msg = string.Empty;
            switch (_operator)
            {
                case "all":            //int order = Ascending)
                    //_query = Query.All();
                    break;
                case "all_field":      //string field, int order = Ascending)
                    //_query = Query.All();
                    break;
                case "eq":             //string field, BsonValue value)
                    if (_value.IsNumeric())
                    {
                        if (_value.IndexOf('.') != -1)
                        {
                            double _d = 0;
                            if (double.TryParse(_value, out _d))
                            {
                                _ok = true;
                                _query = Query.EQ(_field, new BsonValue(_d));
                            }
                        }
                        else
                        {
                            int _val = 0;
                            if (int.TryParse(_value, out _val))
                            {
                                _ok = true;
                                _query = Query.EQ(_field, new BsonValue(_val));
                            }
                        }
                    }
                    else
                    {
                        _ok = true;
                        _query = Query.EQ(_field, new BsonValue(_value));
                    }
                    break;
                case "lt":             //string field, BsonValue value)
                    _value = _value.Trim();
                    if (_value.IsNumeric())
                    {
                        _ok = true;
                        _query = Query.LT(_field, new BsonValue(_value));
                    }
                    else
                    {
                        _msg = "The field [" + _field + "] using operator [" + _operator + "] to compare value be not number: [" + _value + "]";
                    }
                    break;
                case "lte":            //string field, BsonValue value)
                    _value = _value.Trim();
                    if (_value.IsNumeric())
                    {
                        _ok = true;
                        _query = Query.LTE(_field, new BsonValue(_value));
                    }
                    else
                    {
                        _msg = "The field [" + _field + "] using operator [" + _operator + "] to compare value be not number: [" + _value + "]";
                    }
                    break;
                case "gt":             //string field, BsonValue value)
                    _value = _value.Trim();
                    if (_value.IsNumeric())
                    {
                        _ok = true;
                        _query = Query.GT(_field, new BsonValue(_value));
                    }
                    else
                    {
                        _msg = "The field [" + _field + "] using operator [" + _operator + "] to compare value be not number: [" + _value + "]";
                    }
                    break;
                case "gte":            //string field, BsonValue value)
                    _value = _value.Trim();
                    if (_value.IsNumeric())
                    {
                        _ok = true;
                        _query = Query.GTE(_field, new BsonValue(_value));
                    }
                    else
                    {
                        _msg = "The field [" + _field + "] using operator [" + _operator + "] to compare value be not number: [" + _value + "]";
                    }
                    break;
                case "between":        //string field, BsonValue start, BsonValue end, bool startEquals = true, bool endEquals = true)
                    string[] a = _value.Split('|');
                    if (a.Length == 2)
                    {
                        string v1 = a[0].Trim(), v2 = a[1].Trim();
                        if (v1.IsNumeric() && v2.IsNumeric())
                        {
                            _ok = true;
                            _query = Query.Between(_field, new BsonValue(v1), new BsonValue(v2));
                        }
                        else
                        {
                            _msg = "The field [" + _field + "] using operator [" + _operator + "] to compare value be not number: [" + _value + "]";
                        }
                    }
                    else
                        _msg = "The field [" + _field + "] using operator [" + _operator + "] to compare value be not format: [ number_start | number_end ]";
                    break;
                case "start_with":     //string field, string value)
                    _ok = true;
                    _query = Query.StartsWith(_field, _value);
                    break;
                case "contains":       //string field, string value)
                    _ok = true;
                    _query = Query.Contains(_field, _value);
                    break;
                case "not":            //string field, BsonValue value)
                    _ok = true;
                    _query = Query.Not(_field, new BsonValue(_value));
                    break;
                //case "not_query":      //Query query, int order = Query.Ascending)
                //    _query = Query.All();
                //    break;
                //case "in_bson_array":  //string field, BsonArray value)
                //    _query = Query.All();
                //    break;
                case "in_array":       //string field, params BsonValue[] values)
                    _value = _value.Trim();
                    if (string.IsNullOrEmpty(_value))
                        _msg = "The field [" + _field + "] using operator [" + _operator + "] to compare value be not format: [ number_start | number_end ]";
                    else
                    {
                        _ok = true;
                        BsonValue[] a_in_array = _value.Split('|').Select(x => new BsonValue(x)).ToArray();
                        _query = Query.In(_field, a_in_array);
                    }
                    break;
                    //case "in_ienumerable": //string field, IEnumerable<BsonValue> values)      
                    //    _query = Query.All();
                    //    break;
            }
            return new QueryBuilder() { Ok = _ok, Query = _query, Msg = _msg };
        }

        public string Select(string input)
        {
            if (!Opened)
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The model " + Model + " is closed" });

            long _total = Count();
            string msg_field_null = JsonConvert.SerializeObject(new
            {
                ok = false,
                total = _total,
                count = 0,
                msg = "The data of QueryString is NULL. It has format: model=test&action=select&skip=0&limit=10&_op.1=eq&_id.1=8499849689d044a7a5b0ffe9&_andor.1=and&_op.2=eq&___dc.2=20180303"
            });
            if (string.IsNullOrEmpty(input)) return msg_field_null;

            //input = "model=test&action=select&skip=0&limit=10&" +
            //    "f.1=_id&o.1=eq&v.1=b67cb5e92b6c45e0bab345b2" +
            //    "&or=2.3" +
            //    "&f.2=___dc&o.2=eq&v.2=20180303" +
            //    "&f.3=___dc&o.3=eq&v.3=20180303";

            var a = input.Split('&')
                .Select(x => x.Split('='))
                .Where(x => x.Length > 1)
                .Select(x => new { key = x[0], value = x[1] })
                .ToArray();

            if (a.Length != a.Select(x => x.key).Distinct().ToArray().Length)
                return JsonConvert.SerializeObject(new { ok = false, total = _total, count = 0, msg = "The keys of QueryString duplicated" });

            string _or = a.Where(x => x.key == "or").Select(x => x.value).SingleOrDefault();
            var ws = a
                .Where(x => x.key.Contains('.') && x.key.Split('.')[1].IsNumeric())
                .Select(x => new QueryItem(x.key.Split('.')[0], int.Parse(x.key.Split('.')[1]), x.value, _or))
                .ToArray();

            int[] aCmdOR = ws.Where(x => x.isOr).Select(x => x.cmd).Distinct().ToArray();
            QueryBuilder _query = null;
            List<Query> lsOr = new List<Query>() { };
            for (int i = 0; i < aCmdOR.Length; i++)
            {
                var ai = ws.Where(x => x.cmd == aCmdOR[i]).ToArray();
                string _field = ai.Where(x => x.field == "f").Select(x => x.value).SingleOrDefault();
                string _operator = ai.Where(x => x.field == "o").Select(x => x.value).SingleOrDefault();
                string _value = ai.Where(x => x.field == "v").Select(x => x.value).SingleOrDefault();
                if (string.IsNullOrEmpty(_field) || string.IsNullOrEmpty(_operator))
                    return msg_field_null;
                _query = convertQuery(_field, _operator, _value);
                if (_query.Ok && _query.Query != null)
                    lsOr.Add(_query.Query);
                else
                    return JsonConvert.SerializeObject(new { ok = false, total = _total, count = 0, msg = _query.Msg });
            }

            int[] aCmdAnd = ws.Where(x => x.isOr == false).Select(x => x.cmd).Distinct().ToArray();
            List<Query> lsAnd = new List<Query>() { };
            for (int i = 0; i < aCmdAnd.Length; i++)
            {
                var ai = ws.Where(x => x.cmd == aCmdAnd[i]).ToArray();
                string _field = ai.Where(x => x.field == "f").Select(x => x.value).SingleOrDefault();
                string _operator = ai.Where(x => x.field == "o").Select(x => x.value).SingleOrDefault();
                string _value = ai.Where(x => x.field == "v").Select(x => x.value).SingleOrDefault();
                if (string.IsNullOrEmpty(_field) || string.IsNullOrEmpty(_operator))
                    return msg_field_null;
                _query = convertQuery(_field, _operator, _value);
                if (_query.Ok && _query.Query != null)
                    lsAnd.Add(_query.Query);
                else
                    return JsonConvert.SerializeObject(new { ok = false, total = _total, count = 0, msg = _query.Msg });
            }

            Dictionary<string, BsonDocument> dicStore = new Dictionary<string, BsonDocument>() { }; 
            IEnumerable<KeyValuePair<string, BsonDocument>> rsFind;

            switch (lsOr.Count)
            {
                case 0:
                    break;
                case 1:
                    rsFind = _engine.FindCacheIDs(_LITEDB_CONST.COLLECTION_NAME, lsOr[0]).ToArray();
                    foreach (var kv in rsFind)
                        if (!dicStore.ContainsKey(kv.Key))
                            dicStore.Add(kv.Key, kv.Value);
                    break;
                default:
                    rsFind = _engine.FindCacheIDs(_LITEDB_CONST.COLLECTION_NAME, Query.Or(lsOr.ToArray())).ToArray();
                    foreach (var kv in rsFind)
                        if (!dicStore.ContainsKey(kv.Key))
                            dicStore.Add(kv.Key, kv.Value);
                    break;
            }

            switch (lsAnd.Count)
            {
                case 0:
                    break;
                case 1:
                    rsFind = _engine.FindCacheIDs(_LITEDB_CONST.COLLECTION_NAME, lsAnd[0]).ToArray();
                    foreach (var kv in rsFind)
                        if (!dicStore.ContainsKey(kv.Key))
                            dicStore.Add(kv.Key, kv.Value);
                    break;
                default:
                    rsFind = _engine.FindCacheIDs(_LITEDB_CONST.COLLECTION_NAME, Query.And(lsAnd.ToArray())).ToArray();
                    foreach (var kv in rsFind)
                        if (!dicStore.ContainsKey(kv.Key))
                            dicStore.Add(kv.Key, kv.Value);
                    break;
            }

            var jobject = a.Where(x => !x.key.Contains("."))
                .GroupBy(x => x.key).Select(x => x.First())
                .ToDictionary(x => x.key, x => x.value);
            string skip, limit;
            jobject.TryGetValue("skip", out skip);
            jobject.TryGetValue("limit", out limit);

            long _skip = 0;
            long _limit = 0;

            long.TryParse(skip, out _skip);
            long.TryParse(limit, out _limit);

            if (_skip < 0) _skip = _LITEDB_CONST._SKIP;
            if (_limit <= 0) _limit = _LITEDB_CONST._LIMIT;

            string[] allID = dicStore.Keys.ToArray();
            string[] idRs = allID.Skip(_skip).Take(_limit).ToArray();
            List<string> result = new List<string>();
            foreach (string id in idRs)
                result.Add(dicStore[id].toJson);

            //var result = _engine
            //    //.Find(_LITEDB_CONST.COLLECTION_NAME, Query.In(_LITEDB_CONST.FIELD_ID, idRs))
            //    .Find(_LITEDB_CONST.COLLECTION_NAME, Query.Where(_LITEDB_CONST.FIELD_ID, _id => listID.IndexOf(_id) != -1))
            //    .Select(x => x.toJson).ToArray();

            return @"{""ok"":true,""total"":" + _total.ToString() + @",""count"":" + result.Count.ToString() + @",""items"":[" + string.Join(",", result.ToArray()) + @"]}";
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

            var result = _engine.Find(_LITEDB_CONST.COLLECTION_NAME, Query.All())
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
