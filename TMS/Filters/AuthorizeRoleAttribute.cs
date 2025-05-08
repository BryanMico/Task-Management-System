using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TMS.Filters
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string _role;

        public AuthorizeRoleAttribute(string role)
        {
            _role = role;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var isAuthenticated = filterContext.HttpContext.User.Identity.IsAuthenticated;
            var sessionRole = session["UserRole"] as string;

            bool authorized = isAuthenticated && sessionRole == _role;

            if (!authorized)
            {
                var referrer = filterContext.HttpContext.Request.UrlReferrer;
                var currentUrl = filterContext.HttpContext.Request.Url.AbsolutePath;

                if (referrer != null && referrer.AbsolutePath != currentUrl)
                {
                    // Redirect back to the previous page if it's not the same page
                    filterContext.Result = new RedirectResult(referrer.ToString());
                }
                else
                {
                    // If no referrer or same page, redirect to a safe location (Dashboard)
                    filterContext.Controller.TempData["UnauthorizedMessage"] = "You do not have permission to access this page.";
                    filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
            {
                { "controller", "Dashboard" },
                { "action", sessionRole == "Admin" ? "Dashboard_Admin" : "Dashboard_Client" }
            });
                }
            }

            base.OnActionExecuting(filterContext);
        }

    }
}