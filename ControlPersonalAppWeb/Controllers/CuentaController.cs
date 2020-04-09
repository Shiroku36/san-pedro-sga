using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControlPersonalAppWeb.Controllers
{
    public class CuentaController : Controller
    {

        DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        // GET: Cuenta
        public ActionResult Index()
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            if(empresa == "JCP")
            {
                return View(database.Cuentas.ToList());
            }
            else
            {
                List<Cuentas> cuentas = database.Cuentas.Where(x => x.Empresa == empresa).ToList();
                return View(cuentas);
            }
        }

        // GET: Cuenta/Details/5
        public ActionResult Details(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Cuentas cuentas = database.Cuentas.First(x => x.Id == id);
            Utils.SessionManager.log("Cuenta visitada : " + cuentas.Usuario);
            return View(cuentas);
        }
        public string[] GetNombreEmpresas()
        {

            string[] nombres;
            var empresas = db.Empresas.Select(x => new { x.Nombre }).ToList();
            nombres = new string[empresas.Count];
            int count = 0;
            foreach (var empresita in empresas)
            {
                nombres[count] = empresita.Nombre;
                count++;
            }
            return nombres;
        }
        public string[] GetNombreCampos(string empresa)
        {

            string[] nombres;
            var campos = db.Campos.Select(x => new { x.Nombre }).ToList();
            if (!String.IsNullOrEmpty(empresa))
            {
                campos = db.Campos.Where(x => x.Empresa == empresa).Select(x => new { x.Nombre }).ToList();
            }
            nombres = new string[campos.Count];
            int count = 0;
            foreach (var campo in campos)
            {
                nombres[count] = campo.Nombre;
                count++;
            }
            return nombres;
        }
        // GET: Cuenta/Create
        public ActionResult Create()
        {
            //List<string> campos = new List<string>();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            string[] nombres;
            if (empresa == "JCP")
            {
                ViewBag.Empresas = GetNombreEmpresas();
                ViewBag.Campos = GetNombreCampos("");
            }
            else
            {
                nombres = new string[1];
                nombres[0] = empresa;
                ViewBag.Empresas = nombres;
                ViewBag.Campos = GetNombreCampos(empresa);
            }
            return View();
        }

        // POST: Cuenta/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                Cuentas cuentas = new Cuentas();
                cuentas.Usuario = collection["Usuario"];
                cuentas.Password = collection["Password"];
                cuentas.Empresa = collection["Empresa"];
                cuentas.Permisos = collection["Permisos"];
                database.Cuentas.Add(cuentas);
                database.SaveChanges();
                Utils.SessionManager.log("Cuenta creada : " + cuentas.Usuario);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Cuenta/Edit/5
        public ActionResult Edit(int id)
        {
            Cuentas cuentas = db.Cuentas.First(x => x.Id == id);
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            string[] nombres;
            if (empresa == "JCP")
            {
                ViewBag.Empresas = GetNombreEmpresas();
                ViewBag.Campos = GetNombreCampos("");
            }
            else
            {
                nombres = new string[1];
                nombres[0] = empresa;
                ViewBag.Empresas = nombres;
                ViewBag.Campos = GetNombreCampos(empresa);
            }
            return View(cuentas);
        }

        // POST: Cuenta/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                Cuentas cuentas = database.Cuentas.First(x => x.Id == id);
                Utils.SessionManager.log("Cuenta editada : " + cuentas.Usuario);
                cuentas.Usuario = collection["Usuario"];
                cuentas.Password = collection["Password"];
                cuentas.Empresa = collection["Empresa"];
                cuentas.Permisos = collection["Permisos"];
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                return View();
            }
        }

        // GET: Cuenta/Delete/5
        public ActionResult Delete(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Cuentas cuentas = database.Cuentas.First(x => x.Id == id);
            Utils.SessionManager.log("Cuenta eliminada : " + cuentas.Usuario);
            database.Cuentas.Remove(cuentas);
            database.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Cuenta/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        // GET: Cuenta/Delete/5
        public ActionResult Login()
        {
            ViewBag.Cuenta = "lala";
            return View();
        }

        // POST: Cuenta/Delete/5
        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            try
            {
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                Cuentas cuentas = new Cuentas();
                cuentas.Usuario = collection["Usuario"];
                cuentas.Password = collection["Password"];
                Cuentas cuenta = database.Cuentas.First(x => x.Usuario == cuentas.Usuario);
                if (cuentas.Password == cuenta.Password)
                {
                    Utils.SessionManager.Ingresar(cuenta);
                    Utils.SessionManager.log("Ingreso");
                    return RedirectToAction("Index", "Home");
                }
                ViewBag.Cuenta = "lala";
                return View();
            }
            catch
            {
                ViewBag.Cuenta = "lala";
                return View();
            }
        }
        public ActionResult Salir()
        {
            Utils.SessionManager.log("Salida");
            Utils.SessionManager.Salir();
            return RedirectToAction("Login", "Cuenta");
        }
    }
}
