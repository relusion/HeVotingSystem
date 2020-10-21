using Microsoft.Research.SEAL;

namespace Common
{
    public class SealService
    {
        public SEALContext SealContext { get; }

        public SealService()
        {
            SealContext = SealUtils.GetContext();
            using var keygen = new KeyGenerator(SealContext);
            using var publicKey = keygen.PublicKey;
            using var secretKey = keygen.SecretKey;

            PublicKeyBase64 = SealUtils.PublicKeyToBase64String(publicKey);
            PrivateKeyBase64 = SealUtils.SecretKeyToBase64String(secretKey);
        }

        public Encryptor CreateEncryptor()
        {
            var encryptor = new Encryptor(SealContext, SealUtils.BuildPublicKeyFromBase64String(PublicKeyBase64,SealContext));
            return encryptor;
        }

        public Decryptor CreateDecryptor()
        {
            Decryptor decryptor = new Decryptor(SealContext, SealUtils.BuildSecretKeyFromBase64String(PrivateKeyBase64,SealContext));
            return decryptor;
        }

        public Evaluator CreateEvaluator()
        {
            Evaluator evaluator = new Evaluator(SealContext);
            return evaluator;
        }

        public BatchEncoder CreateBatchEncoder()
        {
            var batchEncoder = new BatchEncoder(SealContext);
            return batchEncoder;
        }

        public string PublicKeyBase64 { get; set; }
        public string PrivateKeyBase64 { get; set; }
    }
}
