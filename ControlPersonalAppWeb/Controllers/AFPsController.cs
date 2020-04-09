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
    public class AFPsController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();

        // GET: AFPs
        public ActionResult Index(int? id)
        {
            ViewBag.Remuneracion = "AFPs";
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            ViewBag.personas = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Rut = x.Rut, Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno }).ToList();
            if (id != null && id != 0)
            {
                int idd = (int)id;
                ViewBag.trabajador = db.Trabajador.First(x => x.Id == idd);
                return View(db.AFP.ToList());
            }
            ViewBag.trabajador = new Trabajador { Nombre = "", Rut = "", Id = 0 };
            return View(db.AFP.ToList());
        }

        // GET: AFPs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AFP aFP = db.AFP.Find(id);
            if (aFP == null)
            {
                return HttpNotFound();
            }
            return View(aFP);
        }

        // GET: AFPs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AFPs/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombre,Porcentaje,Periodo,Retiro,Voluntario")] AFP aFP)
        {
            if (ModelState.IsValid)
            {
                db.AFP.Add(aFP);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(aFP);
        }

        // GET: AFPs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AFP aFP = db.AFP.Find(id);
            if (aFP == null)
            {
                return HttpNotFound();
            }
            return View(aFP);
        }

        // POST: AFPs/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,Porcentaje,Periodo,Retiro,Voluntario")] AFP aFP)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aFP).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aFP);
        }

        // GET: AFPs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AFP aFP = db.AFP.Find(id);
            if (aFP == null)
            {
                return HttpNotFound();
            }
            return View(aFP);
        }

        // POST: AFPs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AFP aFP = db.AFP.Find(id);
            db.AFP.Remove(aFP);
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
