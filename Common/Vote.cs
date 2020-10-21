using System;

namespace Common
{
    public class EncryptedVote
    {
        public byte[] Data { get; set; }
        public string Voter { get; set; }
        public DateTimeOffset DateOfVote { get; set; }
    }

    public class SignedEncryptedVote
    {
        public EncryptedVote EncryptedVote { get; set; }
        public string Signature { get; set; }
        
    }
}
