using System;
using System.Linq;

namespace MD5
{
    public struct MDBuffer
    {
        public uint A;
        public uint B;
        public uint C;
        public uint D;

        public static MDBuffer Initialize()
        {
            return new MDBuffer { A = 0x67452301, B = 0xefcdab89, C = 0x98badcfe, D = 0x10325476 };
        }

        public byte[] ToByteArray()
        {
            return BitConverter.GetBytes(A)
                .Concat(BitConverter.GetBytes(B))
                .Concat(BitConverter.GetBytes(C))
                .Concat(BitConverter.GetBytes(D)).ToArray();
        }

        public MDBuffer Clone()
        {
            return (MDBuffer)MemberwiseClone();
        }

        public static MDBuffer operator +(MDBuffer left, MDBuffer right)
        {
            return new MDBuffer
            {
                A = left.A + right.A,
                B = left.B + right.B,
                C = left.C + right.C,
                D = left.D + right.D
            };
        }

        public override string ToString()
        {
            return $"{string.Join(string.Empty, BitConverter.GetBytes(A).Select(y => y.ToString("X2")))}" +
                $"{string.Join(string.Empty, BitConverter.GetBytes(B).Select(y => y.ToString("X2")))}" +
                $"{string.Join(string.Empty, BitConverter.GetBytes(C).Select(y => y.ToString("X2")))}" +
                $"{string.Join(string.Empty, BitConverter.GetBytes(D).Select(y => y.ToString("X2")))}";
        }
    }
}
