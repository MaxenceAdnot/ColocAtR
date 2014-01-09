using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASPNETColocAtR.WSColocAtR;
using System.Collections;

namespace ASPNETColocAtR.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [Connected]
        public ActionResult Scoring()
        {

            string token = @Request.Cookies["User"]["Token"];

            using (ColocAtRClient client = new ColocAtRClient())
            {
                WSProfile[] profiles = client.GetScoringResults(token);

                ViewBag.profilesTab = profiles;
            }

            return View();
        }
    }
}
