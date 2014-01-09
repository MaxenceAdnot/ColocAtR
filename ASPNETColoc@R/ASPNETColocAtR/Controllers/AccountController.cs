using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASPNETColocAtR.Models;
using ASPNETColocAtR.WSColocAtR;

namespace ASPNETColocAtR.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/
        [Connected]
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Account/ProfilWizard/
        [Connected]
        public ActionResult ProfilWizard()
        {
            string token = @Request.Cookies["User"]["Token"];
            string username = @Request.Cookies["User"]["Username"];

            WSProfile getProfile = new WSProfile();

            ProfilWizardModel profile = new ProfilWizardModel();

            using (ColocAtRClient client = new ColocAtRClient())
            {
                getProfile = client.RetrieveProfileUN(token, username);

                profile.age = getProfile.age;
                profile.city = (Convert.ToString(getProfile.city.ElementAt(0))).ToUpper() + getProfile.city.Substring(1);
                profile.desc = getProfile.desc;
                profile.m2 = getProfile.m2;
                profile.price = getProfile.price;
                profile.type = getProfile.type;

            }

            return View(profile);
        }

        [Connected]
        [HttpPost]
        public ActionResult ProfilWizard(ProfilWizardModel model)
        {
            if (ModelState.IsValid)
            {
                string token = @Request.Cookies["User"]["Token"];

                WSAuthMessage profilWizard = new WSAuthMessage();

                using (ColocAtRClient client = new ColocAtRClient())
                {
                    profilWizard = client.ProfilWizard(token, model.type, model.age, model.price, model.city, model.desc, model.m2);
                }

                if (profilWizard.StatusCode == StatusCode.OK)
                {
                    ViewBag.Message = profilWizard.Data;
                    return View();
                }

            }
            return View();
        }

        //
        // GET: /Account/ChangePassword/
        [Connected]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Connected]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                string token = @Request.Cookies["User"]["Token"];

                WSAuthMessage changePasswordMessage = new WSAuthMessage();

                using (ColocAtRClient client = new ColocAtRClient())
                {
                    changePasswordMessage = client.ChangePassword(token, model.oldPassword, model.newPassword);
                }

                if (changePasswordMessage.StatusCode == StatusCode.OK)
                {
                    ViewBag.Message = changePasswordMessage.Data;
                    return View();
                }

            }
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {

                WSAuthMessage createAccount = new WSAuthMessage();

                using (ColocAtRClient client = new ColocAtRClient())
                {
                    createAccount = client.CreateAccount(model.username, model.email, model.password, model.firstname, model.lastname);
                }

                if (createAccount.StatusCode == StatusCode.OK)
                {
                    return View("InscriptionSucces");
                }

            }
            return View();
        }

    }
}
