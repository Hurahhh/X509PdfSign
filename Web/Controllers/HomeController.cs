using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Web.Common;
using Web.Models;
using Web.Services;

namespace Web.Controllers
{
    public class HomeController : BaseController
    {
        private PdfSigningService _service;

        public HomeController()
        {
            this._service = PdfSigningService.GetInstance();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PreSign()
        {
            var vm = DeserializeBody<PreSignVM>();

            var pdfBytes = Convert.FromBase64String(vm.PdfBase64);
            var pdfStream = new MemoryStream(pdfBytes);

            var graphicBytes = ImageUtil.ResizeImage(640, Convert.FromBase64String(vm.GraphicBase64));
            var graphicStream = new MemoryStream(graphicBytes);

            var certChain = CertUtil.GetCertChainFrom(vm.CommaSeparatedCertChainBase64);
            if (!certChain[0].IsValidNow)
            {
                return Error("Certificate is expired");
            }

            var signatureStamp = SignatureStamp.Factory
                                    .SetCertificate(certChain[0])
                                    .SetWidth(SignatureStamp.DEFAULT_WIDTH)
                                    .SetHeight(SignatureStamp.DEFAULT_HEIGHT)
                                    .SetGraphic(Image.FromStream(graphicStream))
                                    .Build();

            var signaturePostions = PdfUtil.GetSignaturePositions(pdfStream, "ký, ghi rõ họ tên", signatureStamp);
            if (signaturePostions.Count() == 0)
            {
                return Error("Cannot find where to sign in file");
            }

            byte[] sigEmbPdfBytes = null;
            this._service.InsertSignatureGraphic(pdfBytes, out sigEmbPdfBytes, signatureStamp, signaturePostions);

            byte[] presignPdfBytes = null;
            this._service.EmptySignature(sigEmbPdfBytes, out presignPdfBytes, signatureStamp);
            var digest = this._service.HashFile(presignPdfBytes, signatureStamp.UniqueId, certChain);

            pdfStream.Dispose();
            graphicStream.Dispose();

            return Success(
                null,
                new
                {
                    UniqueId = signatureStamp.UniqueId,
                    Digest = Convert.ToBase64String(digest),
                    PdfBase64 = Convert.ToBase64String(presignPdfBytes),
                    SerialNumber = certChain[0].SerialNumber.ToString(16)
                }
            );
        }

        [HttpPost]
        public ActionResult InsertSignature()
        {
            var vm = this.DeserializeBody<InsertSignatureVM>();

            var certChain = CertUtil.GetCertChainFrom(vm.CommaSeparatedCertChainBase64);
            if (!certChain[0].IsValidNow)
            {
                return Error("Certificate is expired");
            }

            byte[] signPdf;
            this._service.InsertSignature(
                Convert.FromBase64String(vm.PdfBase64),
                out signPdf,
                vm.UniqueId,
                Convert.FromBase64String(vm.SignatureBases64),
                certChain
            );

            return Success(
                "Sign successful",
                new
                {
                    PdfBase64 = Convert.ToBase64String(signPdf)
                }
            );
        }
    }
}