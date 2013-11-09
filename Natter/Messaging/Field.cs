using System;
using Natter.Byte;

namespace Natter.Messaging
{
    public struct Field : IField
    {
        private readonly byte[] _name;
        private readonly byte[] _value;

        public byte[] Name
        {
            get { return _name; }
        }

        public byte[] Value
        {
            get { return _value; }
        }

        public Field(byte[] name, byte[] value)
        {
            _name = name;
            _value = value;
        }

        public string GetNameAsString()
        {
            return _name.GetString();
        }

        public byte[] Serialise()
        {
            return SerialiseField(this);
        }

        public static IField Deserialise(byte[] data)
        {
            return DeserialiseField(data);
        }

        private static byte[] SerialiseField(IField field)
        {
            // <nameLength,valueLength>name=value
            return ByteTools.Combine(
                ByteValues.LessThanArray,
                field.Name.Length.ToString().GetBytes(),
                ByteValues.CommaArray,
                field.Value.Length.ToString().GetBytes(),
                ByteValues.GreaterThanArray,
                field.Name,
                ByteValues.EqualArray,
                field.Value,
                ByteValues.SpaceArray);
        }

        private static IField DeserialiseField(byte[] data)
        {
            int nameSize = 0;
            int valueSize = 0;
            int start = 1;
            int current = 1;
            for (; current < data.Length; current++)
            {
                if (data[current] == ByteValues.Comma || data[current] == ByteValues.GreaterThan)
                {
                    int length = current - start;
                    byte[] size = new byte[length];
                    Array.Copy(data, start, size, 0, length);
                    if (start == 1)
                    {
                        nameSize = int.Parse(size.GetString());
                        start = current + 1;
                    }
                    else
                    {
                        valueSize = int.Parse(size.GetString());
                        break;
                    }
                }
            }
            byte[] name = new byte[nameSize];
            Array.Copy(data, current + 1, name, 0, nameSize);
            byte[] value = new byte[valueSize];
            Array.Copy(data, current + nameSize + 2, value, 0, valueSize);
            return new Field(name, value);
        }
    }
}
