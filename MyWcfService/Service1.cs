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

        // LAB 11: Kullanıcı Kayıt (Register) Metodu
        public bool RegisterUser(string username, string password)
        {
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
                    throw new System.ServiceModel.FaultException("SQL Hatası: " + ex.Message);
                }
            }
        }

        // LAB 9: YENİ - Kullanıcı Giriş (Login) Metodu
        public int LoginUser(string username, string password)
        {
            // Kayıt işleminde sorunsuz çalışan aynı bağlantı dizesini kullanıyoruz
            string connString = @"Data Source=(localdb)\MSSQLLocalDB;Database=ExchangeDb;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=False";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Kullanıcı adı ve şifre eşleşirse, kullanıcının benzersiz ID'sini (kimliğini) getirir
                string sql = "SELECT Id FROM Users WHERE Username = @user AND Password = @pass";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@pass", password);

                try
                {
                    conn.Open();
                    // Sadece tek bir değer (Id) döneceği için ExecuteScalar kullanıyoruz
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // Giriş başarılı, veritabanındaki Id değerini döndür (Örn: 1, 2, 3...)
                        return Convert.ToInt32(result);
                    }

                    // Eğer sonuç null ise, kullanıcı adı veya şifre yanlıştır. 0 döndür.
                    return 0;
                }
                catch (Exception ex)
                {
                    // Bağlantı hatası olursa WPF tarafına fırlat
                    throw new System.ServiceModel.FaultException("Giriş Hatası: " + ex.Message);
                }
            }
        }
    }
}