using Org.BouncyCastle.X509;
using System;
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
        public static string BASE_STORAGE_DIR = "~/App_Data/";

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

            string pathToSrcPdf = Server.MapPath(BASE_STORAGE_DIR + srcPdf);
            string pathToSigEmbPdf = Server.MapPath(BASE_STORAGE_DIR + sigEmbPdf);
            string pathToPresignPdf = Server.MapPath(BASE_STORAGE_DIR + presignPdf); ;
            string pathToGraphic = Server.MapPath(BASE_STORAGE_DIR + "signature_image.jpg");

            #region Checking
            var fiSrcPdf = new FileInfo(pathToSrcPdf);
            var fiSigEmbPdf = new FileInfo(pathToSigEmbPdf);
            var fiPresignPdf = new FileInfo(pathToPresignPdf);
            var fiGraphic = new FileInfo(pathToGraphic);

            if (!fiSrcPdf.Exists)
            {
                return Error("Source file does not exist");
            }
            if (FileUtil.IsLocked(fiSrcPdf))
            {
                return Error("Source file is unaccessable");
            }
            if (fiSigEmbPdf.Exists && FileUtil.IsLocked(fiSigEmbPdf))
            {
                return Error("Server error: fiSigEmbPdf");
            }
            if (fiPresignPdf.Exists && FileUtil.IsLocked(fiPresignPdf))
            {
                return Error("Server error: fiSigEmbPdf");
            }
            if (!fiGraphic.Exists)
            {
                return Error("Signature graphic does not exist");
            }
            if (FileUtil.IsLocked(fiGraphic))
            {
                return Error("Signature graphic is unaccessable");
            }
            #endregion

            var certChain = CertUtil.GetCertChainFrom(vm.CommaSeparatedCertChainBase64);
            if (!certChain[0].IsValidNow)
            {
                return Error("Certificate is expired");
            }

            var signatureStamp = new SignatureStamp
            {
                PathToImage = pathToGraphic,
                Contact = "binhld8@viettel.com.vn",
                Width = SignatureStamp.DEFAULT_WIDTH,
                Height = SignatureStamp.DEFAULT_HEIGHT,
                certificate = certChain[0],
                Location = "Hanoi",
                Reason = "Signed",
            };

            var signaturePostions = PdfUtil.GetSignaturePositions(pathToSrcPdf, "ký, ghi rõ họ tên");
            if (signaturePostions.Count() == 0)
            {
                return Error("Cannot find where to sign in file");
            }

            this._service.InsertSignatureGraphic(pathToSrcPdf, pathToSigEmbPdf, signatureStamp, signaturePostions);

            this._service.EmptySignature(pathToSigEmbPdf, pathToPresignPdf, signatureStamp);
            var digest = this._service.HashFile(pathToPresignPdf, signatureStamp.UniqueId, certChain);

            HttpContext.Cache[signatureStamp.UniqueId + "_certChain"] = certChain; // cache certChain
            HttpContext.Cache[signatureStamp.UniqueId + "_srcFileName"] = srcPdf;// cache source file

            return Success(
                    "Success",
                    new {
                        UniqueId = signatureStamp.UniqueId,
                        Digest = Convert.ToBase64String(digest),
                        SrcPdf = presignPdf,
                        SerialNumber = certChain[0].SerialNumber.ToString(16)
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

            var pathToSrcPdf = Server.MapPath(BASE_STORAGE_DIR + vm.SrcPdf);
            var index = vm.SrcPdf.LastIndexOf(".presign.pdf");
            if (index < 0)
            {
                return Error("Malformed source pdf");
            }
            var dstPdf = srcPdf.Substring(0, index) + "sign.pdf";
            string pathToDstPdf = Server.MapPath(BASE_STORAGE_DIR + dstPdf);

            #region Checking
            var fiSrcPdf = new FileInfo(pathToSrcPdf);
            var fiDstPdf = new FileInfo(pathToDstPdf);
            if (!fiSrcPdf.Exists)
            {
                return Error("Must presign first");
            }
            if (FileUtil.IsLocked(fiSrcPdf))
            {
                return Error("Presign file is unaccessable");
            }
            if (fiDstPdf.Exists && FileUtil.IsLocked(fiDstPdf))
            {
                return Error("Server error: fiDstPdf");
            }
            #endregion

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
                    SrcPdf = srcPdf,
                    DstPdf = dstPdf
                }
            );
        }
    }
}