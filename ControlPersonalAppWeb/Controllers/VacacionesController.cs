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
    public class VacacionesController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: Vacaciones
        public ActionResult Index(int? id)
        {
            ViewBag.Remuneracion = "Vacaciones";
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            ViewBag.personas = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Rut = x.Rut, Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno }).ToList();
            if (id != null && id != 0)
            {
                int idd = (int)id;
                ViewBag.trabajador = db.Trabajador.First(x => x.Id == idd);
                return View(db.Vacaciones.Where(x => x.IdTrabajador == idd).ToList());
            }
            ViewBag.trabajador = new Trabajador { Nombre = "", Rut = "", Id = 0 };
            List<Vacaciones> vacacion = new List<Vacaciones>();
            return View(vacacion);
        }

        // GET: Vacaciones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vacaciones vacaciones = db.Vacaciones.Find(id);
            if (vacaciones == null)
            {
                return HttpNotFound();
            }
            return View(vacaciones);
        }

        // GET: Vacaciones/Create
        public ActionResult Create(int id)
        {
            ViewBag.id = id;
            return View();
        }

        // POST: Vacaciones/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Año,Normales,Progresivas,Total,TomadasNormales,TomadasProgresivas,Saldo,IdTrabajador")] Vacaciones vacaciones)
        {
            if (ModelState.IsValid)
            {
                db.Vacaciones.Add(vacaciones);
                db.SaveChanges();
                return RedirectToAction("Index", new { id = vacaciones.IdTrabajador });
            }

            return View(vacaciones);
        }

        // GET: Vacaciones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vacaciones vacaciones = db.Vacaciones.Find(id);
            if (vacaciones == null)
            {
                return HttpNotFound();
            }
            return View(vacaciones);
        }

        // POST: Vacaciones/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Año,Normales,Progresivas,Total,TomadasNormales,TomadasProgresivas,Saldo,IdTrabajador")] Vacaciones vacaciones)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vacaciones).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vacaciones);
        }

        // GET: Vacaciones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vacaciones vacaciones = db.Vacaciones.Find(id);
            if (vacaciones == null)
            {
                return HttpNotFound();
            }
            return View(vacaciones);
        }

        // POST: Vacaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vacaciones vacaciones = db.Vacaciones.Find(id);
            db.Vacaciones.Remove(vacaciones);
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
