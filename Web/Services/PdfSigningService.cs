using iText.IO.Image;
using iText.IO.Source;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Signatures;
using Org.BouncyCastle.X509;
using System.Collections.Generic;
using System.IO;
using Web.Models;

namespace Web.Services
{
    public class PdfSigningService
    {
        public static readonly string HASH_ALGORITHM = "SHA1";
        public static readonly string CRYPT_ALGORITHM = "RSA";

        private static PdfSigningService _instance = null;

        private PdfSigningService()
        {
        }

        public static PdfSigningService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new PdfSigningService();
            }

            return _instance;
        }

        /// <summary>
        /// Create a Pdf with signature image embedded
        /// </summary>
        /// <param name="pathToSrcPdf"></param>
        /// <param name="pathToDstPdf"></param>
        /// <param name="pathToGraphic"></param>
        /// <param name="positions"></param>
        public void InsertSignatureGraphic(string pathToSrcPdf, string pathToDstPdf, SignatureStamp signatureStamp, IEnumerable<SignaturePosition> positions)
        {
            var pdfDoc = new PdfDocument(new PdfReader(pathToSrcPdf), new PdfWriter(pathToDstPdf));
            var doc = new Document(pdfDoc);

            foreach (var position in positions)
            {
                var imageData = ImageDataFactory.Create(signatureStamp.PathToImage);
                var image = new Image(imageData)
                                    .ScaleAbsolute(signatureStamp.Width, signatureStamp.Height)
                                    .SetFixedPosition(position.PageNumber, position.llx, position.lly);
                doc.Add(image);
            }

            doc.Close();
        }

        /// <summary>
        /// Create a Pdf with an empty signature
        /// (Fill all required entry for signature dictionary except /Content)
        /// </summary>
        /// <param name="pathToSrcPdf"></param>
        /// <param name="pathToDstPdf"></param>
        /// <param name="signatureStamp"></param>
        public void EmptySignature(string pathToSrcPdf, string pathToDstPdf, SignatureStamp signatureStamp)
        {
            var reader = new PdfReader(pathToSrcPdf);
            var os = new FileStream(pathToDstPdf, FileMode.Create);

            var signer = new PdfSigner(reader, os, new StampingProperties());
            var appearance = signer.GetSignatureAppearance();

            appearance
                .SetReason(signatureStamp.Reason)
                .SetContact(signatureStamp.Contact)
                .SetLocation(signatureStamp.Location)
                .SetCertificate(signatureStamp.certificate)
                .SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION);

            signer.SetFieldName(signatureStamp.UniqueId);

            var blankSigContainter = new ExternalBlankSignatureContainer(PdfName.Adobe_PPKLite, PdfName.Adbe_pkcs7_detached);
            signer.SignExternalContainer(blankSigContainter, 8192);

            reader.Close();
            os.Close();
        }

        /// <summary>
        /// </summary>
        /// <param name="pathToPdf"></param>
        /// <param name="sigDictName">signature dictionary name</param>
        /// <param name="certChain"></param>
        /// <returns>Digest (Authenticated Atrribute) of Pdf</returns>
        public byte[] HashFile(string pathToPdf, string sigDictName, X509Certificate[] certChain)
        {
            var reader = new PdfReader(pathToPdf);

            // turn pdf into bytes
            var sigUtil = new SignatureUtil(new PdfDocument(reader));
            var signatureDictionary = sigUtil.GetSignatureDictionary(sigDictName);
            long[] byteRange = signatureDictionary.GetAsArray(PdfName.ByteRange).ToLongArray(); // Pdf electronic signature BYTERANGE
            var fullRAS = reader.GetSafeFile().CreateSourceView();

            // take portion of bytes
            var partialRAS = new RandomAccessSourceFactory().CreateRanged(fullRAS, byteRange);
            var rasStream = new RASInputStream(partialRAS);

            // hashing
            var hash = DigestAlgorithms.Digest(rasStream, HASH_ALGORITHM);
            var pdfPKCS7 = new PdfPKCS7(null, certChain, HASH_ALGORITHM, false);
            var digest = pdfPKCS7.GetAuthenticatedAttributeBytes(hash, PdfSigner.CryptoStandard.CMS, null, null); // to-be-signed hash

            reader.Close();

            return digest;
        }

        /// <summary>
        /// Create a Pdf file whose /Content entry was filled in by signature
        /// </summary>
        /// <param name="pathToSrcPdf"></param>
        /// <param name="pathToDstPdf"></param>
        /// <param name="sigDictName"></param>
        /// <param name="signature"></param>
        /// <param name="certChain"></param>
        public void InsertSignature(string pathToSrcPdf, string pathToDstPdf, string sigDictName, byte[] signature, X509Certificate[] certChain)
        {
            var reader = new PdfReader(pathToSrcPdf);
            var rDoc = new PdfDocument(reader);
            var os = new FileStream(pathToDstPdf, FileMode.Create);

            var exSigContainer = new ReadySignatureSigner(signature, certChain, HASH_ALGORITHM, CRYPT_ALGORITHM);
            PdfSigner.SignDeferred(rDoc, sigDictName, os, exSigContainer);

            reader.Close();
            rDoc.Close();
            os.Close();
        }
    }

    public class ReadySignatureSigner : IExternalSignatureContainer
    {
        private readonly byte[] _signature;
        private readonly X509Certificate[] certChain;
        private readonly string hashAlg;
        private readonly string cryptAlg;

        public ReadySignatureSigner(byte[] signature, X509Certificate[] certChain, string hashAlg, string cryptAlg)
        {
            _signature = signature;
            this.certChain = certChain;
            this.hashAlg = hashAlg;
            this.cryptAlg = cryptAlg;
        }

        public void ModifySigningDictionary(PdfDictionary signDic)
        {
            // do nothing here :D
        }

        public byte[] Sign(Stream data) // except /Content already
        {
            var pdfPKCS7 = new PdfPKCS7(null, certChain, hashAlg, false);
            pdfPKCS7.SetExternalDigest(_signature, null, cryptAlg);

            var hash = DigestAlgorithms.Digest(data, hashAlg);
            byte[] encodedPKCS7 = pdfPKCS7.GetEncodedPKCS7(hash, PdfSigner.CryptoStandard.CMS, null, null, null);

            return encodedPKCS7;
        }
    }

    public class SignaturePosition
    {
        public int PageNumber { get; private set; }
        public float llx { get; private set; } // lower left x
        public float lly { get; private set; } // lower left y

        public SignaturePosition(int PageNumber, float llx, float lly)
        {
            this.PageNumber = PageNumber;
            this.llx = llx;
            this.lly = lly;
        }
    }
}