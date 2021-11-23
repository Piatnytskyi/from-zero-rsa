namespace RC5
{
    internal interface IWordFactory
    {
        int BytesPerWord { get; }
        int BytesPerBlock { get; }
        IWord CreateQ();
        IWord CreateP();
        IWord Create();
        IWord CreateFromBytes(byte[] bytes, int startFrom);
    }
}
