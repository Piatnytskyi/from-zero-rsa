using System.Security.Cryptography;

namespace RSA
{
    public class RSAWrapper
    {
        private readonly int _encryptionBlockSize;
        private readonly int _decryptionBlockSize;
        private readonly bool _doOAEPPadding;

        public RSAWrapper(int encryptionBlockSize, bool doOAEPPadding)
        {
            _encryptionBlockSize = encryptionBlockSize;
            _decryptionBlockSize = _encryptionBlockSize * 2;
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
                    currentPosition += _encryptionBlockSize)
                {
                    var inputBlock = new byte[_encryptionBlockSize];
                    unencrypted.Read(inputBlock, 0, _encryptionBlockSize);

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
                    currentPosition += _decryptionBlockSize)
                {
                    var inputBlock = new byte[_decryptionBlockSize];
                    encrypted.Read(inputBlock, 0, _decryptionBlockSize);

                    await unencrypted.WriteAsync(rsaCryptoServiceProvider.Decrypt(
                        inputBlock,
                        _doOAEPPadding));
                }
            }
        }
    }
}
