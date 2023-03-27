using Org.BouncyCastle.X509;
using System;
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
            var srcPdf = vm.SrcPdf;
            var index = srcPdf.LastIndexOf(".pdf");
            string sigEmbPdf = srcPdf.Substring(0, index) + ".sigemb.pdf";
            var presignPdf = srcPdf.Substring(0, index) + ".presign.pdf";

            string pathToSrcPdf = Server.MapPath("~/App_Data/" + srcPdf);
            string pathToSignatureEmbeddedPdf = Server.MapPath("~/App_Data/" + sigEmbPdf);
            string pathToPresignPdf = Server.MapPath("~/App_Data/" + presignPdf); ;
            string pathToImage = Server.MapPath("~/App_Data/signature_image.jpg");

            var certChain = CertUtil.GetCertChainFrom(vm.CommaSeparatedCertChainBase64);

            if (!certChain[0].IsValidNow)
            {
                return Error("Certificate is expired");
            }

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

            var signaturePostions = PdfUtil.GetSignaturePositions(pathToSrcPdf, "ký, ghi rõ họ tên");
            if (signaturePostions.Count() == 0)
            {
                return Error("Cannot find where to sign in file");
            }

            this._service.InsertSignatureGraphic(pathToSrcPdf, pathToSignatureEmbeddedPdf, signatureStamp, signaturePostions);

            this._service.EmptySignature(pathToSignatureEmbeddedPdf, pathToPresignPdf, signatureStamp);
            var digest = this._service.HashFile(pathToPresignPdf, signatureStamp.UniqueId, certChain);

            HttpContext.Cache[signatureStamp.UniqueId + "_certChain"] = certChain; // cache certChain
            HttpContext.Cache[signatureStamp.UniqueId + "_srcFileName"] = srcPdf;// cache source file

            return Success(
                    "Success",
                    new {
                        uniqueId = signatureStamp.UniqueId,
                        digest = Convert.ToBase64String(digest),
                        srcPdf = presignPdf,
                        serialNumber = certChain[0].SerialNumber.ToString(16)
                    }
            );
        }

        [HttpPost]
        public ActionResult InsertSignature([System.Web.Http.FromBody] InsertSignatureVM vm)
        {
            var certChain = (X509Certificate[])HttpContext.Cache[vm.UniqueId + "_certChain"];
            var srcPdf = (string)HttpContext.Cache[vm.UniqueId + "_srcFileName"];

            if (certChain == null)
            {
                throw new Exception("Must PreSign first");
            }

            var pathToSrcPdf = Server.MapPath("~/App_Data/" + vm.SrcPdf);
            var index = vm.SrcPdf.LastIndexOf(".presign.pdf");
            var dstPdf = srcPdf.Substring(0, index) + "sign.pdf";
            string pathToDstPdf = Server.MapPath("~/App_Data/" + dstPdf);

            this._service.InsertSignature(
                pathToSrcPdf,
                pathToDstPdf,
                vm.UniqueId,
                Convert.FromBase64String(vm.SignatureBases64),
                certChain
            );

            return Success(
                "Sign successful",
                new {
                    srcPdf = srcPdf,
                    dstPdf = dstPdf
                }
            );
        }
    }
}