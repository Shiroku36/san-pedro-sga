using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb.Models;

namespace ControlPersonalAppWeb.Utils

{
    public class SessionManager
    {
        public static int trabajadorID = 0;
        public static string logo = "https://lh3.googleusercontent.com/t_Qep-pPQb5dI74BY9XBxb_EPgyuplvzbjdxPy2hXDW1bOQ62CXncnufS6JpLQIA2l9Z5tWChZvWgf0SEm9zLL-a4TM5aHJy8sUsHl7w8Au2Xvoz_9RQ07lJZzW-ytLPxXWTKD5UwVFoCWeQwmF55aEx_to8jG3a-157Rz6maV7DIxPD9A3b-CJ8vw8Z7F3wxIDi-xNJvqn3hQFnkocGEFubRLGvjmiaA-qybw4MMSLjaJQkc1pRmP_03sYnFPrynpLeuu5clFEq1z8dBZ0saTTmDum2mD6-0zjMRZsZiX2JXJDpj7fDTmmFTeP1LHslaQENdjmmW7PlRLn0L4xG1vpvZSgwYDjhzVN_jSGY-0ygWh8ENQwJEaOtQoWWB-YMDcvLJ5e5DuNMBBeg5jXfTl8JdzyEhGeIpwVI5CFLyDAX0vRoyeVocUsI7fSpo8J5swQq6Cdp1Y_QM-57lRk17Ay-17wqxSLhDdfzeQSEjMCpbF1daRZkeAvD7orwQEoLwfoUo_JxBWH_owqWt-MMJZUejdqA3wmh7FLh1G3MloRC7VBN801dvZ9I913KqmunTPLVwbx2QZKXxahP4U-3CdgrIsXgamiH_ltNxP8qghW087k1=w360-h153-no";
        public static Trabajador trabajador = new Trabajador();
        public static List<TrabajadorIndex> trabajadores = new List<TrabajadorIndex>();
        public static List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
        public static DateTime inicio = new DateTime();
        public static DateTime fin = new DateTime();

        public static byte[] FotoCarnet { get; set; }
        public static string campo = "";
        public static string tipo = "";
        public static string entrada = "";
        public static string salida = "";
        public static string almuerzo = "";
        public static int email = 1;
        public static string mensaje = "";
        public static string activa = "";
        public static void log(string data)
        {
            DateTime dateTime = DateTime.Now;
            string line = dateTime.ToString()+", Usuario: "+ Utils.SessionManager.CuentaAutenticada().Usuario + ", " + data;
            StreamWriter w = File.AppendText("C:\\Data\\Log.txt");
            w.WriteLine(line);
            w.Close();
        }
        public static Cuentas CuentaAutenticada()
        {
            var c = new Cuentas();
            if (HttpContext.Current.Request.Cookies["Cuenta"] != null
                && HttpContext.Current.Request.Cookies["Cuenta"].Value != string.Empty)
            {
                c.Id = Convert.ToInt32(HttpContext.Current.Request.Cookies["Cuenta"].Value);
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                c = database.Cuentas.First(x => x.Id == c.Id);
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
            cookieSesion.Value = c.Id.ToString();
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
        
    }
}