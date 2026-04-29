using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace MyWcfService
{
    public class Service1 : IService1
    {
        private readonly string connString = @"Data Source=(localdb)\MSSQLLocalDB;Database=ExchangeDb;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=False";

        // YENİ: Hem UserCurrencies hem de Transactions tablosunu kontrol edip yoksa oluşturur
        private void EnsureTableExists()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 1. Cüzdan Tablosu
                string sqlWallet = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserCurrencies' and xtype='U')
                CREATE TABLE UserCurrencies (
                    Id INT PRIMARY KEY IDENTITY,
                    UserId INT,
                    Currency NVARCHAR(10),
                    Amount DECIMAL(18,4)
                )";
                using (SqlCommand cmd = new SqlCommand(sqlWallet, conn)) { cmd.ExecuteNonQuery(); }

                // 2. YENİ: İşlem Geçmişi (Log) Tablosu (Lab 12 gereksinimi)
                string sqlTransactions = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Transactions' and xtype='U')
                CREATE TABLE Transactions (
                    Id INT PRIMARY KEY IDENTITY,
                    UserId INT,
                    Currency NVARCHAR(10),
                    TransactionType NVARCHAR(10),
                    Amount DECIMAL(18,4),
                    Rate DECIMAL(18,4),
                    TransactionDate DATETIME DEFAULT GETDATE()
                )";
                using (SqlCommand cmd = new SqlCommand(sqlTransactions, conn)) { cmd.ExecuteNonQuery(); }
            }
        }

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

        public bool RegisterUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "INSERT INTO Users (Username, Password) VALUES (@user, @pass)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@pass", password);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { throw new System.ServiceModel.FaultException("SQL Hatası: " + ex.Message); }
            }
        }

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
                catch (Exception ex) { throw new System.ServiceModel.FaultException("Giriş Hatası: " + ex.Message); }
            }
        }

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

        public bool AddFunds(int userId, decimal amount)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "UPDATE Users SET BalancePLN = ISNULL(BalancePLN, 0) + @amount WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@id", userId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch { return false; }
            }
        }

        public bool BuyCurrency(int userId, string currencyCode, decimal amountToBuy, decimal currentRate)
        {
            EnsureTableExists();
            decimal costPLN = amountToBuy * currentRate;
            decimal currentPln = GetBalance(userId);

            if (currentPln < costPLN) return false;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string sqlPln = "UPDATE Users SET BalancePLN = BalancePLN - @cost WHERE Id = @userId";
                    SqlCommand cmdPln = new SqlCommand(sqlPln, conn, transaction);
                    cmdPln.Parameters.AddWithValue("@cost", costPLN);
                    cmdPln.Parameters.AddWithValue("@userId", userId);
                    cmdPln.ExecuteNonQuery();

                    string sqlCheck = "SELECT COUNT(*) FROM UserCurrencies WHERE UserId=@userId AND Currency=@curr";
                    SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn, transaction);
                    cmdCheck.Parameters.AddWithValue("@userId", userId);
                    cmdCheck.Parameters.AddWithValue("@curr", currencyCode);
                    int exists = (int)cmdCheck.ExecuteScalar();

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

                    // YENİ: İşlemi Transactions tablosuna kaydet
                    string sqlLog = "INSERT INTO Transactions (UserId, Currency, TransactionType, Amount, Rate) VALUES (@userId, @curr, 'BUY', @amt, @rate)";
                    SqlCommand cmdLog = new SqlCommand(sqlLog, conn, transaction);
                    cmdLog.Parameters.AddWithValue("@userId", userId);
                    cmdLog.Parameters.AddWithValue("@curr", currencyCode);
                    cmdLog.Parameters.AddWithValue("@amt", amountToBuy);
                    cmdLog.Parameters.AddWithValue("@rate", currentRate);
                    cmdLog.ExecuteNonQuery();

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

        public bool SellCurrency(int userId, string currencyCode, decimal amountToSell, decimal currentRate)
        {
            EnsureTableExists();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string sqlCheck = "SELECT Amount FROM UserCurrencies WHERE UserId=@userId AND Currency=@curr";
                SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn);
                cmdCheck.Parameters.AddWithValue("@userId", userId);
                cmdCheck.Parameters.AddWithValue("@curr", currencyCode);
                object res = cmdCheck.ExecuteScalar();

                if (res == null || Convert.ToDecimal(res) < amountToSell) return false;

                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string sqlUpdate = "UPDATE UserCurrencies SET Amount = Amount - @amt WHERE UserId=@userId AND Currency=@curr";
                    SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn, transaction);
                    cmdUpdate.Parameters.AddWithValue("@amt", amountToSell);
                    cmdUpdate.Parameters.AddWithValue("@userId", userId);
                    cmdUpdate.Parameters.AddWithValue("@curr", currencyCode);
                    cmdUpdate.ExecuteNonQuery();

                    decimal gainPLN = amountToSell * currentRate;
                    string sqlPln = "UPDATE Users SET BalancePLN = BalancePLN + @gain WHERE Id = @userId";
                    SqlCommand cmdPln = new SqlCommand(sqlPln, conn, transaction);
                    cmdPln.Parameters.AddWithValue("@gain", gainPLN);
                    cmdPln.Parameters.AddWithValue("@userId", userId);
                    cmdPln.ExecuteNonQuery();

                    // YENİ: İşlemi Transactions tablosuna kaydet
                    string sqlLog = "INSERT INTO Transactions (UserId, Currency, TransactionType, Amount, Rate) VALUES (@userId, @curr, 'SELL', @amt, @rate)";
                    SqlCommand cmdLog = new SqlCommand(sqlLog, conn, transaction);
                    cmdLog.Parameters.AddWithValue("@userId", userId);
                    cmdLog.Parameters.AddWithValue("@curr", currencyCode);
                    cmdLog.Parameters.AddWithValue("@amt", amountToSell);
                    cmdLog.Parameters.AddWithValue("@rate", currentRate);
                    cmdLog.ExecuteNonQuery();

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

        public List<HistoricalRate> GetHistoricalRates(string currencyCode, int daysCount)
        {
            List<HistoricalRate> list = new List<HistoricalRate>();
            try
            {
                string url = $"http://api.nbp.pl/api/exchangerates/rates/c/{currencyCode.ToLower()}/last/{daysCount}/?format=json";
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string json = client.DownloadString(url);
                    dynamic data = JsonConvert.DeserializeObject(json);

                    foreach (var item in data.rates)
                    {
                        list.Add(new HistoricalRate
                        {
                            Date = item.effectiveDate,
                            BuyRate = item.bid,
                            SellRate = item.ask
                        });
                    }
                }
                list.Reverse();
            }
            catch { }
            return list;
        }
    }
}