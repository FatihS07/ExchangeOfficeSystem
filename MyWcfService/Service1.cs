using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic; // YENİ: Listeler için gerekli

namespace MyWcfService
{
    public class Service1 : IService1
    {
        private readonly string connString = @"Data Source=(localdb)\MSSQLLocalDB;Database=ExchangeDb;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=False";

        // YENİ: Otomatik Tablo Oluşturucu (Eğer UserCurrencies tablosu yoksa yaratır)
        private void EnsureTableExists()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserCurrencies' and xtype='U')
                CREATE TABLE UserCurrencies (
                    Id INT PRIMARY KEY IDENTITY,
                    UserId INT,
                    Currency NVARCHAR(10),
                    Amount DECIMAL(18,4)
                )";
                try
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch { } // Tablo zaten varsa sorun yok
            }
        }

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

        // LAB 11: Kayıt
        public bool RegisterUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "INSERT INTO Users (Username, Password) VALUES (@user, @pass)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@pass", password);
                try
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    throw new System.ServiceModel.FaultException("SQL Hatası: " + ex.Message);
                }
            }
        }

        // LAB 9: Giriş
        public int LoginUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT Id FROM Users WHERE Username = @user AND Password = @pass";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@pass", password);
                try
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null) return Convert.ToInt32(result);
                    return 0;
                }
                catch (Exception ex)
                {
                    throw new System.ServiceModel.FaultException("Giriş Hatası: " + ex.Message);
                }
            }
        }

        // LAB 12: Bakiye Getir
        public decimal GetBalance(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT BalancePLN FROM Users WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                try
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value) return Convert.ToDecimal(result);
                    return 0;
                }
                catch { return -1; }
            }
        }

        // LAB 12: Para Yükle
        public bool AddFunds(int userId, decimal amount)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "UPDATE Users SET BalancePLN = ISNULL(BalancePLN, 0) + @amount WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@id", userId);
                try
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        // ==========================================
        // YENİ EKLENEN METOTLAR (Hataları çözen kısım)
        // ==========================================

        // LAB 10: Döviz Satın Alma (Buy)
        public bool BuyCurrency(int userId, string currencyCode, decimal amountToBuy, decimal currentRate)
        {
            EnsureTableExists();
            decimal costPLN = amountToBuy * currentRate;
            decimal currentPln = GetBalance(userId);

            if (currentPln < costPLN) return false; // Parası yetmiyorsa işlemi iptal et

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction(); // Hata olursa işlemi geri alabilmek için
                try
                {
                    // 1. PLN bakiyesinden parayı düş
                    string sqlPln = "UPDATE Users SET BalancePLN = BalancePLN - @cost WHERE Id = @userId";
                    SqlCommand cmdPln = new SqlCommand(sqlPln, conn, transaction);
                    cmdPln.Parameters.AddWithValue("@cost", costPLN);
                    cmdPln.Parameters.AddWithValue("@userId", userId);
                    cmdPln.ExecuteNonQuery();

                    // 2. Kullanıcının o dövizden cüzdanında var mı kontrol et
                    string sqlCheck = "SELECT COUNT(*) FROM UserCurrencies WHERE UserId=@userId AND Currency=@curr";
                    SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn, transaction);
                    cmdCheck.Parameters.AddWithValue("@userId", userId);
                    cmdCheck.Parameters.AddWithValue("@curr", currencyCode);
                    int exists = (int)cmdCheck.ExecuteScalar();

                    // 3. Varsa üzerine ekle, yoksa yeni kayıt oluştur
                    if (exists > 0)
                    {
                        string sqlUpdate = "UPDATE UserCurrencies SET Amount = Amount + @amt WHERE UserId=@userId AND Currency=@curr";
                        SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn, transaction);
                        cmdUpdate.Parameters.AddWithValue("@amt", amountToBuy);
                        cmdUpdate.Parameters.AddWithValue("@userId", userId);
                        cmdUpdate.Parameters.AddWithValue("@curr", currencyCode);
                        cmdUpdate.ExecuteNonQuery();
                    }
                    else
                    {
                        string sqlInsert = "INSERT INTO UserCurrencies (UserId, Currency, Amount) VALUES (@userId, @curr, @amt)";
                        SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn, transaction);
                        cmdInsert.Parameters.AddWithValue("@amt", amountToBuy);
                        cmdInsert.Parameters.AddWithValue("@userId", userId);
                        cmdInsert.Parameters.AddWithValue("@curr", currencyCode);
                        cmdInsert.ExecuteNonQuery();
                    }

                    transaction.Commit(); // İşlemleri onayla
                    return true;
                }
                catch
                {
                    transaction.Rollback(); // Hata olursa parayı iade et
                    return false;
                }
            }
        }

        // LAB 10: Döviz Satma (Sell)
        public bool SellCurrency(int userId, string currencyCode, decimal amountToSell, decimal currentRate)
        {
            EnsureTableExists();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                // Kullanıcının yeterli dövizi var mı kontrol et
                string sqlCheck = "SELECT Amount FROM UserCurrencies WHERE UserId=@userId AND Currency=@curr";
                SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn);
                cmdCheck.Parameters.AddWithValue("@userId", userId);
                cmdCheck.Parameters.AddWithValue("@curr", currencyCode);
                object res = cmdCheck.ExecuteScalar();

                if (res == null || Convert.ToDecimal(res) < amountToSell) return false; // Yeterli dövizi yok

                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    // 1. Döviz miktarını düş
                    string sqlUpdate = "UPDATE UserCurrencies SET Amount = Amount - @amt WHERE UserId=@userId AND Currency=@curr";
                    SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn, transaction);
                    cmdUpdate.Parameters.AddWithValue("@amt", amountToSell);
                    cmdUpdate.Parameters.AddWithValue("@userId", userId);
                    cmdUpdate.Parameters.AddWithValue("@curr", currencyCode);
                    cmdUpdate.ExecuteNonQuery();

                    // 2. Satış gelirini PLN bakiyesine ekle
                    decimal gainPLN = amountToSell * currentRate;
                    string sqlPln = "UPDATE Users SET BalancePLN = BalancePLN + @gain WHERE Id = @userId";
                    SqlCommand cmdPln = new SqlCommand(sqlPln, conn, transaction);
                    cmdPln.Parameters.AddWithValue("@gain", gainPLN);
                    cmdPln.Parameters.AddWithValue("@userId", userId);
                    cmdPln.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        // LAB 12: Sahip Olunan Dövizleri Listeleme
        public List<WalletItem> GetUserCurrencies(int userId)
        {
            EnsureTableExists();
            List<WalletItem> list = new List<WalletItem>();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT Currency, Amount FROM UserCurrencies WHERE UserId = @id AND Amount > 0";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(new WalletItem
                        {
                            Currency = reader["Currency"].ToString(),
                            Amount = Convert.ToDecimal(reader["Amount"])
                        });
                    }
                }
                catch { }
            }
            return list;
        }
    }
}