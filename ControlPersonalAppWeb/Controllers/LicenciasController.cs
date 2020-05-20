using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb.Models;

namespace ControlPersonalAppWeb.Controllers
{
    public class LicenciasController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: Licencias
        public ActionResult Index(int? id)
        {
            ViewBag.Remuneracion = "Licencias";
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            ViewBag.personas = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Rut = x.Rut, Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno }).ToList();
            if (id != null && id != 0)
            {
                int idd = (int)id;
                ViewBag.trabajador = db.Trabajador.First(x => x.Id == idd);
                return View(db.Licencia.Where(x => x.IdTrabajador == idd).ToList());
            }
            ViewBag.trabajador = new Trabajador { Nombre = "", Rut = "", Id = 0 };
            List<Licencia> licencias = new List<Licencia>();
            return View(licencias);
        }

        // GET: Licencias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Licencia licencia = db.Licencia.Find(id);
            if (licencia == null)
            {
                return HttpNotFound();
            }
            return View(licencia);
        }

        // GET: Licencias/Create
        public ActionResult Create(int id)
        {
            ViewBag.id = id;
            return View();
        }

        // POST: Licencias/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Desde,Dias,Hasta,Causa,IdTrabajador")] Licencia licencia)
        {
            if (ModelState.IsValid)
            {
                db.Licencia.Add(licencia);
                db.SaveChanges();
                return RedirectToAction("Index", new { id = licencia.IdTrabajador });
            }

            return View(licencia);
        }

        // GET: Licencias/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Licencia licencia = db.Licencia.Find(id);
            if (licencia == null)
            {
                return HttpNotFound();
            }
            return View(licencia);
        }

        // POST: Licencias/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Desde,Dias,Hasta,Causa,IdTrabajador")] Licencia licencia)
        {
            if (ModelState.IsValid)
            {
                db.Entry(licencia).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(licencia);
        }

        // GET: Licencias/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Licencia licencia = db.Licencia.Find(id);
            if (licencia == null)
            {
                return HttpNotFound();
            }
            return View(licencia);
        }

        // POST: Licencias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Licencia licencia = db.Licencia.Find(id);
            db.Licencia.Remove(licencia);
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
