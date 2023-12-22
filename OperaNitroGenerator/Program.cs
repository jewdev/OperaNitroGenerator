using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OperaNitroGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Proxyless Opera Nitro Generator by jewdev";
            Console.WriteLine("How many codes do you want to generate?");

            if (!File.Exists("codes.txt"))
                File.Create("codes.txt");

            int amount = Convert.ToInt32(Console.ReadLine());

            string url = "https://api.discord.gx.games/v1/direct-fulfillment";

            string randomString = GenerateRandomString(64);

            var data = new { partnerUserId = randomString };
            var jsonData = JsonConvert.SerializeObject(data);
            
            try
            {
                Console.Clear();

                Task.Run(async () =>
                {
                    for (int i = 0; i < amount; i++)
                    {
                        await Send(url, jsonData);
                    }
                }).GetAwaiter().GetResult();
            }
            catch
            {
                Console.WriteLine("Error!");
            }
        }

        static string GenerateRandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static async Task Send(string url, string jsonData)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, url);
            message.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            message.Headers.Add("authority", "api.discord.gx.games");
            message.Headers.Add("accept", "*/*");
            message.Headers.Add("accept-language", "en-US,en;q=0.9");
            message.Headers.Add("origin", "https://www.opera.com");
            message.Headers.Add("referer", "https://www.opera.com/");
            message.Headers.Add("sec-ch-ua", "\"Opera GX\";v=\"105\", \"Chromium\";v=\"119\", \"Not?A_Brand\";v=\"24\"");
            message.Headers.Add("sec-ch-ua-mobile", "?0");
            message.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            message.Headers.Add("sec-fetch-dest", "empty");
            message.Headers.Add("sec-fetch-mode", "cors");
            message.Headers.Add("sec-fetch-site", "cross-site");
            message.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 OPR/105.0.0.0");

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.SendAsync(message);

                if (response.StatusCode == (HttpStatusCode)200)
                {
                    string jsonRes = await response.Content.ReadAsStringAsync();
                    var jsonObj = JObject.Parse(jsonRes);
                    string token = (string)jsonObj["token"];

                    string result = "https://discord.com/billing/partner-promotions/1180231712274387115/" + token;

                    File.AppendAllText("codes.txt", result + Environment.NewLine);
                    Console.WriteLine(result + Environment.NewLine);
                }
                else if (response.StatusCode == (HttpStatusCode)429)
                {
                    Console.WriteLine("Rate limited! Waiting 1 minute.");
                    Thread.Sleep(60000);
                }
                else if (response.StatusCode == (HttpStatusCode)504)
                {
                    Console.WriteLine("Server timed out! Waiting 5 seconds.");
                    Thread.Sleep(5000);
                }
                else
                {
                    Console.WriteLine($"Request failed!\nStatus code: {response.StatusCode}\n Error: {response.Content}");
                }
            }
        }
    }
}
