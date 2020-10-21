using System;
using System.IO;
using System.Text;
using Microsoft.Research.SEAL;

namespace Common
{
    public class SealUtils
    {

        public static byte[] CiphertextToArray(Ciphertext ciphertext)
        {
            using var ms = new MemoryStream();
            ciphertext.Save(ms);
            return ms.ToArray();
        }

        public static string CiphertextToBase64String(Ciphertext ciphertext)
        {
            using (var ms = new MemoryStream())
            {
                ciphertext.Save(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static string DoubleToBase64String(double value)
        {
            using (var ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(value.ToString())))
            {
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static Ciphertext BuildCiphertextFromBytes(byte[] payload, SEALContext context)
        {
            using (var ms = new MemoryStream(payload))
            {
                var ciphertext = new Ciphertext();
                ciphertext.Load(context, ms);

                return ciphertext;
            }
        }


        public static Ciphertext BuildCiphertextFromBase64String(string base64, SEALContext context)
        {
            var payload = Convert.FromBase64String(base64);

            using (var ms = new MemoryStream(payload))
            {
                var ciphertext = new Ciphertext();
                ciphertext.Load(context, ms);

                return ciphertext;
            }
        }

        public static Ciphertext CreateCiphertextFromInt(double value, Encryptor encryptor)
        {
            var plaintext = new Plaintext($"{value}");
            var ciphertext = new Ciphertext();
            encryptor.Encrypt(plaintext, ciphertext);
            return ciphertext;
        }

        public static PublicKey BuildPublicKeyFromBase64String(string base64, SEALContext context)
        {
            var payload = Convert.FromBase64String(base64);

            using (var ms = new MemoryStream(payload))
            {
                var publicKey = new PublicKey();
                publicKey.Load(context, ms);

                return publicKey;
            }
        }

        public static SecretKey BuildSecretKeyFromBase64String(string base64, SEALContext context)
        {
            var payload = Convert.FromBase64String(base64);

            using (var ms = new MemoryStream(payload))
            {
                var secretKey = new SecretKey();
                secretKey.Load(context, ms);

                return secretKey;
            }
        }

        public static string SecretKeyToBase64String(SecretKey secretKey)
        {
            using (var ms = new MemoryStream())
            {
                secretKey.Save(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static string PublicKeyToBase64String(PublicKey publicKey)
        {
            using (var ms = new MemoryStream())
            {
                publicKey.Save(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static SEALContext GetContext()
        {
            var encryptionParameters = new EncryptionParameters(SchemeType.BFV)
            {
                PolyModulusDegree = 32768,
                CoeffModulus = CoeffModulus.BFVDefault(32768)
            };
            //encryptionParameters.PlainModulus = new Modulus(1024);

            /*
           To enable batching, we need to set the plain_modulus to be a prime number
           congruent to 1 modulo 2*PolyModulusDegree. Microsoft SEAL provides a helper
           method for finding such a prime. In this example we create a 20-bit prime
           that supports batching.
           */
            encryptionParameters.PlainModulus = PlainModulus.Batching(32768, 20);


            SEALContext new_context = new SEALContext(encryptionParameters);
            return new_context;
        }
    }
}
