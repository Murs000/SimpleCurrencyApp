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

        private readonly Random _random = new();
        private Dictionary<int, int> GenerateMockedMoneyBatch()
        {
            int[] possibleDenominations = { 1, 5, 10, 20, 50, 100 };
            Dictionary<int, int> batch = new();

            int batchSize = _random.Next(3, 6); // Number of different denominations

            for (int i = 0; i < batchSize; i++)
            {
                int denomination = possibleDenominations[_random.Next(possibleDenominations.Length)];
                int count = _random.Next(1, 5); // 1 to 4 notes of that denomination

                if (batch.ContainsKey(denomination))
                    batch[denomination] += count;
                else
                    batch[denomination] = count;
            }

            return batch;
        }
        private int _currentBatchId = 1;

        private void ReceiveBanknotes()
        {
            var batch = GenerateMockedMoneyBatch();

            foreach (var entry in batch)
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

            LastReceived = $"Received batch {_currentBatchId}: {string.Join(", ", batch.Select(b => $"{b.Value} x {b.Key} AZN"))}";
            _currentBatchId++;

            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(Banknotes));

            CanReceive = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
