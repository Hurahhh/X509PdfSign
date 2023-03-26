using System.Web.Mvc;

namespace Web.Controllers
{
    public class BaseController : Controller
    {
        public enum RESPONSE_STATUS : int
        {
            ERROR = -1,
            SUCCESS = 1,
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="optionalData"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public virtual JsonResult Success(string message, object optionalData = null, JsonRequestBehavior behavior = JsonRequestBehavior.DenyGet)
        {
            return Json(new
            {
                Status = RESPONSE_STATUS.SUCCESS,
                Message = !string.IsNullOrEmpty(message) ? message : "Success :D",
                Data = optionalData
            }, behavior);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="optionalData"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public virtual JsonResult Error(string customMessage = null, object optionalData = null, JsonRequestBehavior behavior = JsonRequestBehavior.DenyGet)
        {
            return Json(new
            {
                Status = RESPONSE_STATUS.ERROR,
                Message = !string.IsNullOrEmpty(customMessage) ? customMessage : "Something went wrong :(",
                Data = optionalData
            }, behavior);
        }
    }
}