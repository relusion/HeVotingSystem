using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.Research.SEAL;
using Newtonsoft.Json;

namespace ExternalVoteCounter
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            Console.WriteLine("Connecting to the vote system...");
            var responseMessage = await client.GetAsync("https://localhost:44319/api/vote");
            Console.WriteLine("Encrypted votes received...");
            var stringResponse = await responseMessage.Content.ReadAsStringAsync();
            List<EncryptedVote> encryptedVotes = JsonConvert.DeserializeObject<List<EncryptedVote>>(stringResponse);

            SealService sealService = new SealService();
            List<List<ulong>> valuesList = new List<List<ulong>>();
            var items = encryptedVotes.Select(encryptedVote => SealUtils.BuildCiphertextFromBytes(encryptedVote.Data, sealService.SealContext)).ToArray();

            Console.WriteLine("Create Seal evaluator... no private key needed...");
            using var evaluator = sealService.CreateEvaluator();
            Ciphertext results = new Ciphertext();
            Console.WriteLine("Executing add function on all encrypted votes...");
            evaluator.AddMany(items, results);
            Console.WriteLine("Finished...");
            var resultsArray = SealUtils.CiphertextToArray(results);
            Console.WriteLine("Encrypted results  serialized");
            var jsonPayload = JsonConvert.SerializeObject(new
            {
                Content = resultsArray,
            });

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            await client.PostAsync("https://localhost:44319/api/vote/results", new StringContent(jsonPayload,Encoding.UTF8,"application/json"));
            Console.WriteLine("Encrypted results uploaded to the voting system");
        }
    }
}
