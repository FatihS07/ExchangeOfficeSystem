using System;
using System.Net;
using Newtonsoft.Json;

namespace MyWcfService
{
    public class Service1 : IService1
    {
        // Lab 1'den kalan metot (Dursun)
        public string MerhabaDe(string isim)
        {
            return "Hello " + isim + ", WCF service is working successfully!";
        }

        // --- LABS 2–4 GÖREVİ: NBP API BAĞLANTISI ---
        public double GetExchangeRate(string currencyCode)
        {
            try
            {
                // NBP API URL'si (Para kodunu URL içine yerleştiriyoruz)
                string url = $"http://api.nbp.pl/api/exchangerates/rates/a/{currencyCode}/?format=json";

                using (WebClient client = new WebClient())
                {
                    // İnternetten JSON formatındaki veriyi çekiyoruz
                    string json = client.DownloadString(url);

                    // Newtonsoft.Json kullanarak veriyi parçalıyoruz
                    dynamic data = JsonConvert.DeserializeObject(json);

                    // JSON içindeki "mid" (orta kur) değerini alıyoruz
                    double rate = (double)data.rates[0].mid;

                    return rate;
                }
            }
            catch (Exception)
            {
                // Hatalı kod girilirse veya internet yoksa 0 döndürür
                return 0;
            }
        }
    }
}