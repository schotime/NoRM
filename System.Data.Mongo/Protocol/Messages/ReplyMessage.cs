﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BSONLib;

namespace System.Data.Mongo.Protocol.Messages
{
    internal class ReplyMessage<T> : Message where T : class, new()
    {
        private int _messageLength;
        private int _requestID;
        private int _responseID;
        private MongoOp _mongoOp;

        private List<T> _results;

        /// <summary>
        /// Processes a response stream.
        /// </summary>
        /// <param name="reply"></param>
        internal ReplyMessage(byte[] reply)
        {
            this._messageLength = BitConverter.ToInt32(reply, 0);
            this._requestID = BitConverter.ToInt32(reply, 4);
            this._responseID = BitConverter.ToInt32(reply, 8);
            this._mongoOp = (MongoOp)BitConverter.ToInt32(reply, 12);
            this.HasError = BitConverter.ToInt32(reply, 16) == 1 ? true : false;
            this.CursorID = BitConverter.ToInt64(reply, 20);
            this.CursorPosition = BitConverter.ToInt32(reply, 28);
            this.ResultsReturned = BitConverter.ToInt32(reply, 32);

            this._results = new List<T>(this.ResultsReturned);
            BSONSerializer serializer = new BSONSerializer();
            var memstream = new MemoryStream(reply.Skip(36).ToArray());
            memstream.Position = 0;
            var bin = new BinaryReader(memstream);
            if (!this.HasError)
            {
                while (bin.BaseStream.Position < bin.BaseStream.Length)
                {
                    this._results.Add(serializer.Deserialize<T>(bin));
                }
            }
            else
            {
                //TODO: load the error document.
            }
        }

        /// <summary>
        /// The cursor to be used in future calls to "get more"
        /// </summary>
        public long CursorID
        {
            get;
            protected set;
        }

        /// <summary>
        /// The location of the cursor.
        /// </summary>
        public int CursorPosition
        {
            get;
            protected set;
        }

        /// <summary>
        /// If "HasError" is set, 
        /// </summary>
        public bool HasError
        {
            get;
            protected set;
        }

        /// <summary>
        /// The number of results returned form this request.
        /// </summary>
        public int ResultsReturned
        {
            get;
            protected set;
        }

        public IEnumerable<T> Results
        {
            get
            {
                return this._results.AsEnumerable();
            }
        }
    }
}