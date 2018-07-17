using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    [OutputCache(CacheProfile = "NoCacheProfile")]
    public class AccountController : Controller
    {
        public ActionResult SignIn()
        {
            this.ControllerContext.HttpContext.User = CustomAuth.GetFakeUser();
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
                //return View();
            }
        }

        // GET: SignOut
        public ActionResult SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Index", "Dashboard");
        }
    }
}