using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDB
{
    class QueryItem
    {
        public string field { set; get; }
        public int cmd { set; get; }
        public string value { set; get; }
        public bool isOr { set; get; }

        public QueryItem(string _field, int _cmd, string _value, string _or) {
            field = _field;
            cmd = _cmd;
            value = _value;
            if (!string.IsNullOrEmpty(_or))
            {
                _or = "." + _or + ".";
                isOr = _or.IndexOf("." + cmd.ToString() + ".") != -1;
            }
        }
    }
}
