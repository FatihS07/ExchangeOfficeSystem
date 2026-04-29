using System.ServiceModel;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace MyWcfService
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        ExchangeRateData GetBuySellRates(string currencyCode);

        [OperationContract]
        bool RegisterUser(string username, string password);

        [OperationContract]
        int LoginUser(string username, string password);

        [OperationContract]
        decimal GetBalance(int userId);

        [OperationContract]
        bool AddFunds(int userId, decimal amount);

        [OperationContract]
        bool BuyCurrency(int userId, string currencyCode, decimal amountToBuy, decimal currentRate);

        [OperationContract]
        bool SellCurrency(int userId, string currencyCode, decimal amountToSell, decimal currentRate);

        [OperationContract]
        List<WalletItem> GetUserCurrencies(int userId);

        // LAB 13: YENİ - Geçmiş Kurları Getirme Metodu
        [OperationContract]
        List<HistoricalRate> GetHistoricalRates(string currencyCode, int daysCount);
    }

    [DataContract]
    public class ExchangeRateData
    {
        [DataMember]
        public string Currency { get; set; }
        [DataMember]
        public double BuyRate { get; set; }
        [DataMember]
        public double SellRate { get; set; }
    }

    [DataContract]
    public class WalletItem
    {
        [DataMember]
        public string Currency { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
    }

    // LAB 13: YENİ - Geçmiş Kur Veri Modeli
    [DataContract]
    public class HistoricalRate
    {
        [DataMember]
        public string Date { get; set; }
        [DataMember]
        public double BuyRate { get; set; }
        [DataMember]
        public double SellRate { get; set; }
    }
}