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
{/*
    public class NotificacionesController : Controller
    {
        private SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: Notificaciones
        public ActionResult Index()
        {
            return View(db.Notificacion.Where(x => x.CuentaId == cuenta.EmpresaId).ToList().OrderByDescending(x => x.Fecha).ToList());
        }
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            int id = Convert.ToInt32(collection["Id"]);
            Notificacion notificacion = db.Notificacion.First(x => x.Id == id);
            if (notificacion.Info=="Producto")
            {
                Solicitud solicitud = db.Solicitud.First(x => x.Id == notificacion.SolicitudId);
                List<Stock> stocks = db.Stock.Where(x => x.SolicitudId == notificacion.SolicitudId).ToList();
                if (collection["Accion"] == "Aceptar")
                {
                    foreach (var stock in stocks)
                    {
                        Producto producto = db.Producto.First(x => x.Id == stock.ProductoId);
                        agregarStock(producto, (int)stock.Cantidad, solicitud);
                    }
                    solicitud.Estado = "Aceptada";
                    notificacion.Estado = "Aceptada";
                    Utils.SessionManager.log("Notificación aceptada: " + id);
                }
                else
                {
                    solicitud.Estado = "Rechazada";
                    notificacion.Estado = "Rechazada";
                    Utils.SessionManager.log("Notificación rechazada: " + id);
                }
            }
            else
            {
                SolicitudDeCompra solicitudDeCompra = db.SolicitudDeCompra.First(x => x.Id == notificacion.SolicitudDeCompraId);
                if (collection["Accion"] == "Aceptar")
                {
                    solicitudDeCompra.Estado = "Aceptada";
                    notificacion.Estado = "Aceptada";
                    Utils.SessionManager.log("Notificación aceptada: " + id);
                }
                else
                {
                    solicitudDeCompra.Estado = "Rechazada";
                    notificacion.Estado = "Rechazada";
                    Utils.SessionManager.log("Notificación rechazada: " + id);
                }
            }
            db.SaveChanges();
            Utils.SessionManager.notificaciones.Remove(Utils.SessionManager.notificaciones.First(x => x.Id == id));
            return View(db.Notificacion.Where(x => x.CuentaId == cuenta.Id).ToList().OrderByDescending(x => x.Fecha).ToList());
        }
        /*
        // GET: Notificaciones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Notificacion notificacion = db.Notificacion.Find(id);
            if (notificacion == null)
            {
                return HttpNotFound();
            }
            return View(notificacion);
        }

        // GET: Notificaciones/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Notificaciones/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Fecha,Correo,SolicitudId,Estado,Texto,CuentaId,Dato,Info")] Notificacion notificacion)
        {
            if (ModelState.IsValid)
            {
                db.Notificacion.Add(notificacion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(notificacion);
        }

        // GET: Notificaciones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Notificacion notificacion = db.Notificacion.Find(id);
            if (notificacion == null)
            {
                return HttpNotFound();
            }
            return View(notificacion);
        }

        // POST: Notificaciones/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Fecha,Correo,SolicitudId,Estado,Texto,CuentaId,Dato,Info")] Notificacion notificacion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(notificacion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(notificacion);
        }

        // GET: Notificaciones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Notificacion notificacion = db.Notificacion.Find(id);
            if (notificacion == null)
            {
                return HttpNotFound();
            }
            return View(notificacion);
        }

        // POST: Notificaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Notificacion notificacion = db.Notificacion.Find(id);
            db.Notificacion.Remove(notificacion);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        */ /*
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public void agregarStock(Producto producto, int cantidad, Solicitud ingreso)
        {
            Stock articuloOrigen;
            Stock articuloDestino;
            articuloOrigen = db.Stock.First(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto" && x.ProductoId == producto.Id && x.Ubicacion == ingreso.Origen);
            articuloOrigen.Cantidad -= cantidad;
            try
            {
                articuloDestino = db.Stock.First(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto" && x.ProductoId == producto.Id && x.Ubicacion == ingreso.Destino);
                articuloDestino.Cantidad += cantidad;
            }
            catch
            {
                articuloDestino = new Stock()
                {
                    Tipo = "Producto",
                    Ubicacion = ingreso.Destino,
                    Empresa = producto.Tipo,
                    EmpresaId = cuenta.EmpresaId,
                    Activo = true,
                    Producto = producto.Nombre,
                    ProductoId = producto.Id,
                    RetirarBajo0 = false,
                    Cantidad = 0
                };
                articuloDestino.Cantidad += cantidad;
                db.Stock.Add(articuloDestino);
            }
        }
    }*/
}
