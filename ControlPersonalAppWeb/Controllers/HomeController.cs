using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControlPersonalAppWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            //ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Log()
        {
            List<String> data = new List<string>();
            Utils.SessionManager.log("Log");
            StreamReader r = new StreamReader("C:\\Data\\Log.txt");
            string line = "";
            while ((line = r.ReadLine()) != null)
            {
                data.Add(line);
            }
            r.Close();
            data.Reverse();
            ViewBag.Log = data;

            return View();
        }
    }
}