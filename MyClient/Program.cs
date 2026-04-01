using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel; // WCF hataları için gerekli

namespace MyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Servis istemcisini oluşturuyoruz
            ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();

            try
            {
                Console.WriteLine("======================================");
                Console.WriteLine("    NBP CURRENCY EXCHANGE SERVICE     ");
                Console.WriteLine("======================================");

                Console.Write("\nEnter currency code (e.g., USD, EUR, GBP): ");
                string input = Console.ReadLine();

                // Boş giriş kontrolü
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("[ERROR] Currency code cannot be empty!");
                }
                else
                {
                    string code = input.Trim().ToUpper();
                    Console.WriteLine($"\nFetching rate for {code}...");

                    // Servis metodunu çağırıyoruz
                    double rate = client.GetExchangeRate(code);

                    if (rate > 0)
                    {
                        Console.WriteLine("\n[SUCCESS]");
                        Console.WriteLine("--------------------------------------");
                        Console.WriteLine($"Currency: {code}");
                        Console.WriteLine($"Rate:     {rate} PLN");
                        Console.WriteLine("--------------------------------------");
                    }
                    else
                    {
                        Console.WriteLine($"\n[ERROR] Could not retrieve rate for: {code}");
                        Console.WriteLine("Please check if the currency code is valid.");
                    }
                }
            }
            catch (EndpointNotFoundException)
            {
                // Sunucu kapalıysa bu hata düşer
                Console.WriteLine("\n[CRITICAL ERROR] Could not connect to the service. Is the server running?");
            }
            catch (FaultException ex)
            {
                // Servis tarafında bir hata oluşursa (SOAP Fault)
                Console.WriteLine("\n[SERVICE ERROR] The service returned an error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Diğer beklenmedik hatalar
                Console.WriteLine("\n[UNEXPECTED ERROR] " + ex.Message);
            }
            finally
            {
                // WCF istemcisini güvenli kapatma protokolü
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    client.Close();
                }

                Console.WriteLine("\nPress ENTER to exit...");
                Console.ReadLine();
            }
        }
    }
}