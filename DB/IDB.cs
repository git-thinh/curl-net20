using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDB
{
    public class UpdateResult
    {
        public List<string> listID_Success { set; get; }
        public List<string> listID_Fail { set; get; }

        public UpdateResult()
        {
            listID_Fail = new List<string>() { };
            listID_Success = new List<string>() { };
        }
    }

    interface IDB
    {
        string[] InsertBulk(IEnumerable<BsonDocument> docs);
        long Count();
        string Select(string input);
        IEnumerable<BsonDocument> Fetch(long skip, long limit);
        BsonDocument FindById(string _id);
        bool RemoveById(string _id);
        bool isOpen();
        bool Close();
        bool Delete(string _id);
        string UpdateByIDs(IEnumerable<BsonDocument> docs);
    }
}
