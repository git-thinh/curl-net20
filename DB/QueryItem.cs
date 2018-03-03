using System;
using System.Collections.Generic;
using System.Text;

namespace curl
{
    class QueryItem
    {
        public string field { set; get; }
        public int cmd { set; get; }
        public string value { set; get; }
        public bool isOr { set; get; } 
    }
}
