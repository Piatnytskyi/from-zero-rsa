namespace RC5
{
    internal class Word64Bit : IWord
    {
        public const int WordSizeInBits = BytesPerWord * Constants.BitsPerByte;
        public const int BytesPerWord = sizeof(ulong);

        public ulong WordValue { get; set; }

        public void CreateFromBytes(byte[] bytes, int startFrom)
        {
            WordValue = 0;

            for (var i = startFrom + BytesPerWord - 1; i > startFrom; --i)
            {
                WordValue |= bytes[i];
                WordValue <<= Constants.BitsPerByte;
            }

            WordValue |= bytes[startFrom];
        }

        public byte[] FillBytesArray(byte[] bytesToFill, int startFrom)
        {
            var i = 0;
            for (; i < BytesPerWord - 1; ++i)
            {
                bytesToFill[startFrom + i] = (byte)(WordValue & Constants.ByteMask);
                WordValue >>= Constants.BitsPerByte;
            }

            bytesToFill[startFrom + i] = (byte)(WordValue & Constants.ByteMask);

            return bytesToFill;
        }

        public IWord ROL(int offset)
        {
            WordValue = (WordValue << offset) | (WordValue >> (WordSizeInBits - offset));

            return this;
        }

        public IWord ROR(int offset)
        {
            WordValue = (WordValue >> offset) | (WordValue << (WordSizeInBits - offset));

            return this;
        }

        public IWord Add(IWord word)
        {
            WordValue += (word as Word64Bit).WordValue;

            return this;
        }

        public IWord Add(byte value)
        {
            WordValue += value;

            return this;
        }

        public IWord Sub(IWord word)
        {
            WordValue -= (word as Word64Bit).WordValue;

            return this;
        }

        public IWord XorWith(IWord word)
        {
            WordValue ^= (word as Word64Bit).WordValue;

            return this;
        }

        public IWord Clone()
        {
            return (IWord)MemberwiseClone();
        }

        public int ToInt()
        {
            return (int)WordValue;
        }
    }
}
