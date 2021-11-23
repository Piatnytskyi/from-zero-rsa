namespace RC5
{
    internal interface IWord
    {
        void CreateFromBytes(byte[] bytes, int startFromIndex);
        byte[] FillBytesArray(byte[] bytesToFill, int startFromIndex);
        IWord ROL(int offset);
        IWord ROR(int offset);
        IWord XorWith(IWord word);
        IWord Add(IWord word);
        IWord Add(byte value);
        IWord Sub(IWord word);
        IWord Clone();
        int ToInt();
    }
}
