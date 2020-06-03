using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControlPersonalAppWeb.Controllers
{
    public class EmpresaController : Controller
    {
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();
        // GET: Empresas
        public ActionResult Index()
        {
            Utils.SessionManager.log("Index empresas");
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            int id = (int)cuenta.EmpresaId;
            if (cuenta.EmpresaId == 1)
            {
                return View(database.Empresas.ToList());
            }
            return View(database.Empresas.Where(x => x.InvocadaId == cuenta.EmpresaId).ToList());
        }

        // GET: Empresas/Details/5
        public ActionResult Details(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Empresas empresas = database.Empresas.First(x => x.Id == id);
            Utils.SessionManager.log("Detalle empresa: " + empresas.Nombre);
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
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                Empresas empresa = new Empresas();
                empresa.Nombre = collection["Nombre"];  
                empresa.Rut = formatearRut(collection["Rut"]);  
                empresa.Invocada = cuenta.Empresa;  
                empresa.InvocadaId = cuenta.EmpresaId;  
                database.Empresas.Add(empresa);
                database.SaveChanges();
                Utils.SessionManager.log("Empresa creada: " + empresa.Nombre);
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
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Empresas empresa = database.Empresas.First(x => x.Id == id);
            return View(empresa);
        }

        // POST: Empresas/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                Empresas empresa = database.Empresas.First(x => x.Id == id);
                Utils.SessionManager.log("Empresa editada: " + empresa.Nombre);
                empresa.Nombre = collection["Nombre"];
                empresa.Rut = collection["Rut"];
                empresa.Invocada = cuenta.Empresa;
                empresa.InvocadaId = cuenta.EmpresaId;      
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
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Empresas empresa = database.Empresas.First(x => x.Id == id);
            database.Empresas.Remove(empresa);
            database.SaveChanges();
            Utils.SessionManager.log("Empresa eliminada: " + empresa.Nombre);
            return RedirectToAction("Index");
        }

        // POST: Empresas/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult Campos(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
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
    }
}
