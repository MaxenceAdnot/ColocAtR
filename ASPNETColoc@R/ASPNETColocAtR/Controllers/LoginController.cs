using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASPNETColocAtR.WSColocAtR;
using ASPNETColocAtR.Models;

namespace ASPNETColocAtR.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/
        public ActionResult Index(string token)
        {
            if (null != Request.Cookies["User"])
            {
                return RedirectToAction("Index", "Account");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(AccountModel model, string token)
        {
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    return View();
                }
                else
                {
                    WSAuthMessage createSessionWithUIDMessage = new WSAuthMessage();
                    WSAuthMessage signInMessage = new WSAuthMessage();

                    using (ColocAtRClient client = new ColocAtRClient())
                    {
                        signInMessage = client.SignIn(model.Email, model.Password);
                    }
                    if (signInMessage.StatusCode == StatusCode.AccessRefused || signInMessage.StatusCode == StatusCode.Error)
                    {
                        ModelState.AddModelError("", signInMessage.Data);
                        return View();
                    }
                    Response.Cookies["User"]["Token"] = signInMessage.Data;

                    using (ColocAtRClient client = new ColocAtRClient())
                    {
                        Response.Cookies["User"]["Username"] = client.Whoami(signInMessage.Data).Data;
                    }

                    Response.Cookies["User"].Expires = DateTime.Now.AddDays(1);
                }

             if (ModelState.IsValid)
                    return RedirectToAction("Index", "Account");

                return View();
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [Connected]
        public ActionResult SignOut()
        {
            WSAuthMessage signoutMessage;
            using (ColocAtRClient client = new ColocAtRClient())
            {
                signoutMessage = client.SignOut(ViewBag.token);
            }
            if (Request.Cookies["User"] != null)
            {
                var c = new HttpCookie("User");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }
            return RedirectToAction("Index");
        }
    }
}