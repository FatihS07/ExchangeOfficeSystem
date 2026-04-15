using System;
using System.Windows;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Butona tıklandığında çalışacak olan metot
        private void btnGetRate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string code = txtCurrencyCode.Text;

                if (string.IsNullOrWhiteSpace(code))
                {
                    txtResult.Foreground = new SolidColorBrush(Colors.Red);
                    txtResult.Text = "Please enter a valid currency code!";
                    return;
                }

                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();

                txtResult.Foreground = new SolidColorBrush(Colors.DarkOrange);
                txtResult.Text = $"Fetching rate for {code}...";

                double rate = client.GetExchangeRate(code);

                if (rate > 0)
                {
                    // Başarılı (Yeşil renk)
                    txtResult.Foreground = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                    txtResult.Text = $"Current Rate: 1 {code} = {rate} PLN";
                }
                else
                {
                    // Hatalı kod (Kırmızı renk)
                    txtResult.Foreground = new SolidColorBrush(Colors.Red);
                    txtResult.Text = "Error: Currency rate not found.";
                }

                client.Close();
            }
            catch (Exception ex)
            {
                // Bağlantı veya servis hatası
                txtResult.Foreground = new SolidColorBrush(Colors.Red);
                txtResult.Text = "Connection error: " + ex.Message;
            }
        }
    }
}