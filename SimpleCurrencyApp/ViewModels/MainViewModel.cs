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

        private void ReceiveBanknotes()
        {
            int[] possibleDenominations = { 1, 5, 10, 20, 50, 100 };
            int denomination = possibleDenominations[_random.Next(possibleDenominations.Length)];
            int count = _random.Next(1, 4); // random 1-3 banknotes

            var existing = Banknotes.FirstOrDefault(x => x.Denomination == denomination && x.Currency == "AZN");
            if (existing != null)
                existing.Count += (uint)count;
            else
                Banknotes.Add(new BanknoteInfo { Denomination = (uint)denomination, Count = (uint)count, Currency = "AZN" });

            LastReceived = $"Received: {count} x {denomination} AZN";

            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(TotalCount));

            // Disable after receiving
            CanReceive = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
