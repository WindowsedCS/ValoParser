using CUE4Parse.Utils;
using CUE4Parse_Conversion.Sounds;
using System.Diagnostics;
using System.IO;
namespace ValoParser.Parsers
{
    public class AudioParser
    {
        public static void Parse(string path, string outputPath)
        {
            var provider = Program.provider;
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath).SubstringBeforeLast("/"));
            var exports = provider.LoadObjectExports(path.Split(".")[0]);
            foreach (var export in exports)
            {
                SoundDecoder.Decode(export, true, out var audioFormat, out var data);
                if (data != null)
                {
                    using var stream = new FileStream(outputPath + "." + audioFormat.ToLower(), FileMode.Create, FileAccess.Write);
                    using var writer = new BinaryWriter(stream);
                    writer.Write(data);
                    writer.Flush();
                    writer.Close();
                    WemToWav(outputPath + "." + audioFormat.ToLower());
                    return;
                }
            }
        }

        static void WemToWav(string outputPath)
        {
            var vgmFilePath = Path.Combine(@"./tools", "vgmstream", "test.exe");
            if (!File.Exists(vgmFilePath)) return;
            string wavFilePath = Path.ChangeExtension(outputPath, ".wav");
            var vgmProcess = Process.Start(new ProcessStartInfo
            {
                FileName = vgmFilePath,
                Arguments = $"-o \"{wavFilePath}\" \"{outputPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            vgmProcess?.WaitForExit();

            File.Delete(outputPath);
            return;
        }
    }
}