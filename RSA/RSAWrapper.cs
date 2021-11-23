using System.Security.Cryptography;

namespace RSA
{
    public class RSAWrapper
    {
        private readonly RSACryptoServiceProvider _rsaCryptoServiceProvider;
        private readonly int _blockSize;

        public RSAWrapper(RSACryptoServiceProvider rsaCryptoServiceProvider, int blockSize)
        {
            _rsaCryptoServiceProvider = rsaCryptoServiceProvider;
            _blockSize = blockSize;
        }

        public async Task Encrypt(Stream unencrypted, Stream encrypted)
        {
            for (
                int currentPosition = 0;
                currentPosition < unencrypted.Length;
                currentPosition += _blockSize)
            {
                var inputBlock = new byte[_blockSize];
                unencrypted.Read(inputBlock, currentPosition, _blockSize);

                await encrypted.WriteAsync(_rsaCryptoServiceProvider.Encrypt(
                    inputBlock,
                    fOAEP: false));
            }
        }

        public async Task Decipher(Stream encrypted, Stream unencrypted)
        {
            for (
                int currentPosition = 0;
                currentPosition < encrypted.Length;
                currentPosition += _blockSize)
            {
                var inputBlock = new byte[_blockSize];
                encrypted.Read(inputBlock, currentPosition, _blockSize);

                await unencrypted.WriteAsync(_rsaCryptoServiceProvider.Decrypt(
                    inputBlock,
                    fOAEP: false));
            }
        }
    }
}
