using System.ServiceModel;
using System.Runtime.Serialization;

namespace MyWcfService
{
    [ServiceContract]
    public interface IService1
    {
        // Döviz Bürosu için Alış ve Satış kurlarını getiren metot (Lab 7)
        [OperationContract]
        ExchangeRateData GetBuySellRates(string currencyCode);

        // YENİ: Kullanıcı Kayıt Metodu (Lab 11 - Veritabanı Bağlantısı için)
        [OperationContract]
        bool RegisterUser(string username, string password);
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
}