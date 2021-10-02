#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TrackMapGenerator.Parameters;

#endregion

namespace TrackMapGenerator.Map
{
    public static class MapHandler
    {
        /// <summary>
        /// The URL to the default Mercator map projection.
        /// </summary>
        public static readonly Dictionary<string, string> MapUrl = new Dictionary<string, string>
        {
            { "mercator", "https://upload.wikimedia.org/wikipedia/commons/8/8f/Whole_world_-_land_and_oceans_12000.jpg" }
        };

        public static readonly string UserAgent =
            $"{Program.AssemblyName.Name}/{Program.AssemblyName.Version} {typeof(HttpClient).Assembly.GetName().Name}/{typeof(HttpClient).Assembly.GetName().Version}";
        
        public static readonly string MapLocation = Path.Join(
            Program.DataFolder,
            "bg_" + Md5(Options.Generator)
        );

        private static string Md5(string uri)
        {
            using MD5 md5 = MD5.Create();
            md5.Initialize();
            byte[] outBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(uri));
                
            StringBuilder builder = new StringBuilder();
            foreach (byte t in outBytes)
                builder.Append(t.ToString("X2"));
            return builder.ToString();
        }

        private static string ProgressBar(int length, double progress)
        {
            int filledLength = (int) Math.Round((length) * progress);
            return "[" 
                + new string('\u2588', filledLength)
                + new string(' ', length - filledLength)
                + "]";
        }

        private static void UpdateProgress(int length, double progress, string extra = "")
        {
            if (!Environment.UserInteractive) return;
            StringBuilder output = new StringBuilder(ProgressBar(length, progress));
            output.Append($" {Math.Round(progress * 100, 2)}%");
            if (extra.Length > 0)
                output.Append($" {extra}");

            Console.Write("\r" + new string(' ', Console.BufferWidth - 1));
            Console.Write($"\r    {output}");
        }
        
        public static byte[] GetMap()
        {
            if (!File.Exists(MapLocation))
                Download().Wait();
            return File.ReadAllBytes(MapLocation);
        }
        
        public static async Task Download()
        {
            Console.WriteLine($"Downloading map image: {MapUrl[Options.Generator]}");
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            HttpResponseMessage response = await client.GetAsync(MapUrl[Options.Generator]);
            response.EnsureSuccessStatusCode();
            
            if (File.Exists(MapLocation))
                File.Delete(MapLocation);
            FileStream fileStream = File.Create(MapLocation);
            Stream dataStream = await response.Content.ReadAsStreamAsync();
            
            byte[] buffer = new byte[4096];
            long expectedLength = response.Content.Headers.ContentLength ?? -1;
            int readBytes = 0;
            int bufferPointer = 0;
            int lastUpdate = Environment.TickCount;
            UpdateProgress(
                20, 
                (double) readBytes / expectedLength,
                expectedLength != -1 ? $"(0 / {expectedLength / 100} kB)" : "(0 kB)"
            );
            while (dataStream.CanRead)
            {
                int read = dataStream.ReadByte();
                if (read == -1) break;

                buffer[bufferPointer++] = (byte) read;
                
                if (Environment.TickCount >= lastUpdate + 200)
                {
                    UpdateProgress(
                        20, 
                        (double) readBytes / expectedLength,
                        expectedLength != -1 
                            ?
                            $"({readBytes / 100} / {expectedLength / 100} kB)"
                            : 
                            $"({readBytes / 100} kB)"
                    );
                    lastUpdate = Environment.TickCount;
                }
                
                if (bufferPointer != buffer.Length) continue;
                
                fileStream.Write(buffer, 0, buffer.Length);
                readBytes += buffer.Length;
                bufferPointer = 0;
            }

            // Write remaining in the buffer.
            if (bufferPointer > 0)
            {
                fileStream.Write(buffer, 0, bufferPointer);
                readBytes += bufferPointer;
                UpdateProgress(
                    20, 
                    1d,
                    expectedLength != -1 
                        ?
                        $"({readBytes / 100} / {expectedLength / 100} kB)"
                        : 
                        $"({readBytes / 100} kB)"
                );
            }
            Console.Write("\r\n");
            
            handler.Dispose();
            fileStream.Flush();
            fileStream.Close();
        }

    }
}