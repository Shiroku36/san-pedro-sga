using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ControlPersonalAppWeb.Infrastructure;
using ControlPersonalAppWeb.Models;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace ControlPersonalAppWeb.Utils

{
    public class SessionManager
    {
        // Session-backed properties (per-user, thread-safe)
        private static HttpSessionStateBase CurrentSession
        {
            get
            {
                var context = HttpContext.Current;
                if (context == null || context.Session == null)
                    return null;
                return new HttpSessionStateWrapper(context.Session);
            }
        }

        private static T GetSession<T>(string key, T defaultValue = default)
        {
            var session = HttpContext.Current?.Session;
            if (session == null) return defaultValue;
            var val = session[key];
            return val != null ? (T)val : defaultValue;
        }

        private static void SetSession<T>(string key, T value)
        {
            var session = HttpContext.Current?.Session;
            if (session != null)
                session[key] = value;
        }

        public static int trabajadorID
        {
            get => GetSession("SM_trabajadorID", 0);
            set => SetSession("SM_trabajadorID", value);
        }

        public static readonly string logo = "https://lh3.googleusercontent.com/t_Qep-pPQb5dI74BY9XBxb_EPgyuplvzbjdxPy2hXDW1bOQ62CXncnufS6JpLQIA2l9Z5tWChZvWgf0SEm9zLL-a4TM5aHJy8sUsHl7w8Au2Xvoz_9RQ07lJZzW-ytLPxXWTKD5UwVFoCWeQwmF55aEx_to8jG3a-157Rz6maV7DIxPD9A3b-CJ8vw8Z7F3wxIDi-xNJvqn3hQFnkocGEFubRLGvjmiaA-qybw4MMSLjaJQkc1pRmP_03sYnFPrynpLeuu5clFEq1z8dBZ0saTTmDum2mD6-0zjMRZsZiX2JXJDpj7fDTmmFTeP1LHslaQENdjmmW7PlRLn0L4xG1vpvZSgwYDjhzVN_jSGY-0ygWh8ENQwJEaOtQoWWB-YMDcvLJ5e5DuNMBBeg5jXfTl8JdzyEhGeIpwVI5CFLyDAX0vRoyeVocUsI7fSpo8J5swQq6Cdp1Y_QM-57lRk17Ay-17wqxSLhDdfzeQSEjMCpbF1daRZkeAvD7orwQEoLwfoUo_JxBWH_owqWt-MMJZUejdqA3wmh7FLh1G3MloRC7VBN801dvZ9I913KqmunTPLVwbx2QZKXxahP4U-3CdgrIsXgamiH_ltNxP8qghW087k1=w360-h153-no";

        public static Trabajador trabajador
        {
            get => GetSession<Trabajador>("SM_trabajador", null) ?? new Trabajador();
            set => SetSession("SM_trabajador", value);
        }

        public static List<TrabajadorIndex> trabajadores
        {
            get => GetSession<List<TrabajadorIndex>>("SM_trabajadores", null) ?? new List<TrabajadorIndex>();
            set => SetSession("SM_trabajadores", value);
        }

        public static List<RegistroTrabajador> registros
        {
            get => GetSession<List<RegistroTrabajador>>("SM_registros", null) ?? new List<RegistroTrabajador>();
            set => SetSession("SM_registros", value);
        }

        public static DateTime inicio
        {
            get => GetSession("SM_inicio", new DateTime());
            set => SetSession("SM_inicio", value);
        }

        public static DateTime fin
        {
            get => GetSession("SM_fin", new DateTime());
            set => SetSession("SM_fin", value);
        }

        public static byte[] FotoCarnet
        {
            get => GetSession<byte[]>("SM_FotoCarnet", null);
            set => SetSession("SM_FotoCarnet", value);
        }

        public static byte[] Foto
        {
            get => GetSession<byte[]>("SM_Foto", null);
            set => SetSession("SM_Foto", value);
        }

        public static byte[] FotoPruducto
        {
            get => GetSession<byte[]>("SM_FotoPruducto", null);
            set => SetSession("SM_FotoPruducto", value);
        }

        public static byte[] MiniaturaPruducto
        {
            get => GetSession<byte[]>("SM_MiniaturaPruducto", null);
            set => SetSession("SM_MiniaturaPruducto", value);
        }

        public static string campo
        {
            get => GetSession("SM_campo", "");
            set => SetSession("SM_campo", value);
        }

        public static string tipo
        {
            get => GetSession("SM_tipo", "");
            set => SetSession("SM_tipo", value);
        }

        public static string entrada
        {
            get => GetSession("SM_entrada", "");
            set => SetSession("SM_entrada", value);
        }

        public static string salida
        {
            get => GetSession("SM_salida", "");
            set => SetSession("SM_salida", value);
        }

        public static string almuerzo
        {
            get => GetSession("SM_almuerzo", "");
            set => SetSession("SM_almuerzo", value);
        }

        public static int email
        {
            get => GetSession("SM_email", 1);
            set => SetSession("SM_email", value);
        }

        public static string mensaje
        {
            get => GetSession("SM_mensaje", "");
            set => SetSession("SM_mensaje", value);
        }

        public static string activa
        {
            get => GetSession("SM_activa", "");
            set => SetSession("SM_activa", value);
        }

        public static int alerta
        {
            get => GetSession("SM_alerta", 0);
            set => SetSession("SM_alerta", value);
        }


        public static Cuentas CuentaAutenticada()
        {
            var context = HttpContext.Current;
            if (context == null || context.User == null || !context.User.Identity.IsAuthenticated)
            {
                return null;
            }

            try
            {
                int userId = Convert.ToInt32(context.User.Identity.Name);
                using (var database = new SgajcpEntities())
                {
                    return database.Cuentas.FirstOrDefault(x => x.Id == userId);
                }
            }
            catch
            {
                return null;
            }
        }
        public static void log(string Accion)
        {
            try
            {
                var cuentaActual = CuentaAutenticada();
                if(cuentaActual != null && cuentaActual.Id != 1)
                {
                    using (var database = new SgajcpEntities())
                    {
                        Log log = new Log()
                        {
                            Acción = Accion,
                            Empresa = cuentaActual.Empresa,
                            Usuario = cuentaActual.Usuario,
                            UsuarioId = cuentaActual.Id,
                            Fecha = DateTime.Now
                        };
                        database.Log.Add(log);
                        database.SaveChanges();
                    }
                }
            }
            catch
            {
                try
                {
                    using (var database = new SgajcpEntities())
                    {
                        Log log = new Log()
                        {
                            Acción = "Error ID",
                            Fecha = DateTime.Now
                        };
                        database.Log.Add(log);
                        database.SaveChanges();
                    }
                }
                catch { }
            }
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
            FormsAuthentication.SetAuthCookie(c.Id.ToString(), false);
        }

        public static int PerfilAutenticado()
        {
            var context = HttpContext.Current;
            if (context == null || context.Session == null)
                return 0;

            var perfil = context.Session["PerfilId"];
            return perfil != null ? (int)perfil : 0;
        }

        public static void IngresarPerfil(int ID)
        {
            var context = HttpContext.Current;
            if (context != null && context.Session != null)
            {
                context.Session["PerfilId"] = ID;
            }
        }

        public static void Salir()
        {
            var cuentaActual = CuentaAutenticada();
            Utils.SessionManager.log("Cuenta salir: " + (cuentaActual != null ? cuentaActual.Usuario : "sin sesión"));
            FormsAuthentication.SignOut();

            var context = HttpContext.Current;
            if (context != null && context.Session != null)
            {
                context.Session.Clear();
                context.Session.Abandon();
            }
        }
        public static int enviarCorreo(List<String> correos, string data)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient();
                mail.From = new MailAddress(AppSettings.SmtpFromAddress,
                    AppSettings.SmtpFromName, System.Text.Encoding.UTF8);
                foreach (var correo in correos)
                {
                    mail.To.Add(correo);
                }
                if(data == "Producto")
                {
                    mail.Subject = "Nueva solicitud de productos " ;
                }
                else
                {
                    mail.Subject = "Nueva solicitud de compra ";
                }
                mail.Body = data + "\n\nPuedes revisarla en:\nhttp://sgajcp.ingenieriajcp.cl/Cuenta/Login";
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Port = AppSettings.SmtpPort;
                SmtpServer.Host = AppSettings.SmtpHost;
                SmtpServer.Credentials = new System.Net.NetworkCredential(AppSettings.SmtpUser, AppSettings.SmtpPassword);
                SmtpServer.EnableSsl = AppSettings.SmtpEnableSsl;
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
