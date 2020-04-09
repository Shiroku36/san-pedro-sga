using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb;

namespace ControlPersonalAppWeb.Controllers
{
    public class EmpresasController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();

        // GET: Empresas
        public ActionResult Index()
        {
            return View(db.Empresas.ToList());
        }

        // GET: Empresas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Empresas empresas = db.Empresas.Find(id);
            if (empresas == null)
            {
                return HttpNotFound();
            }
            return View(empresas);
        }

        // GET: Empresas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Empresas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombre,Rut,RazonSocial,RepresentanteLegalNombre,RepresenntanteLegalRut,Giro,Dirección,Comuna,Ciudad,Telefono,Mutual,FactorMutual,CajaDeCompensacion,TextoLiquidacion,Logo")] Empresas empresas)
        {
            if (ModelState.IsValid)
            {
                db.Empresas.Add(empresas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(empresas);
        }

        // GET: Empresas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Empresas empresas = db.Empresas.Find(id);
            if (empresas == null)
            {
                return HttpNotFound();
            }
            ViewBag.nombre = empresas.Nombre;
            return View(empresas);
        }

        // POST: Empresas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,NombreAnterior,Rut,RazonSocial,RepresentanteLegalNombre,RepresenntanteLegalRut,Giro,Dirección,Comuna,Ciudad,Telefono,Mutual,FactorMutual,CajaDeCompensacion,TextoLiquidacion,Logo")] Empresas empresas)
        {
            if (ModelState.IsValid)
            {

                db.Entry(empresas).State = EntityState.Modified;
                string nombre = empresas.NombreAnterior;
                if(nombre != empresas.Nombre)
                {

                    List<Trabajador> trabajadores = db.Trabajador.Where(x => x.Empresa == nombre).ToList();
                    foreach(var x in trabajadores)
                    {
                        x.Empresa = empresas.Nombre;
                    }
                    List<Campos> campos = db.Campos.Where(x => x.Empresa == nombre).ToList();
                    foreach (var x in campos)
                    {
                        x.Empresa = empresas.Nombre;
                    }
                    List<Cuentas> cuentas = db.Cuentas.Where(x => x.Empresa == nombre).ToList();
                    foreach (var x in cuentas)
                    {
                        x.Empresa = empresas.Nombre;
                    }
                    List<RegistroTrabajador> registros = db.RegistroTrabajador.Where(x => x.Empresa == nombre).ToList();
                    foreach (var x in registros)
                    {
                        x.Empresa = empresas.Nombre;
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(empresas);
        }

        // GET: Empresas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Empresas empresas = db.Empresas.Find(id);
            if (empresas == null)
            {
                return HttpNotFound();
            }
            return View(empresas);
        }

        // POST: Empresas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Empresas empresas = db.Empresas.Find(id);
            db.Empresas.Remove(empresas);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
