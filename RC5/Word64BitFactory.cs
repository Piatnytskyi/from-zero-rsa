namespace RC5
{
    internal class Word64BitFactory : IWordFactory
    {
        public int BytesPerWord => Word64Bit.BytesPerWord;

        public int BytesPerBlock => BytesPerWord * 2;

        public IWord Create()
        {
            return CreateConcrete();
        }

        public IWord CreateP()
        {
            return CreateConcrete(Constants.P64);
        }

        public IWord CreateQ()
        {
            return CreateConcrete(Constants.Q64);
        }

        public IWord CreateFromBytes(byte[] bytes, int startFromIndex)
        {
            var word = Create();
            word.CreateFromBytes(bytes, startFromIndex);

            return word;
        }

        private Word64Bit CreateConcrete(ulong value = 0)
        {
            return new Word64Bit
            {
                WordValue = value
            };
        }
    }
}
