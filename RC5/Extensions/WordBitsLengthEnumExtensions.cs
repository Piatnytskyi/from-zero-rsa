using RC5.Enums;
using System;

namespace RC5.Extensions
{
    public static class WordBitsLengthEnumExtensions
    {
        internal static IWordFactory GetWordFactory(this WordBitsLength wordLengthInBits)
        {
            IWordFactory wordFactory = null;

            switch (wordLengthInBits)
            {
                case WordBitsLength.Bit16:
                    wordFactory = new Word16BitFactory();
                    break;
                case WordBitsLength.Bit32:
                    wordFactory = new Word32BitFactory();
                    break;
                case WordBitsLength.Bit64:
                    wordFactory = new Word64BitFactory();
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(WordBitsLength)} value.");
            }

            return wordFactory;
        }
    }
}
