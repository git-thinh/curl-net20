﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDB
{
    public class Message
    {
        //[JsonIgnore]
        public string query_string { set; get; }
        public string method { set; get; }

        public string model { set; get; }
        public string action { set; get; }

        public string input { set; get; }
        public string output { set; get; }
    }
}
