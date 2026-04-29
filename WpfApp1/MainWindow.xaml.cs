using System;
using System.Windows;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private int currentUserId = 0;

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

        // LAB 11: Kayıt Olma
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
                bool success = client.RegisterUser(txtNewUser.Text, txtNewPass.Password);

                if (success)
                {
                    MessageBox.Show("Registration Successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtNewUser.Clear();
                    txtNewPass.Clear();
                }
                else
                {
                    MessageBox.Show("Registration failed. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during registration: " + ex.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // LAB 9: Giriş Yapma
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLoginUser.Text) || string.IsNullOrWhiteSpace(txtLoginPass.Password))
                {
                    MessageBox.Show("Please enter your username and password.", "Login Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
                int userId = client.LoginUser(txtLoginUser.Text, txtLoginPass.Password);

                if (userId > 0)
                {
                    currentUserId = userId;
                    MessageBox.Show("Login Successful! Welcome back.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    UpdateBalanceUI(); // Yorum satırından kurtardık, artık bakiye güncellenecek!
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection error during login: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // LAB 12: Bakiyeyi Ekrana Yazdırma
        private void UpdateBalanceUI()
        {
            if (currentUserId > 0)
            {
                try
                {
                    ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
                    decimal balance = client.GetBalance(currentUserId);

                    txtBalance.Text = $"{balance} PLN"; // Bakiye buraya yazılıyor

                    client.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not load balance: " + ex.Message);
                }
            }
        }

        // LAB 12: YENİ - Hesaba Para Yükleme (Top-up)
        private void btnTopUp_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserId <= 0)
            {
                MessageBox.Show("Please login first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Textbox'a girilen değeri sayıya (decimal) çevirmeye çalışıyoruz
            if (decimal.TryParse(txtTopUpAmount.Text, out decimal amount) && amount > 0)
            {
                try
                {
                    ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
                    bool success = client.AddFunds(currentUserId, amount);
                    client.Close();

                    if (success)
                    {
                        MessageBox.Show($"Successfully added {amount} PLN to your wallet!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        txtTopUpAmount.Clear();
                        UpdateBalanceUI(); // Para eklendikten sonra ekrandaki bakiyeyi yenile
                    }
                    else
                    {
                        MessageBox.Show("Failed to add funds.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding funds: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid amount (e.g., 100).", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}