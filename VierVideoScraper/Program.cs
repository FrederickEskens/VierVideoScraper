using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace VierVideoScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter URL of first segment:");
            var url = Console.ReadLine();
            Console.WriteLine("Enter filename (without extension)");
            var fileName = Console.ReadLine().Replace(" ","_");
    
            var currIndex = 0;
            var tempfolder = Path.Combine(Directory.GetCurrentDirectory(), "temp");

            using (var client = new WebClient())
            {
                try
                {
                    Directory.CreateDirectory(tempfolder);

                    while (true)
                    {
                        var segment = url.Substring(url.LastIndexOf("_", StringComparison.Ordinal) + 1);
                        var tempFilename = AddPrecedingZeros(segment);
                        client.DownloadFile(url, $"temp/{tempFilename}");
                        Console.WriteLine($"Downloaded part {currIndex + 1}.");
                        currIndex++;
                        url = url.Replace(segment, $"{currIndex}.ts");
                    }
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        CreateFile(fileName, tempfolder);
                    }
                    else
                    {
                        Console.WriteLine($"An error has occured: {e.Message}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error has occured: {e.Message}");
                }
            }
        }

        private static void CreateFile(string fileName, string tempfolder)
        {
            Console.WriteLine("Download complete.");
            Console.WriteLine("Merging files.");

            var cmd = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            cmd.Start();

            cmd.StandardInput.WriteLine($"copy /b temp\\*.ts {fileName}.mp4");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());

            RemoveTempFiles(tempfolder);

            Console.WriteLine("Scrape completed.");
            Console.ReadKey();
        }

        private static void RemoveTempFiles(string tempfolder)
        {
            Console.WriteLine("Removing temporary files.");
            Directory.Delete(tempfolder, true);
        }

        private static string AddPrecedingZeros(string input)
        {
            var a = input.Length;
            var result = "0";

            while (a < 7)
            {
                result += "0";
                a++;
            }

            return result + input;
        }
    }
}
