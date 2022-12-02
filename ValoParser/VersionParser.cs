using System;
using System.IO;

namespace ValoParser
{
    public static class VersionParser
    {
        public static void Parse()
        {
            byte[] buffer = File.ReadAllBytes(@"C:\Riot Games\VALORANT\live\ShooterGame\Binaries\Win64\VALORANT-Win64-Shipping.exe");
            string base64Encoded = Convert.ToBase64String(buffer);

            buffer = Convert.FromBase64String(base64Encoded);


            Console.WriteLine(buffer);
        }
    }
}