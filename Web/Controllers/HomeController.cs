using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
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
        public ActionResult PreSign([System.Web.Http.FromBody] PreSignVM vm)
        {
            string pathToSrcPdf = Server.MapPath("~/App_Data/one_page.pdf");
            var index = pathToSrcPdf.LastIndexOf(".pdf");
            string pathToSignatureEmbeddedPdf = pathToSrcPdf.Substring(0, index) + ".sigemb.pdf";
            string pathToPresignedPdf = pathToSrcPdf.Substring(0, index) + ".presigned.pdf";
            string pathToImage = Server.MapPath("~/App_Data/signature_image.jpg");

            var certChain = CertUtil.GetCertChainFrom(vm.CommaSeparatedCertChainBase64);

            var signatureStamp = new SignatureStamp
            {
                PathToImage = pathToImage,
                Contact = "binhld8@viettel.com.vn",
                Width = 80,
                Height = 60,
                certificate = certChain[0],
                Location = "Hanoi",
                Reason = "Signed",
            };

            var signaturePostions = PdfUtil.GetSignaturePositions(pathToSrcPdf, "Ký, ghi rõ họ tên");

            this._service.InsertSignatureGraphic(pathToSrcPdf, pathToSignatureEmbeddedPdf, signatureStamp, signaturePostions);

            this._service.EmptySignature(pathToSignatureEmbeddedPdf, pathToPresignedPdf, signatureStamp);
            var digest = this._service.HashFile(pathToPresignedPdf, signatureStamp.UniqueId, certChain);

            HttpContext.Cache[signatureStamp.UniqueId] = certChain;

            return Json(new { uniqueId = signatureStamp.UniqueId, digest = Convert.ToBase64String(digest), srcPath = pathToPresignedPdf });
        }

        [HttpPost]
        public ActionResult InsertSignature([System.Web.Http.FromBody] InsertSignatureVM vm)
        {
            var certChain = (X509Certificate[])HttpContext.Cache[vm.UniqueId];

            if (certChain == null)
            {
                throw new Exception("Must PreSign first");
            }

            var index = vm.PathToSrcPdf.LastIndexOf(".presigned.pdf");
            string pathToDstPdf = vm.PathToSrcPdf.Substring(0, index) + ".signed.pdf";

            this._service.InsertSignature(
                vm.PathToSrcPdf,
                pathToDstPdf,
                vm.UniqueId,
                Convert.FromBase64String(vm.SignatureBases64),
                certChain
            );

            return File(pathToDstPdf, "application/pdf");
        }
    }
}