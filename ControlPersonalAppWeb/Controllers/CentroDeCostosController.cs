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
    public class CentroDeCostosController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();

        // GET: CentroDeCostos
        public ActionResult Index(int id)
        {
            ViewBag.id = id;
            return View(db.CentroDeCostos.Where(x => x.IdEmpresa == id).ToList());
        }

        // GET: CentroDeCostos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CentroDeCostos centroDeCostos = db.CentroDeCostos.Find(id);
            if (centroDeCostos == null)
            {
                return HttpNotFound();
            }
            return View(centroDeCostos);
        }

        // GET: CentroDeCostos/Create
        public ActionResult Create(int id)
        {
            ViewBag.id = id;
            return View();
        }

        // POST: CentroDeCostos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombre,IdEmpresa")] CentroDeCostos centroDeCostos)
        {
            if (ModelState.IsValid)
            {
                db.CentroDeCostos.Add(centroDeCostos);
                db.SaveChanges();
                return RedirectToAction("Index", new { id = centroDeCostos.IdEmpresa });
            }

            return View(centroDeCostos);
        }

        // GET: CentroDeCostos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CentroDeCostos centroDeCostos = db.CentroDeCostos.Find(id);
            if (centroDeCostos == null)
            {
                return HttpNotFound();
            }
            return View(centroDeCostos);
        }

        // POST: CentroDeCostos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,IdEmpresa")] CentroDeCostos centroDeCostos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(centroDeCostos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(centroDeCostos);
        }

        // GET: CentroDeCostos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CentroDeCostos centroDeCostos = db.CentroDeCostos.Find(id);
            if (centroDeCostos == null)
            {
                return HttpNotFound();
            }
            return View(centroDeCostos);
        }

        // POST: CentroDeCostos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CentroDeCostos centroDeCostos = db.CentroDeCostos.Find(id);
            db.CentroDeCostos.Remove(centroDeCostos);
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
