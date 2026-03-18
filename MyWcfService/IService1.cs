using System.ServiceModel;

namespace MyWcfService
{
    [ServiceContract]
    public interface IService1
    {
        // Lab 1'den kalan metot (Dursun, hatıra kalsın)
        [OperationContract]
        string MerhabaDe(string isim);

        // --- LABS 2–4 GÖREVİ ---
        // Bu metot dışarıdan "USD", "EUR" gibi kodlar alacak
        // ve NBP API'den gelen kur değerini (sayı olarak) döndürecek.
        [OperationContract]
        double GetExchangeRate(string currencyCode);
    }
}