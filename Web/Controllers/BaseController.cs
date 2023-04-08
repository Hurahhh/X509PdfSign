using System;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Web.Controllers
{
    public class BaseController : Controller
    {
        protected JavaScriptSerializer JSSerializer { get; private set; }

        public enum RESPONSE_STATUS : int
        {
            ERROR = -1,
            SUCCESS = 1,
        }

        public BaseController()
        {
            // when the amount of return data is huge
            // we need to do this
            JSSerializer = new JavaScriptSerializer();
            JSSerializer.MaxJsonLength = Int32.MaxValue;
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
            return new JsonResult()
            {
                Data = new
                {
                    Status = RESPONSE_STATUS.SUCCESS,
                    Message = !string.IsNullOrEmpty(message) ? message : "Success :D",
                    Data = optionalData
                },
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="optionalData"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public virtual JsonResult Error(string message = null, object optionalData = null, JsonRequestBehavior behavior = JsonRequestBehavior.DenyGet)
        {
            return new JsonResult()
            {
                Data = new
                {
                    Status = RESPONSE_STATUS.ERROR,
                    Message = !string.IsNullOrEmpty(message) ? message : "Something went wrong :(",
                    Data = optionalData
                },
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
    }
}