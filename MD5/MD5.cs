using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MD5
{
    public class MD5
    {
        public event EventHandler<HashingProgressEventArgs> HashingProgressChanged;

        private const uint BITS_PER_BYTE = 8;
        private const uint BYTES_COUNT_PER_512_BLOCK = 64;

        private readonly static uint[] T = new uint[64]
        {
            0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
            0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
            0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
            0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
            0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
            0xd62f105d, 0x2441453,  0xd8a1e681, 0xe7d3fbc8,
            0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
            0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
            0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
            0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
            0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x4881d05,
            0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
            0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
            0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
            0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
            0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
        };

        private readonly static int[] S = new int[] {
            7, 12, 17, 22,  7, 12, 17, 22,  7, 12, 17, 22,  7, 12, 17, 22,
            5,  9, 14, 20,  5,  9, 14, 20,  5,  9, 14, 20,  5,  9, 14, 20,
            4, 11, 16, 23,  4, 11, 16, 23,  4, 11, 16, 23,  4, 11, 16, 23,
            6, 10, 15, 21,  6, 10, 15, 21,  6, 10, 15, 21,  6, 10, 15, 21
        };

        private void OnHashingProgressChanged(HashingProgressEventArgs e)
        {
            EventHandler<HashingProgressEventArgs> temp = Volatile.Read(ref HashingProgressChanged);
            if (temp != null) temp(this, e);
        }

        private int GetPaddingLength(long bytesLength)
        {
            var paddingLength = (int)(((448 - ((bytesLength * BITS_PER_BYTE) % 512)) + 512) % 512);
            if (paddingLength == 0)
                paddingLength = 512;

            return paddingLength / 8 + 8;
        }

        private MDBuffer ProcessBlock(MDBuffer mDBuffer, int blockNumber, uint[] blockWords)
        {
            uint F, i, k;
            var tempMDBuffer = mDBuffer;

            for (i = 0; i < BYTES_COUNT_PER_512_BLOCK / 4; ++i)
            {
                k = i;
                F = FuncF(tempMDBuffer.B, tempMDBuffer.C, tempMDBuffer.D);

                this.Shift(ref tempMDBuffer, F, blockWords, i, k);
            }

            for (; i < BYTES_COUNT_PER_512_BLOCK / 2; ++i)
            {
                k = (1 + (5 * i)) % (BYTES_COUNT_PER_512_BLOCK / 4);
                F = FuncG(tempMDBuffer.B, tempMDBuffer.C, tempMDBuffer.D);

                this.Shift(ref tempMDBuffer, F, blockWords, i, k);
            }

            for (; i < BYTES_COUNT_PER_512_BLOCK / 4 * 3; ++i)
            {
                k = (5 + (3 * i)) % (BYTES_COUNT_PER_512_BLOCK / 4);
                F = FuncH(tempMDBuffer.B, tempMDBuffer.C, tempMDBuffer.D);

                this.Shift(ref tempMDBuffer, F, blockWords, i, k);
            }

            for (; i < BYTES_COUNT_PER_512_BLOCK; ++i)
            {
                k = 7 * i % (BYTES_COUNT_PER_512_BLOCK / 4);
                F = FuncI(tempMDBuffer.B, tempMDBuffer.C, tempMDBuffer.D);

                this.Shift(ref tempMDBuffer, F, blockWords, i, k);
            }

            return tempMDBuffer;
        }

        private uint FuncF(uint B, uint C, uint D) => (B & C) | (~B & D);

        private uint FuncG(uint B, uint C, uint D) => (D & B) | (C & ~D);

        private uint FuncH(uint B, uint C, uint D) => B ^ C ^ D;

        private uint FuncI(uint B, uint C, uint D) => C ^ (B | ~D);

        public void Shift(ref MDBuffer mDBuffer, uint F, uint[] X, uint i, uint k)
        {
            var tempD = mDBuffer.D;
            mDBuffer.D = mDBuffer.C;
            mDBuffer.C = mDBuffer.B;

            var valueToShift = mDBuffer.A + F + X[k] + T[i];
            mDBuffer.B += (valueToShift << S[i]) | (valueToShift >> (int)(BITS_PER_BYTE * sizeof(int) - S[i]));

            mDBuffer.A = tempD;
        }

        public MDBuffer ComputeHash(byte[] input)
        {
            var processedInput = new byte[input.Length + GetPaddingLength(input.Length)];
            Array.Copy(input, processedInput, input.Length);

            processedInput[input.Length] = 0x80;
            byte[] length = BitConverter.GetBytes(input.Length * 8);
            Array.Copy(length, 0, processedInput, processedInput.Length - 8, 4);

            var mDBuffer = MDBuffer.Initialize();

            var blocksAmount = (int)(processedInput.Length / BYTES_COUNT_PER_512_BLOCK);
            for (int blockNumber = 0; blockNumber < blocksAmount; ++blockNumber)
            {
                var X = new uint[BYTES_COUNT_PER_512_BLOCK / sizeof(int)];

                for (int byteNumber = 0; byteNumber < BYTES_COUNT_PER_512_BLOCK; byteNumber += sizeof(int))
                {
                    var j = blockNumber * BYTES_COUNT_PER_512_BLOCK + byteNumber;

                    X[byteNumber / sizeof(int)] = processedInput[j]
                        | (((uint)processedInput[j + 1]) << ((int)BITS_PER_BYTE * 1))
                        | (((uint)processedInput[j + 2]) << ((int)BITS_PER_BYTE * 2))
                        | (((uint)processedInput[j + 3]) << ((int)BITS_PER_BYTE * 3));
                }

                mDBuffer += ProcessBlock(mDBuffer.Clone(), blockNumber, X);

                OnHashingProgressChanged(new HashingProgressEventArgs(blockNumber + 1, blocksAmount));
            }

            return mDBuffer;
        }

        public async Task<MDBuffer> ComputeHash(Stream input)
        {
            var mDBuffer = MDBuffer.Initialize();

            var paddingLenght = GetPaddingLength(input.Length);
            var blocksAmount = (int)((input.Length + paddingLenght) / BYTES_COUNT_PER_512_BLOCK);
            for (int blockNumber = 0; blockNumber < blocksAmount - 1; ++blockNumber)
            {
                var streamBuffer = new byte[BYTES_COUNT_PER_512_BLOCK];
                await input.ReadAsync(streamBuffer, 0, (int)BYTES_COUNT_PER_512_BLOCK);
                var X = new uint[BYTES_COUNT_PER_512_BLOCK / sizeof(int)];

                for (int byteNumber = 0; byteNumber < BYTES_COUNT_PER_512_BLOCK; byteNumber += sizeof(int))
                {
                    X[byteNumber / sizeof(int)] = streamBuffer[byteNumber]
                        | (((uint)streamBuffer[byteNumber + 1]) << ((int)BITS_PER_BYTE * 1))
                        | (((uint)streamBuffer[byteNumber + 2]) << ((int)BITS_PER_BYTE * 2))
                        | (((uint)streamBuffer[byteNumber + 3]) << ((int)BITS_PER_BYTE * 3));
                }

                mDBuffer += ProcessBlock(mDBuffer.Clone(), blockNumber, X);

                OnHashingProgressChanged(new HashingProgressEventArgs(blockNumber, blocksAmount));
            }

            var lastPaddedStreamBuffer = new byte[BYTES_COUNT_PER_512_BLOCK];
            var lastBlockStartPosition = input.Position;
            await input.ReadAsync(lastPaddedStreamBuffer, 0, (int)BYTES_COUNT_PER_512_BLOCK);

            lastPaddedStreamBuffer[input.Length - lastBlockStartPosition] = 0x80;
            byte[] length = BitConverter.GetBytes(input.Length * 8);
            Array.Copy(length, 0, lastPaddedStreamBuffer, lastPaddedStreamBuffer.Length - 8, 4);

            var lastX = new uint[BYTES_COUNT_PER_512_BLOCK / sizeof(int)];

            for (int byteNumber = 0; byteNumber < BYTES_COUNT_PER_512_BLOCK; byteNumber += sizeof(int))
            {
                lastX[byteNumber / sizeof(int)] = lastPaddedStreamBuffer[byteNumber]
                    | (((uint)lastPaddedStreamBuffer[byteNumber + 1]) << ((int)BITS_PER_BYTE * 1))
                    | (((uint)lastPaddedStreamBuffer[byteNumber + 2]) << ((int)BITS_PER_BYTE * 2))
                    | (((uint)lastPaddedStreamBuffer[byteNumber + 3]) << ((int)BITS_PER_BYTE * 3));
            }

            mDBuffer += ProcessBlock(mDBuffer.Clone(), (blocksAmount - 1), lastX);

            OnHashingProgressChanged(new HashingProgressEventArgs(blocksAmount, blocksAmount));

            return mDBuffer;
        }
    }
}
