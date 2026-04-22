using System;
using System.Net;
using System.Text;
using Newtonsoft.Json; // Eğer hata verirse CTRL + . ile using'i eklemeyi unutma

namespace MyWcfService
{
    public class Service1 : IService1
    {
        public ExchangeRateData GetBuySellRates(string currencyCode)
        {
            try
            {
                // NBP API 'C' Tablosu: Alış (Bid) ve Satış (Ask) fiyatlarını verir
                string url = $"http://api.nbp.pl/api/exchangerates/rates/c/{currencyCode.ToLower()}/?format=json";

                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string json = client.DownloadString(url);

                    // JSON verisini dinamik olarak çözümlüyoruz
                    dynamic data = JsonConvert.DeserializeObject(json);

                    // API'den gelen bid (alış) ve ask (satış) değerlerini alıyoruz
                    double buy = data.rates[0].bid;
                    double sell = data.rates[0].ask;

                    // Veriyi client'a gönderilmek üzere paketliyoruz
                    return new ExchangeRateData
                    {
                        Currency = currencyCode.ToUpper(),
                        BuyRate = buy,
                        SellRate = sell
                    };
                }
            }
            catch (Exception)
            {
                // Hata durumunda 0 döndürüyoruz
                return new ExchangeRateData { BuyRate = 0, SellRate = 0 };
            }
        }
    }
}