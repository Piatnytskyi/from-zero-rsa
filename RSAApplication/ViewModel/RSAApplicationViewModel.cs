using Microsoft.Win32;
using RC5.Enums;
using RC5.Extensions;
using RSAApplication.Commands;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RSAApplication.ViewModel
{
    class RSAApplicationViewModel : AbstractViewModel
    {
        private readonly RC5.RC5 _rc5;

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
            ChooseFileCommand = new RelayCommand(o => ChooseFile(), c => CanChooseFile());
            ImportRSAKeysCommand = new RelayCommand(o => ImportRSAKeys(), c => CanImportRSAKeys());
            ExportRSAKeysCommand = new RelayCommand(o => ExportRSAKeys(), c => CanExportRSAKeys());
            RSAEncryptFileCommand = new AsyncCommand(o => RSAEncryptFile(), c => CanRSAEncryptFile());
            RSADencryptFileCommand = new AsyncCommand(o => RSADecryptFile(), c => CanRSADecryptFile());
            RC5EncryptFileCommand = new AsyncCommand(o => RC5EncryptFile(), c => CanRC5EncryptFile());
            RC5DencryptFileCommand = new AsyncCommand(o => RC5DecryptFile(), c => CanRC5DecryptFile());

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
                CleanTemporaryFiles();

                Status = "Chosen file:";
                Output = FilenameInput;
            }
        }

        private bool CanImportRSAKeys()
        {
            return true;
        }

        private void ImportRSAKeys()
        {
            throw new NotImplementedException();
        }

        private bool CanExportRSAKeys()
        {
            return true;
        }

        private void ExportRSAKeys()
        {
            throw new NotImplementedException();
        }

        private bool CanRSAEncryptFile()
        {
            return true;
        }

        private Task RSAEncryptFile()
        {
            throw new NotImplementedException();
        }

        private bool CanRSADecryptFile()
        {
            return true;
        }

        private Task RSADecryptFile()
        {
            throw new NotImplementedException();
        }

        private bool CanRC5DecryptFile()
        {
            return !IsInProgress
                || !string.IsNullOrEmpty(FilenameInput);
        }

        private async Task RC5DecryptFile()
        {
            await Task.Run(() =>
            {
                IsInProgress = true;
                Status = "Decrypting...:";

                var hashedKey = Encoding.UTF8
                    .GetBytes(RC5PasswordInput)
                    .GetMD5HashedKeyForRC5(KeyBytesLength.Bytes8);

                var decodedFileContent = _rc5.DecipherCBCPAD(
                    File.ReadAllBytes(FilenameInput),
                    hashedKey);

                File.WriteAllBytes(FilenameInput.Replace(".enc", ""), decodedFileContent);

                Status = "File was decrypted!";
                IsInProgress = false;
            });
        }

        private bool CanRC5EncryptFile()
        {
            return !(IsInProgress
                || string.IsNullOrEmpty(FilenameInput)
                || string.IsNullOrEmpty(RC5PasswordInput)
                || File.Exists(FilenameInput + ".enc"));
        }

        private async Task RC5EncryptFile()
        {
            await Task.Run(() =>
            {
                IsInProgress = true;
                Status = "Encrypting...:";

                var hashedKey = Encoding.UTF8
                    .GetBytes(RC5PasswordInput)
                    .GetMD5HashedKeyForRC5(KeyBytesLength.Bytes8);

                var encodedFileContent = _rc5.EncipherCBCPAD(
                    File.ReadAllBytes(FilenameInput),
                    hashedKey);

                File.WriteAllBytes(FilenameInput + ".enc", encodedFileContent);

                Status = "File was encrypted!";
                IsInProgress = false;
            });
        }

        private void CleanTemporaryFiles()
        {
            File.Delete(FilenameInput + ".enc");
        }
    }
}
