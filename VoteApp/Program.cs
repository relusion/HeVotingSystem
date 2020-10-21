using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;

namespace VoteApp
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();

        private static async Task Main(string[] args)
        {
            var status = await client.GetAsync("https://localhost:44319/api/vote/status");
            status.EnsureSuccessStatusCode();

            var faker = new Faker();

            Parallel.For(0, 50, i =>
            {
                var biden = Randomizer.Seed.Next(2) == 0;
                var trump = !biden;

                var postResponseMessage = client.PostAsync("https://localhost:44319/api/vote",
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("IsBiden", Convert.ToInt16(biden).ToString()),
                        new KeyValuePair<string, string>("IsTrump", Convert.ToInt16(trump).ToString()),
                        new KeyValuePair<string, string>("DisplayName", faker.Name.FullName())
                    })).Result;

                Console.WriteLine($"Vote submitted: {i}, Biden: {biden}, Trump: {trump}");
            });

            Console.WriteLine("Thanks");
        }
    }
}