using Org.BouncyCastle.X509;
using System;

namespace Web.Models
{
    public class SignatureStamp
    {
        public static readonly float DEFAULT_WIDTH = 80;
        public static readonly float DEFAULT_HEIGHT = 60;

        public SignatureStamp()
        {
            this.UniqueId = Guid.NewGuid().ToString("N");
        }

        public string UniqueId { get; private set; }

        public string Contact { get; set; }

        public string Reason { get; set; }

        public string Location { get; set; }

        public string PathToImage { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public X509Certificate certificate { get; set; }
    }
}