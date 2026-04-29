using System;
using System.ServiceModel;

namespace MyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();

            try
            {
                Console.WriteLine("======================================");
                Console.WriteLine("   NBP CURRENCY EXCHANGE CONSOLE      ");
                Console.WriteLine("======================================");
                Console.Write("\nEnter currency code (e.g., USD, EUR, GBP): ");
                string code = Console.ReadLine()?.Trim().ToUpper();

                if (string.IsNullOrWhiteSpace(code))
                {
                    Console.WriteLine("[ERROR] Currency code cannot be empty!");
                }
                else
                {
                    Console.WriteLine($"\nFetching Bid/Ask rates for {code}...");

                    // YENİ METODU ÇAĞIRIYORUZ
                    var rateData = client.GetBuySellRates(code);

                    if (rateData != null && rateData.BuyRate > 0)
                    {
                        Console.WriteLine("\n[SUCCESS]");
                        Console.WriteLine("--------------------------------------");
                        Console.WriteLine($"Currency : {rateData.Currency}");
                        Console.WriteLine($"Buy (Bid): {rateData.BuyRate} PLN");
                        Console.WriteLine($"Sell(Ask): {rateData.SellRate} PLN");
                        Console.WriteLine("--------------------------------------");
                    }
                    else
                    {
                        Console.WriteLine($"\n[ERROR] Could not retrieve rate for: {code}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[ERROR] " + ex.Message);
            }
            finally
            {
                if (client.State == CommunicationState.Faulted)
                    client.Abort();
                else
                    client.Close();
            }

            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }
    }
}