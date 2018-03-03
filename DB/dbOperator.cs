using System;
using System.Collections.Generic;
using System.Text;

namespace curl
{
    public enum DbOperator
    {
        ALL,            //int order = Ascending)
        ALL_FIELD,      //string field, int order = Ascending)
        EQ,             //string field, BsonValue value)
        LT,             //string field, BsonValue value)
        LTE,            //string field, BsonValue value)
        GT,             //string field, BsonValue value)
        GTE,            //string field, BsonValue value)
        BETWEEN,        //string field, BsonValue start, BsonValue end, bool startEquals = true, bool endEquals = true)
        START_WITH,     //string field, string value)
        CONTAINS,       //string field, string value)
        NOT,            //string field, BsonValue value)
        NOT_QUERY,      //Query query, int order = Query.Ascending)
        IN_BSON_ARRAY,  //string field, BsonArray value)
        IN_ARRAY,       //string field, params BsonValue[] values)
        IN_IENUMERABLE, //string field, IEnumerable<BsonValue> values)
        //Where , //string field, Func<BsonValue, bool> predicate, int order = Query.Ascending)
        //And , //Query left, Query right)
        //And , //params Query[] queries)
        //Or , //Query left, Query right)
        //Or , //params Query[] queries)
    }
}
