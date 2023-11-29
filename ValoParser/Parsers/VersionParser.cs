using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class VersionParser
    {
        public VersionParser() { }

        Dictionary<string, string> json1 = new Dictionary<string, string>
        {
            { "Jan", "01" },
            { "Feb", "02" },
            { "Mar", "03" },
            { "Apr", "04" },
            { "May", "05" },
            { "Jun", "06" },
            { "Jul", "07" },
            { "Aug", "08" },
            { "Sep", "09" },
            { "Oct", "10" },
            { "Nov", "11" },
            { "Dec", "12" }
        };

        private static JsonObject jsonObject = new JsonObject();

        public void GetVersionContent(string gamePath = "C:\\Riot Games\\VALORANT\\live", string apiPath = "C:\\Riot Games\\Riot Client")
        {

            string exePath = gamePath + "\\ShooterGame\\Binaries\\Win64\\VALORANT-Win64-Shipping.exe";

            // VALORANT-Win64-Shipping.exe

            byte[] fileBytesExe = File.ReadAllBytes(exePath);

            byte[] utf16lePatternExe = Encoding.Unicode.GetBytes("++Ares-Core+release-");

            int indexExe = FindPattern(fileBytesExe, utf16lePatternExe);

            if (indexExe != -1 && indexExe + utf16lePatternExe.Length + 100 <= fileBytesExe.Length)
            {
                string text = GetNextBytesAsString(fileBytesExe, indexExe, utf16lePatternExe.Length, 100);

                if (!string.IsNullOrEmpty(text))
                {
                    string[] data = text.Split("\u0000\u0000\u0000");

                    // Branch
                    jsonObject.Add("branch", string.Format("release-{0}", data[0].TrimEnd('\x00')));

                    // Build Date
                    string[] date = data[1].Replace("\u0000", " ").Split(" ");
                    jsonObject.Add("buildDate", string.Format("{0}-{1}-{2}T00:00:00.000Z", date[2].TrimEnd('\x00'), json1[date[0].TrimEnd('\x00')], date[1].TrimEnd('\x00')));

                    // Build Version
                    jsonObject.Add("buildVersion", string.Format("{0}", date[3].TrimEnd('\x00')));

                    // Build Version
                    jsonObject.Add("version", string.Format("{0}", data[2].TrimEnd('\x00')));

                    // Riot Client Version
                    jsonObject.Add("riotClientVersion", string.Format("release-{0}-shipping-{1}-{2}", data[0].TrimEnd('\x00'), date[3].TrimEnd('\x00'), data[2].TrimEnd('\x00').Split(".").Last()));
                }
            }
            else
            {
                Console.WriteLine("VersionUtil: Pattern not found or insufficient bytes available after the pattern. (VALORANT-Win64-Shipping.exe)");
                return;
            }

            // RiotClientServices.exe

            string riotClientBuild = "";

            byte[] fileBytesCli = File.ReadAllBytes(apiPath + "\\RiotClientServices.exe");

            byte[] utf16lePatternCli = Encoding.Unicode.GetBytes("FileVersion");

            int indexCli = FindPattern(fileBytesCli, utf16lePatternCli);

            if (indexCli != -1 && indexCli + utf16lePatternCli.Length + 100 <= fileBytesCli.Length)
            {
                string text = GetNextBytesAsString(fileBytesCli, indexCli, utf16lePatternCli.Length, 100);

                if (!string.IsNullOrEmpty(text))
                {
                    string[] data = text.Split("\u0000\u0000\u0000");
                    riotClientBuild += data[0].Replace("\u0000", " ").TrimStart(' ').Split(" ")[0];
                }
            }
            else
            {
                Console.WriteLine("VersionUtil: Pattern not found or insufficient bytes available after the pattern. (RiotClientServices.exe)");
                return;
            }

            // RiotGamesApi.dll

            byte[] fileBytesApi = File.ReadAllBytes(apiPath + "\\RiotGamesApi.dll");

            byte[] utf16lePatternApi = Encoding.Unicode.GetBytes("FileVersion");

            int indexApi = FindPattern(fileBytesApi, utf16lePatternApi);

            if (indexApi != -1 && indexApi + utf16lePatternApi.Length + 100 <= fileBytesApi.Length)
            {
                string text = GetNextBytesAsString(fileBytesApi, indexApi, utf16lePatternApi.Length, 100);

                if (!string.IsNullOrEmpty(text))
                {
                    string[] data = text.Split("\u0000\u0000\u0000");

                    riotClientBuild += "." + data[0].Replace("\u0000", " ").TrimStart(' ').Split(" ")[0].Split(".").Last();

                    jsonObject.Add("riotClientBuild", riotClientBuild);
                }
            }
            else
            {
                Console.WriteLine("VersionUtil: Pattern not found or insufficient bytes available after the pattern. (RiotGamesApi.dll)");
                return;
            }

            UassetUtil.exportJson(jsonObject, "data/version.json");
            return;
        }

        private int FindPattern(byte[] source, byte[] pattern)
        {

            for (int i = 0; i <= source.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    return i;
                }
            }

            return -1;
        }

        private string GetNextBytesAsString(byte[] source, int index, int offset, int length)
        {
            if (index + offset + length <= source.Length)
            {
                byte[] nextBytes = new byte[length];
                Array.Copy(source, index + offset, nextBytes, 0, length);

                string text = Encoding.Unicode.GetString(nextBytes);
                return text;
            }

            return null;
        }
    }
}
