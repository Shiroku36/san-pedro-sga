using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb.Infrastructure;

namespace ControlPersonalAppWeb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequireAuthAttribute());
        }
    }
}
