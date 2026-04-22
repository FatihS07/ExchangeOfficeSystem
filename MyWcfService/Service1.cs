using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace MyWcfService
{
    public class Service1 : IService1
    {
        // LAB 7: NBP API'den Döviz Kuru Çekme
        public ExchangeRateData GetBuySellRates(string currencyCode)
        {
            try
            {
                string url = $"http://api.nbp.pl/api/exchangerates/rates/c/{currencyCode.ToLower()}/?format=json";
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string json = client.DownloadString(url);
                    dynamic data = JsonConvert.DeserializeObject(json);
                    return new ExchangeRateData
                    {
                        Currency = currencyCode.ToUpper(),
                        BuyRate = data.rates[0].bid,
                        SellRate = data.rates[0].ask
                    };
                }
            }
            catch { return new ExchangeRateData { BuyRate = 0, SellRate = 0 }; }
        }

        // LAB 11: Şok Etkisi - Generic Bağlantı Dizesi
        public bool RegisterUser(string username, string password)
        {
            // Initial Catalog yerine Database kullandık. 
            // Güvenlik el sıkışmalarını (handshake) atlamak için Trusted_Connection=True ve Encrypt=False yapıldı.
            string connString = @"Data Source=(localdb)\MSSQLLocalDB;Database=ExchangeDb;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=False";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "INSERT INTO Users (Username, Password) VALUES (@user, @pass)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@pass", password);

                try
                {
                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    // Hata mesajını WPF'e fırlatıyoruz
                    throw new System.ServiceModel.FaultException("SQL Hatası: " + ex.Message);
                }
            }
        }
    }
}