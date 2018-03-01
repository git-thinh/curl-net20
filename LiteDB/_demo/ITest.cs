using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading; 

namespace demo
{
    interface ITest
    {
        void Init();
        void Populate(IEnumerable<BsonDocument> docs);
        long Count();
        List<BsonDocument> Fetch(int skip, int limit);
    }
}