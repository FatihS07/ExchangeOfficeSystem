using System;
using System.Windows;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private int currentUserId = 0;

        // Alım-satım işlemleri için son sorgulanan kuru hafızada tutuyoruz
        private string currentCurrency = "";
        private decimal currentBuyRate = 0;
        private decimal currentSellRate = 0;

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
                    return;
                }

                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
                txtResult.Foreground = new SolidColorBrush(Colors.DarkOrange);
                txtResult.Text = $"Fetching rates for {code}...";

                var rateData = client.GetBuySellRates(code);

                if (rateData != null && rateData.BuyRate > 0)
                {
                    txtResult.Foreground = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                    txtResult.Text = $"{code} rates updated from NBP";
                    txtBuyRate.Text = $"Buy (Bid): {rateData.BuyRate} PLN";
                    txtSellRate.Text = $"Sell (Ask): {rateData.SellRate} PLN";

                    // Alım/Satım için değerleri hafızaya al
                    currentCurrency = code;
                    currentBuyRate = Convert.ToDecimal(rateData.BuyRate);
                    currentSellRate = Convert.ToDecimal(rateData.SellRate);

                    // Yeni kur sorgulandığında geçmiş listesini gizle
                    if (lstHistory != null) lstHistory.Visibility = Visibility.Collapsed;
                }
                else
                {
                    txtResult.Foreground = new SolidColorBrush(Colors.Red);
                    txtResult.Text = $"Error: Rates not found for {code}.";
                    currentCurrency = "";
                }
                client.Close();
            }
            catch (Exception ex)
            {
                txtResult.Foreground = new SolidColorBrush(Colors.Red);
                txtResult.Text = "Connection error: " + ex.Message;
            }
        }

        // LAB 13: YENİ - Geçmiş Kurları (Son 7 Gün) Getirme
        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            string code = txtCurrencyCode.Text.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Please enter a currency code first (e.g., USD).", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
                // Son 7 günün verisini çekiyoruz
                var history = client.GetHistoricalRates(code, 7);
                client.Close();

                if (history != null && history.Length > 0)
                {
                    lstHistory.ItemsSource = history;
                    lstHistory.Visibility = Visibility.Visible; // Listeyi görünür yap
                }
                else
                {
                    MessageBox.Show("Could not find historical data for this currency.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching history: " + ex.Message);
            }
        }

        // LAB 11: Kayıt Olma
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNewUser.Text) || string.IsNullOrWhiteSpace(txtNewPass.Password)) return;

                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
                bool success = client.RegisterUser(txtNewUser.Text, txtNewPass.Password);

                if (success)
                {
                    MessageBox.Show("Registration Successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtNewUser.Clear();
                    txtNewPass.Clear();
                }
                client.Close();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        // LAB 9: Giriş Yapma
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLoginUser.Text) || string.IsNullOrWhiteSpace(txtLoginPass.Password)) return;

                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
                int userId = client.LoginUser(txtLoginUser.Text, txtLoginPass.Password);

                if (userId > 0)
                {
                    currentUserId = userId;
                    MessageBox.Show("Login Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateBalanceUI();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                client.Close();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        // LAB 12: Bakiyeyi ve Sahip Olunan Dövizleri Ekrana Yazdırma
        private void UpdateBalanceUI()
        {
            if (currentUserId > 0)
            {
                try
                {
                    ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();

                    // 1. PLN Bakiyesini Güncelle
                    decimal balance = client.GetBalance(currentUserId);
                    txtBalance.Text = $"{balance} PLN";

                    // 2. Sahip Olunan Dövizleri (Cüzdanı) Güncelle
                    var myCurrencies = client.GetUserCurrencies(currentUserId);
                    lstWallet.ItemsSource = myCurrencies; // ListBox'a verileri bağla

                    client.Close();
                }
                catch { }
            }
        }

        // Hesaba Para Yükleme (Top-up)
        private void btnTopUp_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserId <= 0) { MessageBox.Show("Please login first!"); return; }

            if (decimal.TryParse(txtTopUpAmount.Text, out decimal amount) && amount > 0)
            {
                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
                bool success = client.AddFunds(currentUserId, amount);
                client.Close();

                if (success)
                {
                    txtTopUpAmount.Clear();
                    UpdateBalanceUI();
                }
            }
        }

        // LAB 10: Döviz Satın Alma Butonu (Banka bize döviz satar = Ask Rate)
        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserId <= 0) { MessageBox.Show("Please login first!"); return; }
            if (string.IsNullOrEmpty(currentCurrency)) { MessageBox.Show("Please get a currency rate first!"); return; }

            if (decimal.TryParse(txtTransactionAmount.Text, out decimal amountToBuy) && amountToBuy > 0)
            {
                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
                // Döviz bürosu dövizi bize Ask(Satış) fiyatından satar.
                bool success = client.BuyCurrency(currentUserId, currentCurrency, amountToBuy, currentSellRate);
                client.Close();

                if (success)
                {
                    MessageBox.Show($"Successfully bought {amountToBuy} {currentCurrency}!", "Transaction Success");
                    txtTransactionAmount.Clear();
                    UpdateBalanceUI(); // Cüzdanı yenile
                }
                else
                {
                    MessageBox.Show("Transaction failed! Do you have enough PLN?", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // LAB 10: Döviz Satma Butonu (Banka bizden dövizi alır = Bid Rate)
        private void btnSell_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserId <= 0) { MessageBox.Show("Please login first!"); return; }
            if (string.IsNullOrEmpty(currentCurrency)) { MessageBox.Show("Please get a currency rate first!"); return; }

            if (decimal.TryParse(txtTransactionAmount.Text, out decimal amountToSell) && amountToSell > 0)
            {
                ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
                // Biz dövizi satarken döviz bürosu Bid(Alış) fiyatından alır.
                bool success = client.SellCurrency(currentUserId, currentCurrency, amountToSell, currentBuyRate);
                client.Close();

                if (success)
                {
                    MessageBox.Show($"Successfully sold {amountToSell} {currentCurrency}!", "Transaction Success");
                    txtTransactionAmount.Clear();
                    UpdateBalanceUI(); // Cüzdanı yenile
                }
                else
                {
                    MessageBox.Show($"Transaction failed! Do you have enough {currentCurrency} in your wallet?", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}