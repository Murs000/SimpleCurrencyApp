using SimpleCurrencyApp.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SimpleCurrencyApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }

}
