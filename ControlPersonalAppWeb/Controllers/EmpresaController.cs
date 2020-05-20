using System;
using System.Collections.Generic;
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
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            return View(database.Empresas.ToList());
        }

        // GET: Empresas/Details/5
        public ActionResult Details(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Empresas empresas = database.Empresas.First(x => x.Id == id);
            return View(empresas);
        }

        // GET: Empresas/Create
        public ActionResult Create()
        {
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
                database.Empresas.Add(empresa);
                database.SaveChanges();
                Utils.SessionManager.log("Empresa creada : " + empresa.Nombre);
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
                Empresas empresa = new Empresas();
                Utils.SessionManager.log("Empresa editada : " + empresa.Nombre);
                empresa.Nombre = collection["Nombre"];
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Empresas/Delete/5
        public ActionResult Delete(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Empresas empresa = database.Empresas.First(x => x.Id == id);
            database.Empresas.Remove(empresa);
            database.SaveChanges();
            Utils.SessionManager.log("Empresa eliminada : " + empresa.Nombre);
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
    }
}
