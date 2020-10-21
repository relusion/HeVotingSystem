using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Research.SEAL;
using Newtonsoft.Json;
using VoteMachineWeb.Models;

namespace VoteMachineWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoteController : ControllerBase
    {
        private readonly SealService _sealService;
        private static readonly ConcurrentDictionary<string, EncryptedVote> _votes = new ConcurrentDictionary<string, EncryptedVote>();
        private Ciphertext _computationResults;
        private static readonly ulong[] CalculatedVotes = new ulong[2];

        public VoteController(SealService sealService)
        {
            _sealService = sealService;
        }

        [HttpGet]
        public List<EncryptedVote> Get()
        {
            return _votes.Values.ToList();
        }

        [HttpGet("/api/vote/{voteId}")]
        public EncryptedVote Get(string voteId)
        {
            return _votes[voteId];
        }

        [HttpGet("/api/vote/status")]
        public ContentResult GetStatus()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var encryptedVote in _votes)
            {
                string userVoteHtml = $"<p><b>{encryptedVote.Value.Voter}</b> voted. Registered vote id: <a href=\"{encryptedVote.Key}\">{encryptedVote.Key}</a></p>";
                stringBuilder.Append(userVoteHtml);
            }

            var html = $"<html><body>{stringBuilder}</body></html>";

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html,
            };
        }

        [HttpGet("/api/vote/results")]
        public ContentResult GetResults()        
        {
            var html =  $"<html><body><p>Trump:{CalculatedVotes[1]}</p><p>Biden:{CalculatedVotes[0]}</p></body></html>";

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html,
            };
        }

        [HttpPost("/api/vote/results")]
        public IActionResult PostResults([FromBody] VoteCalcModel content)
        {
            _computationResults = SealUtils.BuildCiphertextFromBytes(content.Content, _sealService.SealContext);
            using var decryptor = _sealService.CreateDecryptor();
            Plaintext plaintext = new Plaintext();
            decryptor.Decrypt(_computationResults, plaintext);
            using var batchEncoder = _sealService.CreateBatchEncoder();
            List<ulong> decodedValues = new List<ulong>();
            batchEncoder.Decode(plaintext, decodedValues);
            decodedValues.RemoveRange(2,decodedValues.Count-2);
            CalculatedVotes[0] = decodedValues[0];
            CalculatedVotes[1] = decodedValues[1];
            return Ok();
        }

        [HttpPost]
        public IActionResult Post([FromForm] VoteModel content)
        {
            using var batchEncoder = _sealService.CreateBatchEncoder();
            using var encryptor = _sealService.CreateEncryptor();

            //create a vector with data
            var voteMatrix = new ulong[2];
            voteMatrix[0] = content.IsBiden;
            voteMatrix[1] = content.IsTrump;

            using var plainMatrix = new Plaintext();
            //encode the vector
            batchEncoder.Encode(voteMatrix, plainMatrix);
            var voteMatrixCipher = new Ciphertext();
            //encrypt the vector with HE
            encryptor.Encrypt(plainMatrix, voteMatrixCipher);
            //serialize encrypted data
            var encryptedMatrixBytes = SealUtils.CiphertextToArray(voteMatrixCipher);

            var encryptedVote = new EncryptedVote
            {
                Data = encryptedMatrixBytes,
                DateOfVote = DateTimeOffset.UtcNow,
                Voter = content.DisplayName,
            };

            var dataToSign = JsonConvert.SerializeObject(encryptedVote, Formatting.None);
            var signature = RCASignString(dataToSign);

            //generate a vote id and store the encrypted vote. In the real system that would be an azure storage account or file system.
            var voteId = Guid.NewGuid().ToString("N");
            _votes.TryAdd(voteId, encryptedVote);

            // return the encrypted vote to the user.
            return Ok(new
            {
                EncryptedVote = encryptedVote,
                Signature = signature,
                voteId,
            });
        }

        private string RCASignString(string dataToSign)
        {
            var originalData = Encoding.UTF8.GetBytes(dataToSign);
            byte[] signedData;

            // Create a new instance of the RSACryptoServiceProvider class
            // and automatically create a new key-pair.
            var RSAalg = new RSACryptoServiceProvider();

            // Export the key information to an RSAParameters object.
            // You must pass true to export the private key for signing.
            // However, you do not need to export the private key
            // for verification.
            var Key = RSAalg.ExportParameters(true);

            // Hash and sign the data.
            signedData = HashAndSignBytes(originalData, Key);

            return Convert.ToBase64String(signedData);
        }

        public static byte[] HashAndSignBytes(byte[] DataToSign, RSAParameters Key)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the
                // key from RSAParameters.
                var RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportParameters(Key);

                // Hash and sign the data. Pass a new instance of SHA256
                // to specify the hashing algorithm.
                return RSAalg.SignData(DataToSign, SHA256.Create());
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }
    }
}