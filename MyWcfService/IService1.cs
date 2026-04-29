using System.ServiceModel;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace MyWcfService
{
    [ServiceContract]
    public interface IService1
    {
        // Döviz Bürosu için Alış ve Satış kurlarını getiren metot (Lab 7)
        [OperationContract]
        ExchangeRateData GetBuySellRates(string currencyCode);

        // Kullanıcı Kayıt Metodu (Lab 11 - Veritabanı Bağlantısı için)
        [OperationContract]
        bool RegisterUser(string username, string password);

        // Kullanıcı Giriş Metodu (Lab 9 - Sisteme Giriş için)
        [OperationContract]
        int LoginUser(string username, string password);

        // Kullanıcının PLN bakiyesini getir (Lab 12)
        [OperationContract]
        decimal GetBalance(int userId);

        // Kullanıcının hesabına PLN yükle (Lab 12 - Top-up)
        [OperationContract]
        bool AddFunds(int userId, decimal amount);

        // LAB 10: YENİ - Döviz Alım ve Satım Metotları
        [OperationContract]
        bool BuyCurrency(int userId, string currencyCode, decimal amountToBuy, decimal currentRate);

        [OperationContract]
        bool SellCurrency(int userId, string currencyCode, decimal amountToSell, decimal currentRate);

        // LAB 12: YENİ - Kullanıcının Sahip Olduğu Dövizleri Getirme
        [OperationContract]
        List<WalletItem> GetUserCurrencies(int userId);
    }

    // Hem Alış hem Satış değerini aynı anda gönderebilmek için oluşturduğumuz Veri Modeli
    [DataContract]
    public class ExchangeRateData
    {
        [DataMember]
        public string Currency { get; set; }

        [DataMember]
        public double BuyRate { get; set; }  // Döviz Bürosunun Alış Fiyatı (Bid)

        [DataMember]
        public double SellRate { get; set; } // Döviz Bürosunun Satış Fiyatı (Ask)
    }

    // YENİ: Sahip olunan dövizleri listede göstermek için Veri Modeli
    [DataContract]
    public class WalletItem
    {
        [DataMember]
        public string Currency { get; set; }

        [DataMember]
        public decimal Amount { get; set; }
    }
}