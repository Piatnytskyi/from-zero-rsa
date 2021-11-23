using RC5.Enums;
using RC5.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RC5
{
    public class RC5
    {
        private readonly LinearCongruentialGenerator.LinearCongruentialGenerator _linearCongruentialGenerator;
        private readonly IWordFactory _wordsFactory;
        private readonly int _roundsCount;

        public RC5(RoundCount roundCount = RoundCount.Rounds8, WordBitsLength wordBitsLength = WordBitsLength.Bit16)
        {
            _linearCongruentialGenerator = new LinearCongruentialGenerator.LinearCongruentialGenerator();

            _wordsFactory = wordBitsLength.GetWordFactory();
            _roundsCount = (int)roundCount;
        }

        public byte[] EncipherCBCPAD(byte[] input, byte[] key)
        {
            var paddedBytes = ConcatArrays(input, GetPadding(input));
            var bytesPerBlock = _wordsFactory.BytesPerBlock;
            var s = BuildExpandedKeyTable(key);
            var cnPrev = GetRandomBytesForInitVector().Take(bytesPerBlock).ToArray();
            var encodedFileContent = new byte[cnPrev.Length + paddedBytes.Length];

            EncipherECB(cnPrev, encodedFileContent, inStart: 0, outStart: 0, s);

            for (int i = 0; i < paddedBytes.Length; i += bytesPerBlock)
            {
                var cn = new byte[bytesPerBlock];
                Array.Copy(paddedBytes, i, cn, 0, cn.Length);

                cn.XorWith(
                    xorArray: cnPrev,
                    inStartIndex: 0,
                    xorStartIndex: 0,
                    length: cn.Length);

                EncipherECB(
                    inBytes: cn,
                    outBytes: encodedFileContent,
                    inStart: 0,
                    outStart: i + bytesPerBlock,
                    s: s);

                Array.Copy(encodedFileContent, i + bytesPerBlock, cnPrev, 0, cn.Length);
            }

            return encodedFileContent;
        }

        public byte[] DecipherCBCPAD(byte[] input, byte[] key)
        {
            var bytesPerBlock = _wordsFactory.BytesPerBlock;
            var s = BuildExpandedKeyTable(key);
            var cnPrev = new byte[bytesPerBlock];
            var decodedFileContent = new byte[input.Length - cnPrev.Length];

            DecipherECB(
                inBuf: input,
                outBuf: cnPrev,
                inStart: 0,
                outStart: 0,
                s: s);

            for (int i = bytesPerBlock; i < input.Length; i += bytesPerBlock)
            {
                var cn = new byte[bytesPerBlock];
                Array.Copy(input, i, cn, 0, cn.Length);

                DecipherECB(
                    inBuf: cn,
                    outBuf: decodedFileContent,
                    inStart: 0,
                    outStart: i - bytesPerBlock,
                    s: s);

                decodedFileContent.XorWith(
                    xorArray: cnPrev,
                    inStartIndex: i - bytesPerBlock,
                    xorStartIndex: 0,
                    length: cn.Length);

                Array.Copy(input, i, cnPrev, 0, cnPrev.Length);
            }

            var decodedWithoutPadding = new byte[decodedFileContent.Length - decodedFileContent.Last()];
            Array.Copy(decodedFileContent, decodedWithoutPadding, decodedWithoutPadding.Length);

            return decodedWithoutPadding;
        }

        private void EncipherECB(byte[] inBytes, byte[] outBytes, int inStart, int outStart, IWord[] s)
        {
            var a = _wordsFactory.CreateFromBytes(inBytes, inStart);
            var b = _wordsFactory.CreateFromBytes(inBytes, inStart + _wordsFactory.BytesPerWord);

            a.Add(s[0]);
            b.Add(s[1]);

            for (var i = 1; i < _roundsCount + 1; ++i)
            {
                a.XorWith(b).ROL(b.ToInt()).Add(s[2 * i]);
                b.XorWith(a).ROL(a.ToInt()).Add(s[2 * i + 1]);
            }

            a.FillBytesArray(outBytes, outStart);
            b.FillBytesArray(outBytes, outStart + _wordsFactory.BytesPerWord);
        }

        private void DecipherECB(byte[] inBuf, byte[] outBuf, int inStart, int outStart, IWord[] s)
        {
            var a = _wordsFactory.CreateFromBytes(inBuf, inStart);
            var b = _wordsFactory.CreateFromBytes(inBuf, inStart + _wordsFactory.BytesPerWord);

            for (var i = _roundsCount; i > 0; --i)
            {
                b = b.Sub(s[2 * i + 1]).ROR(a.ToInt()).XorWith(a);
                a = a.Sub(s[2 * i]).ROR(b.ToInt()).XorWith(b);
            }

            a.Sub(s[0]);
            b.Sub(s[1]);

            a.FillBytesArray(outBuf, outStart);
            b.FillBytesArray(outBuf, outStart + _wordsFactory.BytesPerWord);
        }

        private byte[] GetPadding(byte[] inBytes)
        {
            var paddingLength = _wordsFactory.BytesPerBlock - inBytes.Length % _wordsFactory.BytesPerBlock;

            var padding = new byte[paddingLength];

            for (int i = 0; i < padding.Length; ++i)
            {
                padding[i] = (byte)paddingLength;
            }

            return padding;
        }

        private byte[] GetRandomBytesForInitVector()
        {
            var ivParts = new List<byte[]>();

            while (ivParts.Sum(ivp => ivp.Length) < _wordsFactory.BytesPerBlock)
            {
                ivParts.Add(BitConverter.GetBytes(_linearCongruentialGenerator.Next()));
            }

            return ConcatArrays(ivParts.ToArray());
        }

        private IWord[] BuildExpandedKeyTable(byte[] key)
        {
            var keysWordArrLength = key.Length % _wordsFactory.BytesPerWord > 0
                ? key.Length / _wordsFactory.BytesPerWord + 1
                : key.Length / _wordsFactory.BytesPerWord;

            var lArr = new IWord[keysWordArrLength];

            for (int i = 0; i < lArr.Length; i++)
            {
                lArr[i] = _wordsFactory.Create();
            }

            for (var i = key.Length - 1; i >= 0; i--)
            {
                lArr[i / _wordsFactory.BytesPerWord].ROL(Constants.BitsPerByte).Add(key[i]);
            }

            var sArray = new IWord[2 * (_roundsCount + 1)];
            sArray[0] = _wordsFactory.CreateP();
            var q = _wordsFactory.CreateQ();

            for (var i = 1; i < sArray.Length; i++)
            {
                sArray[i] = sArray[i - 1].Clone();
                sArray[i].Add(q);
            }

            var x = _wordsFactory.Create();
            var y = _wordsFactory.Create();

            var n = 3 * Math.Max(sArray.Length, lArr.Length);

            for (int k = 0, i = 0, j = 0; k < n; ++k)
            {
                sArray[i].Add(x).Add(y).ROL(3);
                x = sArray[i].Clone();

                lArr[j].Add(x).Add(y).ROL(x.ToInt() + y.ToInt());
                y = lArr[j].Clone();

                i = (i + 1) % sArray.Length;
                j = (j + 1) % lArr.Length;
            }

            return sArray;
        }

        private static T[] ConcatArrays<T>(params T[][] arrays)
        {
            var position = 0;
            var outputArray = new T[arrays.Sum(a => a.Length)];

            foreach (var a in arrays)
            {
                Array.Copy(a, 0, outputArray, position, a.Length);
                position += a.Length;
            }

            return outputArray;
        }
    }
}
