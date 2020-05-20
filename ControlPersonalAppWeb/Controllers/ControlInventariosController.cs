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
    public class ControlInventariosController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: ControlInventarios
        public ActionResult Index()
        {
            return View(db.ControlInventario.ToList());
        }

        // GET: ControlInventarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ControlInventario controlInventario = db.ControlInventario.Find(id);
            if (controlInventario == null)
            {
                return HttpNotFound();
            }
            return View(controlInventario);
        }

        // GET: ControlInventarios/Create
        public ActionResult Create()
        {
            string empresa = cuenta.Empresa;
            List<Producto> productos = db.Producto.Where(x => x.Empresa == empresa && x.Activo == true).ToList();
            List<String> nombres = new List<string>();
            foreach (var producto in productos)
            {
                nombres.Add(producto.Nombre);
            }
            nombres.Add("Arreglar");
            ViewBag.productos = nombres;
            return View();
        }

        // POST: ControlInventarios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Producto,ProductoId,Cantidad,Razon,Fecha,Trabajador,TrabajadorId,Empresa,EmpresaId")] ControlInventario controlInventario)
        {
            if (ModelState.IsValid)
            {
                db.ControlInventario.Add(controlInventario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(controlInventario);
        }

        // GET: ControlInventarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ControlInventario controlInventario = db.ControlInventario.Find(id);
            if (controlInventario == null)
            {
                return HttpNotFound();
            }
            return View(controlInventario);
        }

        // POST: ControlInventarios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Producto,ProductoId,Cantidad,Razon,Fecha,Trabajador,TrabajadorId,Empresa,EmpresaId")] ControlInventario controlInventario)
        {
            if (ModelState.IsValid)
            {
                db.Entry(controlInventario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(controlInventario);
        }

        // GET: ControlInventarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ControlInventario controlInventario = db.ControlInventario.Find(id);
            if (controlInventario == null)
            {
                return HttpNotFound();
            }
            return View(controlInventario);
        }

        // POST: ControlInventarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ControlInventario controlInventario = db.ControlInventario.Find(id);
            db.ControlInventario.Remove(controlInventario);
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
