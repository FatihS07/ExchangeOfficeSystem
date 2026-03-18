using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Servis bağlantısını kuruyoruz
            ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();

            try
            {
                Console.WriteLine("=== NBP CURRENCY SERVICE ===");
                Console.Write("Enter currency code (USD, EUR, GBP, etc.): ");
                string code = Console.ReadLine().ToUpper();

                // Yeni metodu çağırıyoruz
                double rate = client.GetExchangeRate(code);

                if (rate > 0)
                {
                    Console.WriteLine("\n[SUCCESS]");
                    Console.WriteLine("Current rate for " + code + " is: " + rate + " PLN");
                }
                else
                {
                    Console.WriteLine("\n[ERROR]");
                    Console.WriteLine("Could not retrieve rate for: " + code);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nAn error occurred: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("\nPress ENTER to exit...");
                Console.ReadLine();
                client.Close();
            }
        }
    }
}sd