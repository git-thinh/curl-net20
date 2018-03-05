using System;
using System.Collections;
using System.Collections.Generic;

namespace LiteDB
{
    /// <summary>
    /// Include all components to be used in execution of a qery
    /// </summary>
    internal class QueryCursor : IDisposable
    {
        private int _position;
        private int _skip;
        private int _limit;
        private Query _query;
        private IEnumerator<IndexNode> _nodes;

        public List<BsonDocument> Documents { get; private set; }
        public List<string> DocumentIDs { get; private set; }
        public Dictionary<string, BsonDocument> CacheIDs { get; private set; }
        public bool HasMore { get; private set; }

        public QueryCursor(Query query)
        {
            _query = query;
            _skip = 0;
            _limit = -1;
            _position = 0;
            _nodes = null;
            
            this.HasMore = true;
            this.Documents = new List<BsonDocument>();
            this.DocumentIDs = new List<string>();
            this.CacheIDs = new Dictionary<string, BsonDocument>(){ };
        }

        public QueryCursor(Query query, int skip, int limit)
        {
            _query = query;
            _skip = skip;
            _limit = limit;
            _position = skip;
            _nodes = null;

            this.HasMore = true;
            this.Documents = new List<BsonDocument>();
            this.DocumentIDs = new List<string>();
            this.CacheIDs = new Dictionary<string, BsonDocument>(){ };
        }

        private void Clean() {
            this.Documents.Clear();
            this.DocumentIDs.Clear();
            this.CacheIDs.Clear();
        }

        /// <summary>
        /// Initialize nodes enumeator with query execution
        /// </summary>
        public void Initialize(IEnumerator<IndexNode> nodes)
        {
            _nodes = nodes;
        }

        /// <summary>
        /// ReQuery result and set skip counter to current position
        /// </summary>
        public void ReQuery(IEnumerator<IndexNode> nodes)
        {
            _nodes = nodes;
            _skip = _position;
        }

        /// <summary>
        /// Fetch documents from enumerator and add to buffer. If cache recycle, stop read to execute in another read
        /// </summary>
        public void FetchCacheIDs(TransactionService trans, DataService data, BsonReader bsonReader)
        {
            // empty document buffer
            this.Clean();

            // while until must cache not recycle
            while (trans.CheckPoint() == false)
            {
                // read next node
                this.HasMore = _nodes.MoveNext();

                // if finish, exit loop
                if (this.HasMore == false) return;

                // get current node
                var node = _nodes.Current;

                // read document from data block
                var buffer = data.Read(node.DataBlock);
                var doc = bsonReader.Deserialize(buffer).AsDocument;

                // if need run in full scan, execute full scan and test return
                if (_query.UseFilter)
                {
                    // execute query condition here - if false, do not add on final results
                    if (_query.FilterDocument(doc) == false) continue;
                }

                // increment position cursor
                _position++;

                string id = doc[_LITEDB_CONST.FIELD_ID].AsString;
                if (!this.CacheIDs.ContainsKey(id))
                    this.CacheIDs.Add(id, doc);
            }
        }

        /// <summary>
        /// Fetch documents from enumerator and add to buffer. If cache recycle, stop read to execute in another read
        /// </summary>
        public void FetchIDs(TransactionService trans, DataService data, BsonReader bsonReader)
        {
            // empty document buffer
            this.Clean();

            // while until must cache not recycle
            while (trans.CheckPoint() == false)
            {
                // read next node
                this.HasMore = _nodes.MoveNext();

                // if finish, exit loop
                if (this.HasMore == false) return;

                // get current node
                var node = _nodes.Current;

                // read document from data block
                var buffer = data.Read(node.DataBlock);
                var doc = bsonReader.Deserialize(buffer).AsDocument;

                // if need run in full scan, execute full scan and test return
                if (_query.UseFilter)
                {
                    // execute query condition here - if false, do not add on final results
                    if (_query.FilterDocument(doc) == false) continue;
                }

                // increment position cursor
                _position++;

                this.DocumentIDs.Add(doc[_LITEDB_CONST.FIELD_ID].AsString);
            }
        }

        /// <summary>
        /// Fetch documents from enumerator and add to buffer. If cache recycle, stop read to execute in another read
        /// </summary>
        public void Fetch(TransactionService trans, DataService data, BsonReader bsonReader)
        {
            // empty document buffer
            this.Clean();

            // while until must cache not recycle
            while (trans.CheckPoint() == false)
            {
                // read next node
                this.HasMore = _nodes.MoveNext();

                // if finish, exit loop
                if (this.HasMore == false) return;

                // if run ONLY under index, skip/limit before deserialize
                if (_query.UseIndex && _query.UseFilter == false)
                {
                    if (--_skip >= 0) continue;

                    if (--_limit <= -1)
                    {
                        this.HasMore = false;
                        return;
                    }
                }

                // get current node
                var node = _nodes.Current;

                // read document from data block
                var buffer = data.Read(node.DataBlock);
                var doc = bsonReader.Deserialize(buffer).AsDocument;

                // if need run in full scan, execute full scan and test return
                if (_query.UseFilter)
                {
                    // execute query condition here - if false, do not add on final results
                    if (_query.FilterDocument(doc) == false) continue;

                    // implement skip/limit after deserialize in full scan
                    if (--_skip >= 0) continue;

                    if (--_limit <= -1)
                    {
                        this.HasMore = false;
                        return;
                    }
                }

                // increment position cursor
                _position++;

                // avoid lock again just to check limit
                if (_limit == 0)
                {
                    this.HasMore = false;
                }

                this.Documents.Add(doc);
            }
        }

        public void Dispose()
        {
            if (_nodes != null)
            {
                _nodes.Dispose();
            }
        }
    }
}