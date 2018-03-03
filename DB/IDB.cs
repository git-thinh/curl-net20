using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDB
{
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
    }
}
