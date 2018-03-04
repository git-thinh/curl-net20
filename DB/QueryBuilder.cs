using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDB
{
    public class QueryBuilder
    {
        public bool Ok { set; get; }
        public Query Query { set; get; }
        public string Msg { set; get; }
    }
}
