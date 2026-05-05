using ControlPersonalAppWeb.Infrastructure;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControlPersonalAppWeb.Controllers
{
    public class EmpresaController : Controller
    {
        private Cuentas cuenta => Utils.SessionManager.CuentaAutenticada();
        SgajcpEntities db = new SgajcpEntities();
        // GET: Empresas
        public ActionResult Habilitados(int Id)
        {
            try
            {
                db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.Trabajador SET Habilitado = 0 WHERE Expiración IS NOT NULL AND Expiración < @p0 AND Habilitado = 1",
                    DateTime.Today);
            }
            catch { }
            Empresas empresa = db.Empresas.FirstOrDefault(x => x.Id == Id);
            if (empresa == null)
                return HttpNotFound();
            List<Trabajador> trabajadores = db.Trabajador.Where(x => x.Empresa == empresa.Nombre).ToList();
            ViewBag.id = Id;
            return View(trabajadores);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Habilitados(FormCollection collection)
        {
            int Id =Convert.ToInt32(collection["IdEmpresa"]);
            Empresas empresa = db.Empresas.FirstOrDefault(x => x.Id == Id);
            if (empresa == null)
                return RedirectToAction("Index");
            foreach (var trabajador in db.Trabajador.Where(x => x.Empresa == empresa.Nombre).ToList())
            {
                trabajador.Habilitado = false;
            }
            db.SaveChanges();
            if (!string.IsNullOrEmpty(collection["esta"]))
            {
                string[] ids = collection["esta"].Split(',');
                foreach (string aid in ids)
                {
                    int id = Convert.ToInt32(aid);
                    Trabajador trabajador = db.Trabajador.FirstOrDefault(x => x.Id == id);
                    if (trabajador == null)
                        continue;
                    trabajador.Habilitado = true;
                }
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        public ActionResult Index()
        {
            SgajcpEntities database = new SgajcpEntities();
            Utils.SessionManager.log("Contratistas índice");
            return View(database.Empresas.OrderBy(x => x.Nombre).ToList());
        }

        // GET: Empresas/Details/5
        public ActionResult Details(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Empresas empresas = database.Empresas.FirstOrDefault(x => x.Id == id);
            if (empresas == null)
                return HttpNotFound();
            return View(empresas);
        }

        // GET: Empresas/Create
        public ActionResult Create()
        {
            if(cuenta.Empresa!="JCP")
            {
                ViewBag.proveedor = true;
            }
            ViewBag.proveedor = true;
            return View();
        }

        // POST: Empresas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection, HttpPostedFileBase file)
        {
            try
            {
                // TODO: Add insert logic here

                SgajcpEntities database = new SgajcpEntities();
                Empresas empresa = new Empresas();
                empresa.Nombre = collection["Nombre"];  
                empresa.Nomina = collection["Nomina"];  
                empresa.Rut = formatearRut(collection["Rut"]);

                HttpPostedFileBase postedFile = Request.Files["Nomina"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    string savedName = FileUploadService.SaveFile(postedFile, "Empresas");
                    empresa.Nomina = savedName;
                }
                database.Empresas.Add(empresa);
                Utils.SessionManager.log("Contratistas crear: " + empresa.Nombre);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch 
            {
                
                return View();
            }
        }

        // GET: Empresas/Edit/5
        public ActionResult Edit(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Empresas empresa = database.Empresas.FirstOrDefault(x => x.Id == id);
            if (empresa == null)
                return HttpNotFound();
            return View(empresa);
        }

        // POST: Empresas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, FormCollection collection, HttpPostedFileBase file)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                Empresas empresa = database.Empresas.First(x => x.Id == id);
                empresa.Nombre = collection["Nombre"];
                empresa.Rut = collection["Rut"];
                empresa.Nomina = collection["Nomina"];
                HttpPostedFileBase postedFile = Request.Files["Nomina1"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    string savedName = FileUploadService.SaveFile(postedFile, "Empresas");
                    empresa.Nomina = savedName;
                }
                Utils.SessionManager.log("Contratistas editar: " + empresa.Nombre);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
            }
            return View();
        }

        // GET: Empresas/Delete/5
        public ActionResult Delete(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Empresas empresa = database.Empresas.FirstOrDefault(x => x.Id == id);
            if (empresa == null)
                return HttpNotFound();
            return View(empresa);
        }

        // POST: Empresas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                Empresas empresa = database.Empresas.First(x => x.Id == id);
                Utils.SessionManager.log("Contratistas eliminar: " + empresa.Nombre);
                database.Empresas.Remove(empresa);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
        public ActionResult Campos(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            return RedirectToAction("Index", "Campo", database.Campos.ToList());
        }
        public string formatearRut(string rut)
        {
            int cont = 0;
            string format;
            if (rut.Length == 0)
            {
                return "";
            }
            else
            {
                rut = rut.Replace(".", "");
                rut = rut.Replace("-", "");
                rut = rut.Replace(" ", "");
                format = "-" + rut.Substring(rut.Length - 1);
                for (int i = rut.Length - 2; i >= 0; i--)
                {
                    format = rut.Substring(i, 1) + format;
                    cont++;
                    if (cont == 3 && i != 0)
                    {
                        format = "." + format;
                        cont = 0;
                    }
                }
                return format;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
