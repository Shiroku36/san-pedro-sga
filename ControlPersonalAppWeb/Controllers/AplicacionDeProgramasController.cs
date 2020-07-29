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
    public class AplicacionDeProgramasController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: AplicacionDeProgramas
        public ActionResult Index(int? id)
        {
            Utils.SessionManager.log("Index aplicaciones de programa");
            if (id != null)
            {
                return View(db.AplicacionDePrograma.Where(x => x.SolicitudId == id));
            }
            return View(db.AplicacionDePrograma.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList().OrderByDescending(x => x.FechaInicio).ToList());
        }

        // GET: AplicacionDeProgramas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AplicacionDePrograma aplicacionDePrograma = db.AplicacionDePrograma.Find(id);
            if (aplicacionDePrograma == null)
            {
                return HttpNotFound();
            }
            Utils.SessionManager.log("Detalle aplicacion de programa: " + aplicacionDePrograma.Id);
            int idd = aplicacionDePrograma.Id;
            ViewBag.comentarios = db.Comentario.Where(x => x.AplicacionDeProgramaId == idd).ToList();
            return View(aplicacionDePrograma);
        }

        // GET: AplicacionDeProgramas/Create
        public ActionResult Create(int id)
        {
            ViewBag.solicitudId = id;
            return View();
        }

        // POST: AplicacionDeProgramas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SolicitudId,FechaInicio,FechaFin")] AplicacionDePrograma aplicacionDePrograma)
        {
            if (ModelState.IsValid)
            {
                aplicacionDePrograma.Empresa = cuenta.Empresa;
                aplicacionDePrograma.EmpresaId = cuenta.EmpresaId;
                db.AplicacionDePrograma.Add(aplicacionDePrograma);
                db.SaveChanges();
                Utils.SessionManager.log("Crear aplicacion de programa: "+ aplicacionDePrograma.Id );
                return RedirectToAction("Index");
            }

            return View(aplicacionDePrograma);
        }
        
        // GET: AplicacionDeProgramas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AplicacionDePrograma aplicacionDePrograma = db.AplicacionDePrograma.Find(id);
            if (aplicacionDePrograma == null)
            {
                return HttpNotFound();
            }
            return View(aplicacionDePrograma);
        }

        // POST: AplicacionDeProgramas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SolicitudId,FechaInicio,FechaFin,Empresa,EmpresaId")] AplicacionDePrograma aplicacionDePrograma)
        {
            if (ModelState.IsValid)
            {
                aplicacionDePrograma.Empresa = cuenta.Empresa;
                aplicacionDePrograma.EmpresaId = cuenta.EmpresaId;
                db.Entry(aplicacionDePrograma).State = EntityState.Modified;
                db.SaveChanges();
                Utils.SessionManager.log("Editar aplicacion de programa: " + aplicacionDePrograma.Id);
                return RedirectToAction("Index");
            }
            return View(aplicacionDePrograma);
        }
        /*
        // GET: AplicacionDeProgramas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AplicacionDePrograma aplicacionDePrograma = db.AplicacionDePrograma.Find(id);
            if (aplicacionDePrograma == null)
            {
                return HttpNotFound();
            }
            return View(aplicacionDePrograma);
        }

        // POST: AplicacionDeProgramas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AplicacionDePrograma aplicacionDePrograma = db.AplicacionDePrograma.Find(id);
            db.AplicacionDePrograma.Remove(aplicacionDePrograma);
            db.SaveChanges();
            return RedirectToAction("Index");
        }*/
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
