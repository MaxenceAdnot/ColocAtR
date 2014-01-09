using System.Web.Mvc;
using System;
using System.Web;
using ASPNETColocAtR.WSColocAtR;
public class Connected : FilterAttribute, IAuthorizationFilter
{
    void IAuthorizationFilter.OnAuthorization(AuthorizationContext filterContext)
    {
        string token = String.Empty;
        string username = String.Empty;

        try
        {
            token = @filterContext.HttpContext.Request.Cookies["User"]["Token"];
            username = @filterContext.HttpContext.Request.Cookies["User"]["Username"];
            filterContext.Controller.ViewBag.Token = token;
            filterContext.Controller.ViewBag.Username = username;
        }
        catch (Exception)
        {
            //filterContext.Result = new RedirectResult("~/");
        }

        // Vérification de la session
        WSAuthMessage retrieveLastActivityMessage;

        using (ColocAtRClient client = new ColocAtRClient())
        {
            retrieveLastActivityMessage = client.RefreshLastActivity(token);
        }
        if (retrieveLastActivityMessage.StatusCode == StatusCode.Error ||
           retrieveLastActivityMessage.StatusCode == StatusCode.AccessRefused)
        {
            // Suppression cookie
            if (@filterContext.HttpContext.Request.Cookies["User"] != null)
            {
                HttpCookie myCookie = new HttpCookie("User");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                @filterContext.HttpContext.Response.Cookies.Add(myCookie);
            }
            filterContext.Result = new RedirectResult("~/");
        }
    }
}