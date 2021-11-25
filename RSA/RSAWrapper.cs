using System.Security.Cryptography;

namespace RSA
{
    public class RSAWrapper
    {
        private readonly int _blockSize;
        private readonly bool _doOAEPPadding;

        public RSAWrapper(int blockSize, bool doOAEPPadding)
        {
            _blockSize = blockSize;
            _doOAEPPadding = doOAEPPadding;
        }

        public async Task Encrypt(Stream unencrypted, Stream encrypted, RSAParameters rsaKeyInfo)
        {
            using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider())
            {
                rsaCryptoServiceProvider.ImportParameters(rsaKeyInfo);

                for (
                    int currentPosition = 0;
                    currentPosition < unencrypted.Length;
                    currentPosition += _blockSize)
                {
                    var inputBlock = new byte[_blockSize];
                    unencrypted.Read(inputBlock, currentPosition, _blockSize);

                    await encrypted.WriteAsync(rsaCryptoServiceProvider.Encrypt(
                        inputBlock,
                        _doOAEPPadding));
                }
            }
        }

        public async Task Decipher(Stream encrypted, Stream unencrypted, RSAParameters rsaKeyInfo)
        {
            using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider())
            {
                rsaCryptoServiceProvider.ImportParameters(rsaKeyInfo);

                for (
                    int currentPosition = 0;
                    currentPosition < encrypted.Length;
                    currentPosition += _blockSize)
                {
                    var inputBlock = new byte[_blockSize];
                    encrypted.Read(inputBlock, currentPosition, _blockSize);

                    await unencrypted.WriteAsync(rsaCryptoServiceProvider.Decrypt(
                        inputBlock,
                        _doOAEPPadding));
                }
            }
        }
    }
}
