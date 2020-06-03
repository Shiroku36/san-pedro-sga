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
    public class SolicitudesController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: Solicitudes
        public ActionResult Index()
        {
            Utils.SessionManager.log("Index solicitudes");
            return View(db.Solicitud.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList());
        }

        // GET: Solicitudes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Solicitud solicitud = db.Solicitud.Find(id);
            if (solicitud == null)
            {
                return HttpNotFound();
            }
            ViewBag.productos = db.Stock.Where(x => x.SolicitudId == id).ToList();
            Utils.SessionManager.log("Detalle solicitud: " + solicitud.Id);
            return View(solicitud);
        }

        // GET: Solicitudes/Create
        public ActionResult Create()
        {
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            int empresaId =(int) cuenta.EmpresaId;
            ViewBag.productos = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto").ToList();
            //ViewBag.productos = db.Producto.Where(x => x.EmpresaId == empresaId).ToList();
            return View();
        }

        // POST: Solicitudes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection,[Bind(Include = "Id,Productos,Observación,Origen,Destino")] Solicitud solicitud)
        {
            if (ModelState.IsValid)
            {
                solicitud.Trabajador = cuenta.Trabajador;
                solicitud.TrabajadorId = cuenta.TrabajadorId;
                solicitud.Estado = "Solicitado";
                solicitud.Fecha = DateTime.Now ;
                solicitud.Empresa = cuenta.Empresa;
                solicitud.EmpresaId = cuenta.EmpresaId;
                solicitud.Productos = "";
                string[] productos = collection["Producto"].Split(new char[] { ',' });
                string[] cantidades = collection["Cantidad"].Split(new char[] { ',' });
                Utils.SessionManager.log("Crear solicitud: " + solicitud.Id);
                db.Solicitud.Add(solicitud);
                db.SaveChanges();
                string texto = "";
                for (int i = 0; i <productos.Length; i++)
                {
                    Stock stock = new Stock();
                    string nombre = productos[i];
                    Producto producto = db.Producto.First(x => x.Nombre == nombre );
                    stock.Producto = producto.Nombre;
                    stock.ProductoId = producto.Id;
                    stock.Cantidad = Convert.ToInt32(cantidades[i]);
                    stock.SolicitudId = solicitud.Id;
                    stock.Ubicacion = solicitud.Origen;
                    db.Stock.Add(stock);
                    if (i == productos.Length - 1)
                    {
                        solicitud.Productos = solicitud.Productos + producto.Nombre;
                        texto += stock.Cantidad + " " + producto.Nombre;
                    }
                    else
                    {
                        solicitud.Productos = solicitud.Productos + producto.Nombre + ", ";
                        texto += stock.Cantidad +" "+ producto.Nombre + ", ";
                    }
                }
                Notificacion notificacion = new Notificacion()
                {
                    Fecha = DateTime.Now,
                    Correo = db.Trabajador.First(x => x.Id == cuenta.TrabajadorId).Email,
                    SolicitudId = solicitud.Id,
                    Estado = "Solicitado",
                    CuentaId = cuenta.Id,
                    Texto = "El trabajador " + cuenta.Trabajador + " ha solicitado los siguientes productos: " +
                    texto + ", desde: " + solicitud.Origen + " a " + solicitud.Destino + ", el día " + solicitud.Fecha.Value.ToLongDateString() +
                    " a las " + solicitud.Fecha.Value.ToShortTimeString()
                };
                Utils.SessionManager.log("Crear notificacion: " + notificacion.Id);
                db.Notificacion.Add(notificacion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(solicitud);
        }
        /*
        // GET: Solicitudes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Solicitud solicitud = db.Solicitud.Find(id);
            if (solicitud == null)
            {
                return HttpNotFound();
            }
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            ViewBag.stocks = db.Stock.Where(x => x.SolicitudId == id).ToList();
            int empresaId = (int)cuenta.EmpresaId;
            ViewBag.productos = db.Producto.Where(x => x.EmpresaId == empresaId).ToList();
            return View(solicitud);
        }

        // POST: Solicitudes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection collection, [Bind(Include = "Id,Productos,Observación,Origen,Destino")] Solicitud solicitud)
        {
            if (ModelState.IsValid)
            {
                db.Entry(solicitud).State = EntityState.Modified;
                solicitud.Trabajador = cuenta.Trabajador;
                solicitud.TrabajadorId = cuenta.TrabajadorId;
                solicitud.Estado = "Solicitado";
                solicitud.Fecha = DateTime.Now;
                solicitud.Empresa = cuenta.Empresa;
                solicitud.EmpresaId = cuenta.EmpresaId;
                solicitud.Productos = "";
                string[] productos = collection["Producto"].Split(new char[] { ',' });
                string[] cantidades = collection["Cantidad"].Split(new char[] { ',' });
                int idd = (int)solicitud.Id;
                List<Stock> stocks = db.Stock.Where(x => x.SolicitudId == idd).ToList();
                db.Stock.RemoveRange(stocks);
                for (int i = 0; i < productos.Length; i++)
                {
                    Stock stock = new Stock();
                    string nombre = productos[i];
                    Producto producto = db.Producto.First(x => x.Nombre == nombre);
                    stock.Producto = producto.Nombre;
                    stock.ProductoId = producto.Id;
                    stock.Cantidad = Convert.ToInt32(cantidades[i]);
                    stock.SolicitudId = solicitud.Id;
                    db.Stock.Add(stock);
                    if (i == productos.Length - 1)
                    {
                        solicitud.Productos = solicitud.Productos + producto.Nombre;
                    }
                    else
                    {
                        solicitud.Productos = solicitud.Productos + producto.Nombre + ", ";
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            int id = solicitud.Id;
            return View(solicitud);
        }

        // GET: Solicitudes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Solicitud solicitud = db.Solicitud.Find(id);
            if (solicitud == null)
            {
                return HttpNotFound();
            }
            return View(solicitud);
        }

        // POST: Solicitudes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Solicitud solicitud = db.Solicitud.Find(id);
            List<Stock> stocks = db.Stock.Where(x => x.SolicitudId == id).ToList();
            foreach (var stock in stocks)
            {
                Stock total = db.Stock.First(x => x.EmpresaId == cuenta.Id && x.Tipo == "Total" && x.ProductoId == stock.ProductoId);
                total.Cantidad -= stock.Cantidad;
                Stock articulo = db.Stock.First(x => x.EmpresaId == cuenta.Id && x.Tipo == "Total" && x.ProductoId == stock.ProductoId && x.Ubicacion == stock.Ubicacion);
                articulo.Cantidad -= stock.Cantidad;
            }
            db.Notificacion.Remove(db.Notificacion.First(x => x.SolicitudId == id));
            db.Solicitud.Remove(solicitud);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public string[] GetNombreCampos(string empresa)
        {

            string[] nombres;
            var campos = db.Campos.Select(x => new { x.Nombre }).ToList();
            if (!String.IsNullOrEmpty(empresa))
            {
                campos = db.Campos.Where(x => x.Empresa == empresa).Select(x => new { x.Nombre }).ToList();
            }
            nombres = new string[campos.Count];
            int count = 0;
            foreach (var campo in campos)
            {
                nombres[count] = campo.Nombre;
                count++;
            }
            return nombres;
        }
    }
}
