using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ToyShopParser.Extensions;
using ToyShopParser.Parser;

namespace ToyShopParser
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var toysSaintP = await ToyParser.ParseSectionAsync("https://www.toy.ru/catalog/boy_transport/", City.SPB, 3000, 2000);
            var toysRostov = await ToyParser.ParseSectionAsync("https://www.toy.ru/catalog/boy_transport/", City.RostovOnDon, 3000, 2000);
            sw.Stop();

            using (var stream = new StreamWriter("Toys.csv", false, Encoding.UTF8))
            {
                stream.WriteLine("Region;Breadcrumbs;Name;Price;OldPrice;IsAvailable;Link;PicLinks");

                foreach (var toy in toysSaintP)
                {
                    stream.WriteLine(toy.ToCSV());
                }

                foreach (var toy in toysRostov)
                {
                    stream.WriteLine(toy.ToCSV());
                }
            }

            Console.WriteLine($"{toysSaintP.Count + toysRostov.Count} toys parsed. Elapsed time: {sw.Elapsed}");
            Console.ReadLine();
        }

    }
}
