using Org.BouncyCastle.X509;
using System;
using System.Drawing;

namespace Web.Models
{
    public class SignatureStamp
    {
        public static SignatureStampFactory Factory
        {
            get
            {
                return new SignatureStampFactory();
            }
        }

        public static readonly float DEFAULT_WIDTH = 100;
        public static readonly float DEFAULT_HEIGHT = 80;
        public static readonly float DEFAULT_PADDING = 10;

        public SignatureStamp()
        {
            this.UniqueId = Guid.NewGuid().ToString("N");
        }

        public string UniqueId { get; private set; }
        public string Contact { get; private set; } = string.Empty;
        public string Reason { get; private set; } = string.Empty;
        public string Location { get; private set; } = string.Empty;
        public Image Graphic { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public X509Certificate Certificate { get; private set; }

        public class SignatureStampFactory
        {
            private SignatureStamp _Instance;

            public SignatureStampFactory()
            {
                this._Instance = new SignatureStamp();
            }

            /*
             *
             * Fluent API
             * @author ❤️ BinhLD ❤️
             *
             */

            public SignatureStampFactory SetContact(string contact)
            {
                this._Instance.Contact = contact;
                return this;
            }

            public SignatureStampFactory SetReason(string reason)
            {
                this._Instance.Reason = reason;
                return this;
            }

            public SignatureStampFactory SetLocation(string location)
            {
                this._Instance.Location = location;
                return this;
            }

            public SignatureStampFactory SetGraphic(Image image)
            {
                this._Instance.Graphic = image;
                return this;
            }

            public SignatureStampFactory SetWidth(float width)
            {
                this._Instance.Width = width;
                return this;
            }

            public SignatureStampFactory SetHeight(float height)
            {
                this._Instance.Height = height;
                return this;
            }

            public SignatureStampFactory SetCertificate(X509Certificate certificate)
            {
                this._Instance.Certificate = certificate;
                return this;
            }

            public SignatureStamp Build()
            {
                return this._Instance;
            }
        }
    }
}