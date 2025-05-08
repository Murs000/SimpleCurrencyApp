using Kisan.SystemController;
using Kisan.SystemController.Models.Responses.SC;
using SimpleCurrencyApp.Commands;
using SimpleCurrencyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        //private List<string> _errors = new();  

        private readonly SCManager _manager;
        public ObservableCollection<BanknoteInfo> Banknotes { get; set; } = new();

        private RelayCommand _startReceivingCommand;
        public ICommand StartReceivingCommand => _startReceivingCommand ??= new RelayCommand(ReceiveBanknotes, () => CanReceive);

        private RelayCommand _readyCommand;
        public ICommand ReadyCommand => _readyCommand ??= new RelayCommand(() => CanReceive = true);

        private bool _canReceive;
        public bool CanReceive
        {
            get => _canReceive;
            set
            {
                _canReceive = value;
                OnPropertyChanged();
                _startReceivingCommand?.RaiseCanExecuteChanged();
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

        public MainViewModel()
        {
            _manager = new SCManager();
            _manager = new SCManager("COM25");
            _manager.ConnectAsync(false);
        }
        public void AddBatch(SendBanknoteInformation banknoteInfo)
        {
            if(banknoteInfo.BanknoteError != Kisan.SystemController.Enums.BanknoteError.RECOGNITION_ERROR)
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

                //_errors.Add($"BanknoteError - {banknoteInfo.BanknoteError.ToString()}");
                //_errors.Add($"RecoError - {banknoteInfo.RecoError.ToString()}");

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
        private int _currentBatchId = 1;

        private async void ReceiveBanknotes()
        {
            await _manager.SendUserReadyCompleteAsync();
            await _manager.SendDepositStartAsync();
        }
        public void SetReady()
        {
            CanReceive = true;
        }
        public void SetUnReady()
        {
            CanReceive = false;
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

            StringBuilder sb = new StringBuilder();

            if (_batch.Count > 0)
            {
                sb.Append($"Received batch: {string.Join(", ", _batch.Select(b => $"{b.Value} x {b.Key} AZN"))}\n");
            }

            if (_rejectBatch.Count > 0)
            {
                sb.Append($"Rejected batch:{ string.Join(", ", _rejectBatch.Select(rB => $"{rB.Value} x {rB.Key} AZN"))}\n");
            }

            /*if (_errors.Count > 0)
            {
                sb.Append($"Rejected batch Errors:{string.Join(", ", _errors)}");
            }*/

            LastReceived = sb.ToString();
            sb.Clear();

            _currentBatchId++;

            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(Banknotes));

            _batch = new();
            _rejectBatch = new();

            CanReceive = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
