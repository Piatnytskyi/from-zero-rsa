namespace RC5
{
    internal class Word16BitFactory : IWordFactory
    {
        public int BytesPerWord => Word16Bit.BytesPerWord;

        public int BytesPerBlock => BytesPerWord * 2;

        public IWord Create()
        {
            return CreateConcrete();
        }

        public IWord CreateP()
        {
            return CreateConcrete(Constants.P16);
        }

        public IWord CreateQ()
        {
            return CreateConcrete(Constants.Q16);
        }

        public IWord CreateFromBytes(byte[] bytes, int startFromIndex)
        {
            var word = Create();
            word.CreateFromBytes(bytes, startFromIndex);

            return word;
        }

        private Word16Bit CreateConcrete(ushort value = 0)
        {
            return new Word16Bit
            {
                WordValue = value
            };
        }
    }
}
