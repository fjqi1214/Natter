using System;
using System.Linq;

namespace Natter.Byte
{
    public static class ByteTools
    {
        public static byte[] GetBytes(this string data)
        {
            return System.Text.Encoding.UTF8.GetBytes(data);
        }

        public static string GetString(this byte[] data)
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }

        public static byte[] GetBytes(this long data)
        {
            return BitConverter.GetBytes(data);
        }

        public static long GetLong(this byte[] data)
        {
            return BitConverter.ToInt64(data, 0);
        }

        public static byte[] GetBytes(this int data)
        {
            return BitConverter.GetBytes(data);
        }

        public static int GetInt(this byte[] data)
        {
            return BitConverter.ToInt32(data, 0);
        }

        public static byte[] Combine(params byte[][] values)
        {
            int offset = 0;
            byte[] res = new byte[values.Sum(x => x.Length)];
            foreach (byte[] val in values)
            {
                Buffer.BlockCopy(val, 0, res, offset, val.Length);
                offset += val.Length;
            }
            return res;
        }

        public static bool Compare(byte[] b1, byte[] b2)
        {
            return Compare(b1, 0, b2, 0, b1.Length > b2.Length ? b1.Length : b2.Length);
        }

        public static bool Compare(byte[] b1, int start1, byte[] b2, int start2, int length)
        {
            if (b1.Length < start1 + length  || b2.Length < start2 + length)
            {
                return false;
            }

            for (int loop = 0; loop < length; loop++)
            {
                if (b1[loop + start1] != b2[loop + start2])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
