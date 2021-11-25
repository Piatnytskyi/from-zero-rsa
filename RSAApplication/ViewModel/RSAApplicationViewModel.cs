using Microsoft.Win32;
using RC5.Enums;
using RC5.Extensions;
using RSA;
using RSAApplication.Commands;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RSAApplication.ViewModel
{
    class RSAApplicationViewModel : AbstractViewModel
    {
        private readonly RSACryptoServiceProvider _rsaCryptoServiceProvider;
        private readonly RC5.RC5 _rc5;

        //TODO: Use some kind of settings.
        private const int _rsaEncryptionBlockSize = 64;

        private string _rc5PasswordInput = string.Empty;

        public string RC5PasswordInput
        {
            get => _rc5PasswordInput;
            set
            {
                if (_rc5PasswordInput.Equals(value))
                {
                    return;
                }
                _rc5PasswordInput = value;
                RaisePropertyChanged(nameof(RC5PasswordInput));
            }
        }

        private RSAService _rsaCurrentService;

        public RSAService RSACurrentService
        {
            get => _rsaCurrentService;
            set
            {
                if (_rsaCurrentService.Equals(value))
                {
                    return;
                }
                _rsaCurrentService = value;
                RaisePropertyChanged(nameof(RSACurrentService));
            }
        }

        private string _filenameInput = string.Empty;

        public string FilenameInput
        {
            get => _filenameInput;
            set
            {
                if (_filenameInput.Equals(value))
                {
                    return;
                }
                _filenameInput = value;
                RaisePropertyChanged(nameof(FilenameInput));
            }
        }
        
        //TODO: Make this and next property into state pattern.
        private string _status = string.Empty;

        public string Status
        {
            get => _status;
            set
            {
                if (_status.Equals(value))
                {
                    return;
                }
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }

        private string _output = string.Empty;

        public string Output
        {
            get => _output;
            set
            {
                if (_output.Equals(value))
                {
                    return;
                }
                _output = value;
                RaisePropertyChanged(nameof(Output));
            }
        }

        private bool _isInProgress;

        public bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                if (_isInProgress.Equals(value))
                {
                    return;
                }
                _isInProgress = value;
                RaisePropertyChanged(nameof(IsInProgress));
            }
        }

        public RelayCommand ChooseFileCommand { get; set; }
        public RelayCommand ImportRSAKeysCommand { get; set; }
        public RelayCommand ExportRSAKeysCommand { get; set; }
        public AsyncCommand RSAEncryptFileCommand { get; set; }
        public AsyncCommand RSADencryptFileCommand { get; set; }
        public AsyncCommand RC5EncryptFileCommand { get; set; }
        public AsyncCommand RC5DencryptFileCommand { get; set; }

        public RSAApplicationViewModel()
        {
            //TODO: Create decorators for state change.
            ChooseFileCommand = new RelayCommand(o => ChooseFile(), c => CanChooseFile());
            ImportRSAKeysCommand = new RelayCommand(o => ImportRSAKeys(), c => CanImportRSAKeys());
            ExportRSAKeysCommand = new RelayCommand(o => ExportRSAKeys(), c => CanExportRSAKeys());
            RSAEncryptFileCommand = new AsyncCommand(o => RSAEncryptFile(), c => CanRSAEncryptFile());
            RSADencryptFileCommand = new AsyncCommand(o => RSADecryptFile(), c => CanRSADecryptFile());
            RC5EncryptFileCommand = new AsyncCommand(o => RC5EncryptFile(), c => CanRC5EncryptFile());
            RC5DencryptFileCommand = new AsyncCommand(o => RC5DecryptFile(), c => CanRC5DecryptFile());

            _rsaCryptoServiceProvider = new RSACryptoServiceProvider();
            _rc5 = new RC5.RC5(RoundCount.Rounds12, WordBitsLength.Bit64);
        }

        private bool CanChooseFile()
        {
            return !IsInProgress;
        }

        private void ChooseFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose File...";
            openFileDialog.Filter = "All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                FilenameInput = openFileDialog.FileName;

                Status = "Chosen file:";
                Output = FilenameInput;
            }
        }

        private bool CanImportRSAKeys()
        {
            return !IsInProgress;
        }

        private void ImportRSAKeys()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose File...";
            openFileDialog.Filter = "All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _rsaCryptoServiceProvider.FromXmlString(File.ReadAllText(openFileDialog.FileName));

                    Status = "RSA Keys Imported:";
                    Output = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExportRSAKeys()
        {
            return !IsInProgress;
        }

        private void ExportRSAKeys()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save File...";
            saveFileDialog.FileName = "RSAKeys.xml";
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(
                    saveFileDialog.FileName,
                    _rsaCryptoServiceProvider.ToXmlString(includePrivateParameters: true));

                Status = "RSA Keys Exported:";
                Output = saveFileDialog.FileName;
            }
        }

        private bool CanRSAEncryptFile()
        {
            return !(IsInProgress
                || !File.Exists(FilenameInput)
                || string.IsNullOrEmpty(FilenameInput));
        }

        private async Task RSAEncryptFile()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save File...";
            saveFileDialog.FileName = Path.GetFileName(FilenameInput + ".enc");

            if (saveFileDialog.ShowDialog() == true)
            {
                IsInProgress = true;
                Status = "Encrypting...:";

                var rsaWrapper = new RSAWrapper(_rsaEncryptionBlockSize, false);

                try
                {
                    using (var fileToEncrypt = File.OpenRead(FilenameInput))
                    using (var decryptedFile = File.OpenWrite(
                        FilenameInput == saveFileDialog.FileName
                            ? saveFileDialog.FileName + ".enc"
                            : saveFileDialog.FileName))
                    await rsaWrapper.Encrypt(
                        fileToEncrypt,
                        decryptedFile,
                        _rsaCryptoServiceProvider.ExportParameters(_rsaCurrentService == RSAService.Authentication));

                    Status = "File was encrypted!";
                    IsInProgress = false;
                }
                catch (Exception ex)
                {
                    Status = "Encryption failed!";
                    IsInProgress = false;
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanRSADecryptFile()
        {
            return !(IsInProgress
                || !File.Exists(FilenameInput)
                || string.IsNullOrEmpty(FilenameInput));
        }

        private async Task RSADecryptFile()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save File...";
            saveFileDialog.FileName = Path.GetFileName(FilenameInput);

            if (saveFileDialog.ShowDialog() == true)
            {
                IsInProgress = true;
                Status = "Decrypting...:";

                var rsaWrapper = new RSAWrapper(_rsaEncryptionBlockSize, false);
                var selectedFileInfo = new FileInfo(saveFileDialog.FileName);

                try
                {
                    using (var fileToDecrypt = File.OpenRead(FilenameInput))
                    using (var decryptedFile = File.OpenWrite(
                        FilenameInput == saveFileDialog.FileName
                            ? selectedFileInfo.DirectoryName + "\\"
                                + selectedFileInfo.Name + ".dec" + selectedFileInfo.Extension
                            : saveFileDialog.FileName))
                    await rsaWrapper.Decipher(
                        fileToDecrypt,
                        decryptedFile,
                        _rsaCryptoServiceProvider.ExportParameters(_rsaCurrentService == RSAService.Confidentiality));

                    Status = "File was decrypted!";
                    IsInProgress = false;
                }
                catch (Exception ex)
                {
                    Status = "Decryption failed!";
                    IsInProgress = false;
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanRC5DecryptFile()
        {
            return !(IsInProgress
                || !File.Exists(FilenameInput)
                || string.IsNullOrEmpty(FilenameInput));
        }

        private async Task RC5DecryptFile()
        {
            await Task.Run(() =>
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save File...";
                saveFileDialog.FileName = Path.GetFileName(FilenameInput);

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsInProgress = true;
                    Status = "Decrypting...:";

                    var hashedKey = Encoding.UTF8
                        .GetBytes(RC5PasswordInput)
                        .GetMD5HashedKeyForRC5(KeyBytesLength.Bytes8);

                    var decodedFileContent = _rc5.DecipherCBCPAD(
                        File.ReadAllBytes(FilenameInput),
                        hashedKey);

                    var selectedFileInfo = new FileInfo(saveFileDialog.FileName);
                    File.WriteAllBytes(
                        FilenameInput == saveFileDialog.FileName
                            ? selectedFileInfo.DirectoryName + "\\"
                                + selectedFileInfo.Name + ".dec" + selectedFileInfo.Extension
                            : saveFileDialog.FileName,
                        decodedFileContent);

                    Status = "File was decrypted!";
                    IsInProgress = false;
                }
            });
        }

        private bool CanRC5EncryptFile()
        {
            return !(IsInProgress
                || !File.Exists(FilenameInput)
                || string.IsNullOrEmpty(FilenameInput)
                || string.IsNullOrEmpty(RC5PasswordInput));
        }

        private async Task RC5EncryptFile()
        {
            await Task.Run(() =>
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save File...";
                saveFileDialog.FileName = Path.GetFileName(FilenameInput + ".enc");

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsInProgress = true;
                    Status = "Encrypting...:";

                    var hashedKey = Encoding.UTF8
                        .GetBytes(RC5PasswordInput)
                        .GetMD5HashedKeyForRC5(KeyBytesLength.Bytes8);

                    var encodedFileContent = _rc5.EncipherCBCPAD(
                        File.ReadAllBytes(FilenameInput),
                        hashedKey);

                    File.WriteAllBytes(
                        FilenameInput == saveFileDialog.FileName
                            ? saveFileDialog.FileName + ".enc"
                            : saveFileDialog.FileName,
                        encodedFileContent);

                    Status = "File was encrypted!";
                    IsInProgress = false;
                }
            });
        }
    }
}
