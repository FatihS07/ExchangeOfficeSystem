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

        // LAB 7: NBP API Üzerinden Kur Getirme
        private void btnGetRate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string code = txtCurrencyCode.Text.Trim().ToUpper();

                if (string.IsNullOrWhiteSpace(code))
                {
                    txtResult.Foreground = new SolidColorBrush(Colors.Red);
                    txtResult.Text = "Please enter a valid currency code!";
                    txtBuyRate.Text = "Buy (Bid): -";
                    txtSellRate.Text = "Sell (Ask): -";
                    return;
                }

                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();

                txtResult.Foreground = new SolidColorBrush(Colors.DarkOrange);
                txtResult.Text = $"Fetching Bid/Ask rates for {code}...";

                var rateData = client.GetBuySellRates(code);

                if (rateData != null && rateData.BuyRate > 0)
                {
                    txtResult.Foreground = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                    txtResult.Text = $"{code} rates updated from NBP";
                    txtBuyRate.Text = $"Buy (Bid): {rateData.BuyRate} PLN";
                    txtSellRate.Text = $"Sell (Ask): {rateData.SellRate} PLN";
                }
                else
                {
                    txtResult.Foreground = new SolidColorBrush(Colors.Red);
                    txtResult.Text = $"Error: Rates not found for {code}.";
                    txtBuyRate.Text = "Buy (Bid): -";
                    txtSellRate.Text = "Sell (Ask): -";
                }

                client.Close();
            }
            catch (Exception ex)
            {
                txtResult.Foreground = new SolidColorBrush(Colors.Red);
                txtResult.Text = "Connection error: " + ex.Message;
            }
        }

        // LAB 11: WCF Üzerinden Veritabanına Kayıt Yapma
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNewUser.Text) || string.IsNullOrWhiteSpace(txtNewPass.Password))
                {
                    MessageBox.Show("Please fill in all registration fields!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();

                // Servisteki metodu çağırıyoruz
                bool success = client.RegisterUser(txtNewUser.Text, txtNewPass.Password);

                if (success)
                {
                    MessageBox.Show("Registration Successful! Welcome to the bank.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Kutucukları temizleyelim
                    txtNewUser.Clear();
                    txtNewPass.Clear();
                }
                else
                {
                    MessageBox.Show("Registration failed. Please try again or check connection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during registration: " + ex.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}