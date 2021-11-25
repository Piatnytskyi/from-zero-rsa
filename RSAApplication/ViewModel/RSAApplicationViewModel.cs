using Microsoft.Win32;
using RC5.Enums;
using RC5.Extensions;
using RSAApplication.Commands;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RSAApplication.ViewModel
{
    class RSAApplicationViewModel : AbstractViewModel
    {
        private readonly RC5.RC5 _rc5;

        private string _passwordInput = string.Empty;

        public string PasswordInput
        {
            get => _passwordInput;
            set
            {
                if (_passwordInput.Equals(value))
                {
                    return;
                }
                _passwordInput = value;
                RaisePropertyChanged(nameof(PasswordInput));
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

        public RelayCommand ChooseFileToEncryptCommand { get; set; }
        public RelayCommand SaveEncryptedFileCommand { get; set; }
        public AsyncCommand EncryptFileCommand { get; set; }
        public AsyncCommand DencryptFileCommand { get; set; }

        public RSAApplicationViewModel()
        {
            ChooseFileToEncryptCommand = new RelayCommand(o => ChooseFileToEncrypt(), c => CanChooseFileToEncrypt());
            SaveEncryptedFileCommand = new RelayCommand(o => SaveEncryptedFile(), c => CanSaveEncryptedFile());
            EncryptFileCommand = new AsyncCommand(o => EncryptFile(), c => CanEncryptFile());
            DencryptFileCommand = new AsyncCommand(o => DecryptFile(), c => CanDecryptFile());

            _rc5 = new RC5.RC5(RoundCount.Rounds12, WordBitsLength.Bit64);
        }

        private bool CanSaveEncryptedFile()
        {
            return (!IsInProgress
                || !string.IsNullOrEmpty(FilenameInput))
                && File.Exists(FilenameInput + ".enc");
        }

        private void SaveEncryptedFile()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save File...";
            saveFileDialog.FileName = Path.GetFileName(FilenameInput + ".enc");

            if (saveFileDialog.ShowDialog() == true && (FilenameInput + ".enc") != saveFileDialog.FileName)
            {
                File.Move(FilenameInput + ".enc", saveFileDialog.FileName);
                CleanTemporaryFiles();

                Status = "Encrypted file saved:";
            }
        }

        private bool CanDecryptFile()
        {
            return !IsInProgress
                || !string.IsNullOrEmpty(FilenameInput);
        }

        private async Task DecryptFile()
        {
            await Task.Run(() =>
            {
                IsInProgress = true;
                Status = "Decrypting...:";

                var hashedKey = Encoding.UTF8
                    .GetBytes(PasswordInput)
                    .GetMD5HashedKeyForRC5(KeyBytesLength.Bytes8);

                var decodedFileContent = _rc5.DecipherCBCPAD(
                    File.ReadAllBytes(FilenameInput),
                    hashedKey);

                File.WriteAllBytes(FilenameInput.Replace(".enc", ""), decodedFileContent);

                Status = "File was decrypted!";
                IsInProgress = false;
            });
        }

        private bool CanEncryptFile()
        {
            return !(IsInProgress
                || string.IsNullOrEmpty(FilenameInput)
                || string.IsNullOrEmpty(PasswordInput)
                || File.Exists(FilenameInput + ".enc"));
        }

        private async Task EncryptFile()
        {
            await Task.Run(() =>
            {
                IsInProgress = true;
                Status = "Encrypting...:";

                var hashedKey = Encoding.UTF8
                    .GetBytes(PasswordInput)
                    .GetMD5HashedKeyForRC5(KeyBytesLength.Bytes8);

                var encodedFileContent = _rc5.EncipherCBCPAD(
                    File.ReadAllBytes(FilenameInput),
                    hashedKey);

                File.WriteAllBytes(FilenameInput + ".enc", encodedFileContent);

                Status = "File was encrypted!";
                IsInProgress = false;
            });
        }

        private bool CanChooseFileToEncrypt()
        {
            return !IsInProgress;
        }

        private void ChooseFileToEncrypt()
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

        private void CleanTemporaryFiles()
        {
            File.Delete(FilenameInput + ".enc");
        }
    }
}
