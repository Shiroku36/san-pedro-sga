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
    public class HorasExtrasController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: HorasExtras
        public ActionResult Index(int? id)
        {
            ViewBag.Remuneracion = "HorasExtras";
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            ViewBag.personas = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Rut = x.Rut, Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno }).ToList();
            if (id != null && id != 0)
            {
                int idd = (int)id;
                ViewBag.trabajador = db.Trabajador.First(x => x.Id == idd);
                return View(db.HorasExtras.Where(x => x.IdTrabajador == idd).ToList());
            }
            ViewBag.trabajador = new Trabajador { Nombre = "", Rut = "", Id = 0 };
            List<HorasExtras> horas = new List<HorasExtras>();
            return View(horas);
        }

        // GET: HorasExtras/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HorasExtras horasExtras = db.HorasExtras.Find(id);
            if (horasExtras == null)
            {
                return HttpNotFound();
            }
            return View(horasExtras);
        }

        // GET: HorasExtras/Create
        public ActionResult Create(int id)
        {
            ViewBag.id = id;
            return View();
        }

        // POST: HorasExtras/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Horas,Porcentaje,Periodo,IdTrabajador")] HorasExtras horasExtras)
        {
            if (ModelState.IsValid)
            {
                db.HorasExtras.Add(horasExtras);
                db.SaveChanges();
                return RedirectToAction("Index", new { id = horasExtras.IdTrabajador });
            }

            return View(horasExtras);
        }

        // GET: HorasExtras/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HorasExtras horasExtras = db.HorasExtras.Find(id);
            if (horasExtras == null)
            {
                return HttpNotFound();
            }
            return View(horasExtras);
        }

        // POST: HorasExtras/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Horas,Porcentaje,Periodo,IdTrabajador")] HorasExtras horasExtras)
        {
            if (ModelState.IsValid)
            {
                db.Entry(horasExtras).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(horasExtras);
        }

        // GET: HorasExtras/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HorasExtras horasExtras = db.HorasExtras.Find(id);
            if (horasExtras == null)
            {
                return HttpNotFound();
            }
            return View(horasExtras);
        }

        // POST: HorasExtras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            HorasExtras horasExtras = db.HorasExtras.Find(id);
            db.HorasExtras.Remove(horasExtras);
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
