using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;

namespace ControlPersonalAppWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult NuevoModulo()
        {
            return View();
        }
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
            ViewBag.alerta = 0;


            return View();
        }
        [HttpPost]
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
                mail.From = new MailAddress("notificacionjcp@ingenieriajcp.cl",
                "SGA JCP", System.Text.Encoding.UTF8);
                mail.To.Add("jcastro@ingenieriajcp.cl");
                //mail.To.Add("emilio.silva1@hotmail.com");
                mail.Bcc.Add("sebastianct36@outlook.com");
                mail.Subject = "Solicitud de Contacto de " + nombre ;
                Utils.SessionManager.email = 2;
                Utils.SessionManager.email = 1;
                mail.Body = nombre + " quiere tener informacion del sistema\nNumero de telefono: " + telefono + "\nComentario: " + comentario;
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Port = 25;
                SmtpServer.Host = "mail.ingenieriajcp.cl";
                SmtpServer.Credentials = new System.Net.NetworkCredential("notificacionjcp@ingenieriajcp.cl", "notificacion");
                SmtpServer.EnableSsl = true;
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