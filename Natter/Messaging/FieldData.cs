using System;
using System.Linq;
using Natter.Byte;

namespace Natter.Messaging
{
    public struct FieldData
    {
        private IField[] _fields;

        public FieldData(IField[] fields)
        {
            _fields = fields;
        }

        public IField[] GetFields()
        {
            return _fields ?? new IField[0];
        }

        public void Add(string key, string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Add(key, value.GetBytes());
        }

        public void Add(string key, byte[] value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            if(value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (_fields == null)
            {
                _fields = new IField[0];
            }
            AddNewField(new Field(key.GetBytes(), value));
        }

        public bool ContainsKey(string key)
        {
            var keyBytes = (key ?? string.Empty).GetBytes();
            return (_fields ?? new IField[0]).Any(f => ByteTools.Compare(f.Name, keyBytes));
        }

        public string GetStringValue(string key)
        {
            var bytes = GetByteValue(key);
            return bytes != null ? bytes.GetString() : null;
        }

        public byte[] GetByteValue(string key)
        {
            if (ContainsKey(key))
            {
                var keyBytes = (key ?? string.Empty).GetBytes();
                var field = (_fields ?? new IField[0]).First(f => ByteTools.Compare(f.Name, keyBytes));
                return field.Value;
            }
            return null;
        }

        private void AddNewField(IField field)
        {
            var newFields = new IField[_fields.Length + 1];
            newFields[_fields.Length] = field;
            Array.Copy(_fields, 0, newFields, 0, _fields.Length);
            _fields = newFields;
        }
    }
}
