using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ControlPersonalAppWeb.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireAuthAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                return false;

            return httpContext.User != null && httpContext.User.Identity.IsAuthenticated;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new
                {
                    controller = "Cuenta",
                    action = "Login",
                    returnUrl = filterContext.HttpContext.Request.RawUrl
                }));
        }
    }
}
