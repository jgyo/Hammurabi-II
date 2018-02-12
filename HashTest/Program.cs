using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HashTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var Secret = "SixthKingOfTheFirstBabylonianDynasty";
            var input = "gil1517329137show76.231.147.77yoder$45Media";
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(Secret)))
            {
                var bytes = Encoding.ASCII.GetBytes(input);
                var stream = new MemoryStream(bytes);
                var data = hmac.ComputeHash(stream);
                var hash = BitConverter.ToString(data);

                Console.WriteLine($"Secret: {Secret}");
                Console.WriteLine($"Input: {input}");
                Console.WriteLine($"Hash: {hash}");


            }
        }
    }
}
