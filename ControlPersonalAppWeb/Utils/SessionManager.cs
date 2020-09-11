using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb.Models;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace ControlPersonalAppWeb.Utils

{
    public class SessionManager
    {
        public static DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        public static int trabajadorID = 0;
        public static string logo = "https://lh3.googleusercontent.com/t_Qep-pPQb5dI74BY9XBxb_EPgyuplvzbjdxPy2hXDW1bOQ62CXncnufS6JpLQIA2l9Z5tWChZvWgf0SEm9zLL-a4TM5aHJy8sUsHl7w8Au2Xvoz_9RQ07lJZzW-ytLPxXWTKD5UwVFoCWeQwmF55aEx_to8jG3a-157Rz6maV7DIxPD9A3b-CJ8vw8Z7F3wxIDi-xNJvqn3hQFnkocGEFubRLGvjmiaA-qybw4MMSLjaJQkc1pRmP_03sYnFPrynpLeuu5clFEq1z8dBZ0saTTmDum2mD6-0zjMRZsZiX2JXJDpj7fDTmmFTeP1LHslaQENdjmmW7PlRLn0L4xG1vpvZSgwYDjhzVN_jSGY-0ygWh8ENQwJEaOtQoWWB-YMDcvLJ5e5DuNMBBeg5jXfTl8JdzyEhGeIpwVI5CFLyDAX0vRoyeVocUsI7fSpo8J5swQq6Cdp1Y_QM-57lRk17Ay-17wqxSLhDdfzeQSEjMCpbF1daRZkeAvD7orwQEoLwfoUo_JxBWH_owqWt-MMJZUejdqA3wmh7FLh1G3MloRC7VBN801dvZ9I913KqmunTPLVwbx2QZKXxahP4U-3CdgrIsXgamiH_ltNxP8qghW087k1=w360-h153-no";
        public static Trabajador trabajador = new Trabajador();
        public static List<TrabajadorIndex> trabajadores = new List<TrabajadorIndex>();
        public static List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
        public static DateTime inicio = new DateTime();
        public static DateTime fin = new DateTime();
        public static List<Notificacion> notificaciones = new List<Notificacion>();

        public static byte[] FotoCarnet { get; set; }
        public static byte[] FotoPruducto { get; set; }
        public static byte[] MiniaturaPruducto { get; set; }
        public static string campo = "";
        public static string tipo = "";
        public static string entrada = "";
        public static string salida = "";
        public static string almuerzo = "";
        public static int email = 1;
        public static string mensaje = "";
        public static string activa = "";
        public static int alerta = 0;

        public static List<Notificacion>  getNotificaciones() {
            int id;
            if (CuentaAutenticada()!=null && CuentaAutenticada().Notificacion!=null && (bool)CuentaAutenticada().Notificacion)
            {
                id = (int)CuentaAutenticada().EmpresaId;
                notificaciones = db.Notificacion.Where(x => x.CuentaId == id && x.Estado == "Solicitado").ToList().OrderByDescending(x => x.Fecha).ToList();
            }
            return notificaciones;
        }


        public static void log(string Accion)
        {
            try
            {
                Log log = new Log()
                {
                    Accion = Accion,
                    Empresa = CuentaAutenticada().Empresa,
                    EmpresaId = CuentaAutenticada().EmpresaId,
                    Usuario = CuentaAutenticada().Usuario,
                    UsuarioId = CuentaAutenticada().Id,
                    Fecha = DateTime.Now
                };
                db.Log.Add(log);
                db.SaveChanges();
            }
            catch
            {
                Log log = new Log()
                {
                    Accion = "Error ID",
                    Fecha = DateTime.Now
                };
                db.Log.Add(log);
                db.SaveChanges();
                Salir();

            }
        }
        public void agregarLog(Log log)
        {
        }
        public static Cuentas CuentaAutenticada()
        {
            var c = new Cuentas();
            if (HttpContext.Current.Request.Cookies["Cuenta"] != null
                && HttpContext.Current.Request.Cookies["Cuenta"].Value != string.Empty)
            {
                c.Id = Convert.ToInt32(HttpContext.Current.Request.Cookies["Cuenta"].Value)/(464 * 653);
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                try
                {
                    c = database.Cuentas.First(x => x.Id == c.Id);
                }
                catch 
                { 
                    Salir();
                    return null;
                }
            }
            else
            {
                c = null;
            }
            return c;
        }

        public static bool Cuenta()
        {
            if(CuentaAutenticada()==null)
            {
                return true;
            }
            return false;
        }
        public static void Ingresar(Cuentas c)
        {
            var cookieSesion = new HttpCookie("Cuenta");
            long id = 464 * 653 * c.Id;
            cookieSesion.Value = id.ToString();
            HttpContext.Current.Response.Cookies.Add(cookieSesion);
        }

        public static int PerfilAutenticado()
        {
            var ID = 0;
            if (HttpContext.Current.Request.Cookies["Perfil"] != null
                && HttpContext.Current.Request.Cookies["Perfil"].Value != string.Empty)
            {
                ID = Convert.ToInt32(HttpContext.Current.Request.Cookies["Perfil"].Value);
                
            }
            
            return ID;
        }

        public static void IngresarPerfil(int ID)
        {
            int perfil_id = ID;
            var cookieSesion = new HttpCookie("Perfil");
            //cookieSesion.Expires = DateTime.Now.AddDays(1);
            cookieSesion.Value = ID.ToString();
            HttpContext.Current.Response.Cookies.Add(cookieSesion);
        }

        public static void Salir()
        {
            var cookieSesion = new HttpCookie("Cuenta");
            cookieSesion.Expires = DateTime.Now.AddDays(-1);
            cookieSesion.Value = string.Empty;
            var cookiePerfil = new HttpCookie("Perfil");
            cookiePerfil.Expires = DateTime.Now.AddDays(-1);
            cookiePerfil.Value = string.Empty;
            HttpContext.Current.Response.Cookies.Add(cookieSesion);
            HttpContext.Current.Response.Cookies.Add(cookiePerfil);
        }
        public static int enviarCorreo(List<String> correos, Notificacion notificacion)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient();
                mail.From = new MailAddress("notificacionjcp@ingenieriajcp.cl",
                "SGA JCP", System.Text.Encoding.UTF8);
                foreach (var correo in correos)
                {
                    mail.To.Add(correo);
                    log("Correo de notificacion enviado a:" + correo);
                }
                if(notificacion.Info == "Producto")
                {
                    mail.Subject = "Nueva solicitud de productos " + notificacion.Fecha.Value.ToLongDateString();
                }
                else
                {
                    mail.Subject = "Nueva solicitud de compra " + notificacion.Fecha.Value.ToLongDateString();
                }
                mail.Body = notificacion.Texto + "\n\nPuedes revisarla en:\nhttp://sgajcp.ingenieriajcp.cl/Cuenta/Login";
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Port = 25;
                SmtpServer.Host = "mail.ingenieriajcp.cl";
                SmtpServer.Credentials = new System.Net.NetworkCredential("notificacionjcp@ingenieriajcp.cl", "notificacion");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                mail.Dispose();
                SmtpServer.Dispose();
                Utils.SessionManager.mensaje = "Correo enviado correctamente";
                return 1;
            }
            catch (Exception ex)
            {
                ex.ToString();
                Utils.SessionManager.mensaje = "Falló el envío del correo";
                return 2;
            }
        }
    }
}