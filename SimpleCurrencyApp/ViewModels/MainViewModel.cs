using Kisan.SystemController;
using Kisan.SystemController.Enums;
using Kisan.SystemController.Models.Responses.SC;
using SimpleCurrencyApp.Commands;
using SimpleCurrencyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleCurrencyApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Dictionary<int, int> _batch = new();
        private Dictionary<int, int> _rejectBatch = new();
        private List<string> _errors = new();

        private readonly SCManager _manager;
        public ObservableCollection<BanknoteInfo> Banknotes { get; set; } = new();

        private string _depositText = "Enter Deposit";
        public string DepositText
        {
            get => _depositText;
            set
            {
                _depositText = value;
                OnPropertyChanged();
            }
        }
        private string _rejectText;
        public string RejectText
        {
            get => _rejectText;
            set
            {
                _rejectText = value;
                OnPropertyChanged();
            }
        }
        

        public uint TotalAmount => (uint)Banknotes.Sum(x => x.Total);
        public uint TotalCount => (uint)Banknotes.Sum(x => x.Count);

        // Last received info (bottom display)
        private string _lastReceived;
        public string LastReceived
        {
            get => _lastReceived;
            set { _lastReceived = value; OnPropertyChanged(); }
        }

        private string _connectionStatus;
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    OnPropertyChanged(nameof(ConnectionStatus));
                }
            }
        }

        public MainViewModel()
        {
            _manager = new SCManager("COM25");
            ConnectionStatus = "Connecting...";

            ConnectAsync();
        }

        private async void ConnectAsync()
        {
            try
            {
                await _manager.ConnectAsync(false); // false = do not reconnect automatically
                string port = _manager.PortName;
                ConnectionStatus = $"Connected To {port}";

                await Task.Delay(5000);
                ConnectionStatus = "";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Failed: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _manager.Dispose();
            ConnectionStatus = "Disconnected";
        }

        private int _currentBatchId = 1;
        public void AddBatch(SendBanknoteInformation banknoteInfo)
        {
            if(banknoteInfo.BanknoteError == BanknoteError.NO_ERROR)
            {
                int denomination = (int)banknoteInfo.BanknoteCode.GetDenomAmount();

                if (_batch.ContainsKey(denomination))
                {
                    _batch[denomination]++;
                }
                else
                {
                    _batch[denomination] = 1;
                }
            }
            else
            {
                int denomination = (int)banknoteInfo.BanknoteCode.GetDenomAmount();

                _errors.Add($"BanknoteError - {banknoteInfo.BanknoteError.ToString()}");
                _errors.Add($"RecoError - {banknoteInfo.RecoError.ToString()}");

                if (_rejectBatch.ContainsKey(denomination))
                {
                    _rejectBatch[denomination]++;
                }
                else
                {
                    _rejectBatch[denomination] = 1;
                }
            }
        }
        

        public async void ReceiveBanknotes()
        {
            try
            {
                //await _manager.SendUserReadyCompleteAsync();
                await _manager.SendDepositStartAsync();
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\Users\ADMIN\Desktop\Error.txt",$"{DateTime.Now} : {ex.Message}");
                DepositText = ex.Message;
            }
        }
        public async void SetReadyAsync()
        {
            DepositText = "Processing..";
        }

        public async void SetUnReadyAsync()
        {
            DepositText = "Enter Deposit";
        }

        public async void RejectedAsync()
        {
            RejectText = "Take Money From Reject Section";
        }

        public async void TakeRejectedAsync()
        {
            RejectText = "";
        }

        public void CalculateBanknotes()
        {
            foreach (var entry in _batch)
            {
                int denomination = entry.Key;
                int count = entry.Value;

                Banknotes.Add(new BanknoteInfo
                {
                    Denomination = denomination,
                    Count = count,
                    Currency = "AZN",
                    BatchId = _currentBatchId
                });
            }

            StringBuilder sB = new StringBuilder();

            if (_batch.Count > 0)
            {
                sB.Append($"Received batch: {string.Join(", ", _batch.Select(b => $"{b.Value} x {b.Key} AZN"))}\n");
            }

            if (_rejectBatch.Count > 0)
            {
                sB.Append($"Rejected batch:{ string.Join(", ", _rejectBatch.Select(rB => $"{rB.Value} x {rB.Key} AZN"))}\n");
            }

            LastReceived = sB.ToString();
            sB.Clear();

            WriteErrors();

            _currentBatchId++;

            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(Banknotes));

            _batch = new();
            _rejectBatch = new();
        }
        private void WriteErrors()
        {
            StringBuilder sB = new StringBuilder();

            if (_errors.Count > 0)
            {
                sB.Append($"Rejected batch Errors:{string.Join("", _errors.Select(e => $"{e}\n"))}");
            }

            File.AppendAllTextAsync(@"C:\Users\ADMIN\Desktop\Error.txt", sB.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
