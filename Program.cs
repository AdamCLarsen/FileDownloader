using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FileDownloader
{
	class Program
	{
		static void Main(string[] args)
		{
			var rawInput = File.ReadAllLines(args[0]);
			var outFolder = Path.GetFullPath(args[1]);
			ServicePointManager.ServerCertificateValidationCallback += delegate {return true; };
			if (!Directory.Exists(outFolder))
			{
				Directory.CreateDirectory(outFolder);
			}

			Console.WriteLine("Writing to " + outFolder);
			using (var client = new HttpClient())
			{
				
				foreach (var s in rawInput)
				{
					try
					{
						Go(s, outFolder, client).Wait();
					}
					catch (Exception ex)
					{
						Console.WriteLine("Failed to read file from {0}, {1}", s, ex.GetBaseException().Message);
					}
				}
			}

			Console.WriteLine("Done...");
			Console.ReadKey(true);
		}

		static public async Task Go(string s, string outFolder, HttpClient client)
		{
			var uri = new Uri(s);
			var fileName = Path.GetFileName(uri.AbsolutePath);
			var outputFile = Path.Combine(outFolder, fileName);
			var tempFile = Path.Combine(outFolder, fileName) + ".tmp";

			if (File.Exists(outputFile))
			{
				Console.WriteLine("Skipping {0}", s);

				return;
			}

			using (var request = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
			{
				request.EnsureSuccessStatusCode();
				using (var fileStream = File.Create(tempFile))
				using (var httpStream = await request.Content.ReadAsStreamAsync())
				{
					await httpStream.CopyToAsync(fileStream);
					fileStream.Flush(true);
				}

				File.Move(tempFile, outputFile);
			}

			Console.WriteLine("Saved {0}", s);
		}
	}
}
