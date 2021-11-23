using RC5.Enums;
using System;
using System.Linq;

namespace RC5.Extensions
{
    public static class ByteArrayExtensions
    {
        public static byte[] GetMD5HashedKeyForRC5(
            this byte[] key,
            KeyBytesLength keyBytesLength)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var hasher = new MD5.MD5();
            var bytesHash = hasher.ComputeHash(key).ToByteArray();

            if (keyBytesLength == KeyBytesLength.Bytes8)
            {
                bytesHash = bytesHash.Take(bytesHash.Length / 2).ToArray();
            }
            else if (keyBytesLength == KeyBytesLength.Bytes32)
            {
                bytesHash = bytesHash
                    .Concat(hasher.ComputeHash(bytesHash).ToByteArray())
                    .ToArray();
            }

            if (bytesHash.Length != (int)keyBytesLength)
            {
                throw new InvalidOperationException(
                    $"Internal error at {nameof(ByteArrayExtensions.GetMD5HashedKeyForRC5)} method, " +
                    $"hash result is not equal to {(int)keyBytesLength}.");
            }

            return bytesHash;
        }

        internal static void XorWith(
            this byte[] array,
            byte[] xorArray,
            int inStartIndex,
            int xorStartIndex,
            int length)
        {
            for (int i = 0;  i < length; ++i)
            {
                array[i + inStartIndex] ^= xorArray[i + xorStartIndex];
            }
        }
    }
}
