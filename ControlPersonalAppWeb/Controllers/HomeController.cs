using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb.Infrastructure;

namespace ControlPersonalAppWeb.Controllers
{
    public class HomeController : Controller
    {

        SgajcpEntities db = new SgajcpEntities();
        public ActionResult NuevoModulo()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult Pulseras()
        {
            ViewBag.Pulseras = db.Pulseras.ToList();
            return View();
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            //ViewBag.Message = "Your contact page.";
            ViewBag.alerta = 0;


            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(FormCollection form)
        {
            //ViewBag.Message = "Your contact page.";
            string nombre = form["nombre"];
            string telefono = form["telefono"];
            string email = form["email"];
            string comentario = form["comentario"];

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient();
                mail.From = new MailAddress(AppSettings.SmtpFromAddress,
                    AppSettings.SmtpFromName, System.Text.Encoding.UTF8);
                mail.To.Add("jcastro@ingenieriajcp.cl");
                //mail.To.Add("emilio.silva1@hotmail.com");
                mail.Bcc.Add("sebastianct36@outlook.com");
                mail.Subject = "Solicitud de Contacto de " + nombre ;
                Utils.SessionManager.email = 2;
                Utils.SessionManager.email = 1;
                mail.Body = nombre + " quiere tener informacion del sistema\nNumero de telefono: " + telefono + "\nComentario: " + comentario;
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Port = AppSettings.SmtpPort;
                SmtpServer.Host = AppSettings.SmtpHost;
                SmtpServer.Credentials = new System.Net.NetworkCredential(AppSettings.SmtpUser, AppSettings.SmtpPassword);
                SmtpServer.EnableSsl = AppSettings.SmtpEnableSsl;
                SmtpServer.Send(mail);
                mail.Dispose();
                SmtpServer.Dispose();
                Utils.SessionManager.mensaje = "Enviado correctamente";
                ViewBag.alerta = 1;
            }
            catch (Exception ex)
            {
                ex.ToString();
                Utils.SessionManager.mensaje = "Falló el envío";
                ViewBag.alerta = 2;
            }

            return View();
        }
        public ActionResult Log()
        {
            List<String> data = new List<string>();
            string logPath = System.IO.Path.Combine(AppSettings.FileStoragePath, "Log.txt");
            if (!System.IO.File.Exists(logPath))
            {
                ViewBag.Log = data;
                return View();
            }
            StreamReader r = new StreamReader(logPath);
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