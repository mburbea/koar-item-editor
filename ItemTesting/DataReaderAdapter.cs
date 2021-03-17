﻿using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ItemTesting
{
    public class DbDataReaderAdapter : DbDataReader
    {
        public DbDataReaderAdapter(IEnumerable<object[]> data) => _data = data.GetEnumerator();

        private StrongBox<bool> _firstRead = new();
        private readonly IEnumerator<object[]> _data;

        public override bool Read() => Interlocked.Exchange(ref _firstRead, null) is {Value: var value } ? value : _data.MoveNext();

        public override int FieldCount => (_firstRead.Value = _data.MoveNext()) ? _data.Current.Length : 0;

        public override object GetValue(int ordinal) => _data.Current[ordinal];

        public override bool IsDBNull(int ordinal) => _data.Current[ordinal] is null;

        protected override void Dispose(bool disposing) => _data.Dispose();
        #region Not Implemented methods
        public override object this[int ordinal] => throw new NotImplementedException();
        public override object this[string name] => throw new NotImplementedException();
        public override int Depth => throw new NotImplementedException();
        public override bool HasRows => throw new NotImplementedException();
        public override bool IsClosed => throw new NotImplementedException();
        public override int RecordsAffected => throw new NotImplementedException();
        public override bool GetBoolean(int ordinal) => throw new NotImplementedException();
        public override byte GetByte(int ordinal) => throw new NotImplementedException();
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => throw new NotImplementedException();
        public override char GetChar(int ordinal) => throw new NotImplementedException();
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => throw new NotImplementedException();
        public override string GetDataTypeName(int ordinal) => throw new NotImplementedException();
        public override DateTime GetDateTime(int ordinal) => throw new NotImplementedException();
        public override decimal GetDecimal(int ordinal) => throw new NotImplementedException();
        public override double GetDouble(int ordinal) => throw new NotImplementedException();
        public override IEnumerator GetEnumerator() => throw new NotImplementedException();
        public override Type GetFieldType(int ordinal) => throw new NotImplementedException();
        public override float GetFloat(int ordinal) => throw new NotImplementedException();
        public override Guid GetGuid(int ordinal) => throw new NotImplementedException();
        public override short GetInt16(int ordinal) => throw new NotImplementedException();
        public override int GetInt32(int ordinal) => throw new NotImplementedException();
        public override long GetInt64(int ordinal) => throw new NotImplementedException();
        public override string GetName(int ordinal) => throw new NotImplementedException();
        public override int GetOrdinal(string name) => throw new NotImplementedException();
        public override string GetString(int ordinal) => throw new NotImplementedException();
        public override int GetValues(object[] values) => throw new NotImplementedException();
        public override bool NextResult() => throw new NotImplementedException();
        #endregion
    }
}