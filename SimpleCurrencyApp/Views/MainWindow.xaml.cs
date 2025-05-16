using Kisan.SystemController.Enums;
using Kisan.SystemController.Models.Responses.SC;
using Kisan.SystemController.Models.Responses;
using Kisan.SystemController;
using SimpleCurrencyApp.Models;
using SimpleCurrencyApp.ViewModels;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleCurrencyApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel;
        public MainWindow()
        {
            InitializeComponent();

            _mainViewModel = new MainViewModel();
            DataContext = _mainViewModel;

            Loaded += (s, e) => SCManager.ReceivedCommand += SCEvent;
            Unloaded += (s, e) => SCManager.ReceivedCommand -= SCEvent;
        }

        private void SCEvent(object sender, BaseResponse e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (e.Command)
                {
                    case ScCommand.SC_SendBanknoteInformation:
                        if (e is SendBanknoteInformation banknoteInfo)
                        {
                            /*_banknoteInfo = banknoteInfo;
                            StatusText.Text = "Banknote info received.";
                            count++;
                            WritePropertiesToFile(banknoteInfo, path);*/
                            _mainViewModel.AddBatch(banknoteInfo);
                        }
                        break;

                    case ScCommand.SC_SendEnvelopeDepositResult:
                        if (e is StructResponse<CompleteError> envelopeResult &&
                            envelopeResult.Value == CompleteError.Complete)
                        {
                            //StatusText.Text = "Envelope deposit completed.";
                        }
                        break;

                    case ScCommand.SC_SendJamInformation:
                        //StatusText.Text = "Jam information received.";
                        break;

                    case ScCommand.SC_SendDepositReadyStatus:
                        //StatusText.Text = "Deposit Do Something.";
                        break;

                    case ScCommand.SC_SendCountInformation:
                        //StatusText.Text = "Deposit ended.";
                        _mainViewModel.CalculateBanknotes();
                        break;

                    case ScCommand.SC_HopperOn:
                        //StatusText.Text = "Deposit Putted.";
                        _mainViewModel.SetReadyAsync();
                        _mainViewModel.ReceiveBanknotes();
                        break;

                    case ScCommand.SC_HopperOff:
                        //StatusText.Text = "Deposit Putted.";
                        _mainViewModel.SetUnReadyAsync();
                        break;

                    case ScCommand.SC_RejectOn:
                        //StatusText.Text = "Reject Full.";
                        _mainViewModel.RejectedAsync();
                        break;

                    case ScCommand.SC_RejectOff:
                        //StatusText.Text = "Reject Empity.";
                        _mainViewModel.TakeRejectedAsync();
                        break;

                    default:
                        //StatusText.Text = "Received unknown command.";
                        break;
                }
            });
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            _mainViewModel.Dispose();
        }
    }
}