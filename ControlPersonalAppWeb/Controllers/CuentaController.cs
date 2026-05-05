using ControlPersonalAppWeb.Infrastructure;
using ControlPersonalAppWeb.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ControlPersonalAppWeb.Controllers
{
    public class CuentaController : Controller
    {
        
        SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta => Utils.SessionManager.CuentaAutenticada();

        private string ObtenerEmpresaActual()
        {
            var cuentaActual = Utils.SessionManager.CuentaAutenticada();
            if (cuentaActual != null && !String.IsNullOrWhiteSpace(cuentaActual.Empresa))
            {
                return cuentaActual.Empresa;
            }

            if (cuenta != null && !String.IsNullOrWhiteSpace(cuenta.Empresa))
            {
                return cuenta.Empresa;
            }

            return "VSP";
        }
        // GET: Cuenta
        public ActionResult Index()
        {
            SgajcpEntities database = new SgajcpEntities();
            Utils.SessionManager.log("Cuenta índice");
            return View(database.Cuentas.ToList());
        }

        // GET: Cuenta/Details/5
        public ActionResult Details(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Cuentas cuentas = database.Cuentas.FirstOrDefault(x => x.Id == id);
            if (cuentas == null)
                return HttpNotFound();
            Utils.SessionManager.log("Cuenta detalle: " + cuentas.Usuario);
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
        public ActionResult Password(int id)
        {
            ViewBag.id = id;
            Cuentas cuentas = db.Cuentas.FirstOrDefault(x => x.Id == id);
            if (cuentas == null)
                return HttpNotFound();
            return View(cuentas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Password(FormCollection collection)
        {
            try
            {

                int id = Convert.ToInt32(collection["Id"]);
                Cuentas cuentas = db.Cuentas.First(x => x.Id == id);
                if (cuentas.Password == collection["Contraseña"])
                {
                    cuentas.Password = collection["Password"];
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id });
                }
                else
                {
                    ViewBag.alerta = 2;
                    return View(cuentas);
                }
                
            }
            catch
            {
                return View();
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult createTrabajador(int id)
        {
            var cuentaExistente = db.Cuentas.FirstOrDefault(x => x.TrabajadorId == id);
            if (cuentaExistente != null)
            {
                Utils.SessionManager.alerta = 2;
                return RedirectToAction("Details", "Trabajador", new { id = id });
            }

            Trabajador trabajador = db.Trabajador.FirstOrDefault(x => x.Id == id);
            if (trabajador == null)
                return HttpNotFound();

            if (string.IsNullOrEmpty(trabajador.Email))
            {
                Utils.SessionManager.alerta = 3;
                return RedirectToAction("Details", "Trabajador", new { id = id });
            }

            Empresas empresa = db.Empresas.FirstOrDefault(x => x.Nombre == trabajador.Empresa);
            if (empresa == null)
                return RedirectToAction("Details", "Trabajador", new { id = id });

            string usuario = trabajador.Rut.Replace(".", "").Replace("-", "");
            Cuentas cuenta = new Cuentas()
            {
                Nivel = 6,
                TrabajadorId = id,
                Usuario = usuario,
                Password = usuario,
                Nombre = trabajador.Nombre,
                Apellido = trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno,
                Email = trabajador.Email,
                Empresa = trabajador.Empresa,
                EmpresaId = empresa.Id,
                Permisos = ""
            };
            db.Cuentas.Add(cuenta);
            db.SaveChanges();
            Utils.SessionManager.alerta = 1;
            return RedirectToAction("Details", "Trabajador", new { id = id });
        }
                // GET: Cuenta/Create
        public ActionResult Create()
        {
            //List<string> campos = new List<string>();
            string empresa = ObtenerEmpresaActual();
            ViewBag.personas = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Rut = x.Rut, Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno }).ToList();
            ViewBag.empresas = db.Empresas.Select(x => x.Nombre).OrderBy(x => x).ToList();
            ViewBag.campos = db.Campos.Select(x => x.Nombre ).OrderBy(x => x).ToList();
            return View();
        }

        // POST: Cuenta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                Cuentas cuentas = new Cuentas();
                cuentas.Usuario = collection["Usuario"];
                cuentas.Password = collection["Password"];
                cuentas.Empresa = collection["Empresa"];
                cuentas.Nombre = collection["Nombre"];
                cuentas.Apellido = collection["Apellido"];
                cuentas.Empresa = collection["Empresa"];
                cuentas.Permisos = collection["Permisos"];
                cuentas.Email = collection["Email"];
                database.Cuentas.Add(cuentas);
                Utils.SessionManager.log("Cuenta crear: " + cuentas.Usuario);
                database.SaveChanges();
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
            Cuentas cuentas = db.Cuentas.FirstOrDefault(x => x.Id == id);
            if (cuentas == null)
                return HttpNotFound();
            string empresa = ObtenerEmpresaActual();
            ViewBag.personas = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Rut = x.Rut, Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno }).ToList();
            ViewBag.empresas = db.Empresas.Select(x => x.Nombre).OrderBy(x => x).ToList();
            ViewBag.campos = db.Campos.Select(x => x.Nombre).OrderBy(x => x).ToList();
            return View(cuentas);
        }

        // POST: Cuenta/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                Cuentas cuentas = database.Cuentas.First(x => x.Id == id);
                cuentas.Usuario = collection["Usuario"];
                cuentas.Password = collection["Password"];
                cuentas.Empresa = collection["Empresa"];
                cuentas.Nombre = collection["Nombre"];
                cuentas.Apellido = collection["Apellido"];
                cuentas.Empresa = collection["Empresa"];
                cuentas.Email = collection["Email"];
                cuentas.Permisos = collection["Permisos"];
                Utils.SessionManager.log("Cuenta editar: " + cuentas.Usuario);
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
            SgajcpEntities database = new SgajcpEntities();
            Cuentas cuentas = database.Cuentas.FirstOrDefault(x => x.Id == id);
            if (cuentas == null)
                return HttpNotFound();
            return View(cuentas);
        }

        // POST: Cuenta/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                Cuentas cuentas = database.Cuentas.First(x => x.Id == id);
                Utils.SessionManager.log("Cuenta eliminar: " + cuentas.Usuario);
                database.Cuentas.Remove(cuentas);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            ViewBag.Cuenta = "lala";
            if(cuenta!=null)
            {
                return RedirectToAction("Index","Home");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(FormCollection collection)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                Cuentas cuentas = new Cuentas();
                cuentas.Usuario = collection["Usuario"];
                cuentas.Password = collection["Password"];
                Cuentas cuenta = database.Cuentas.FirstOrDefault(x => x.Usuario == cuentas.Usuario);
                if (cuenta == null || cuentas.Password != cuenta.Password)
                {
                    ViewBag.error = "Cuenta no encontrada y/o contraseña incorrecta";
                    ViewBag.Cuenta = "lala";
                    return View();
                }
                Utils.SessionManager.Ingresar(cuenta);
                if(cuenta.Nivel==6)
                {
                    return RedirectToAction("Index", "Registro", new { id = cuenta.TrabajadorId });
                }
                Utils.SessionManager.log("Cuenta ingreso: " + cuentas.Usuario);
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ViewBag.error = "Cuenta no encontrada y/o contraseña incorrecta";
                ViewBag.Cuenta = "lala";
                return View();
            }
        }
        public ActionResult Salir()
        {
            Utils.SessionManager.Salir();
            return RedirectToAction("Login", "Cuenta");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
