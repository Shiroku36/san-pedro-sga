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
    public class SolicitudDeComprasController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: SolicitudDeCompras
        public ActionResult Index()
        {
            return View(db.SolicitudDeCompras.ToList());
        }

        // GET: SolicitudDeCompras/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SolicitudDeCompra solicitudDeCompra = db.SolicitudDeCompras.Find(id);
            if (solicitudDeCompra == null)
            {
                return HttpNotFound();
            }
            ViewBag.productos = db.Stock.Where(x => x.SolicitudDeCompraId == id).ToList();
            Utils.SessionManager.log("Detalle solicitud: " + solicitudDeCompra.Id);
            int idd = solicitudDeCompra.Id;
            ViewBag.tipo = "compra";
            ViewBag.comentarios = db.Comentario.Where(x => x.SolicitudDeCompraId == idd).ToList();
            return View(solicitudDeCompra);
        }

        // GET: SolicitudDeCompras/Create
        public ActionResult Create()
        {
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            int empresaId = (int)cuenta.EmpresaId;
            ViewBag.ingreso = true;
            ViewBag.products = db.Producto.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList();
            ViewBag.productos = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto" && x.Cantidad > 0).ToList();
            //ViewBag.productos = db.Producto.Where(x => x.EmpresaId == empresaId).ToList();
            return View();
        }

        // POST: SolicitudDeCompras/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection, [Bind(Include = "Id,Trabajador,TrabajadorId,Productos,Observación,Destino,Estado,Fecha,Empresa,EmpresaId")] SolicitudDeCompra solicitudDeCompra)
        {
            if (ModelState.IsValid)
            {
                solicitudDeCompra.Trabajador = cuenta.Nombre + " " + cuenta.Apellido;
                solicitudDeCompra.TrabajadorId = cuenta.Id;
                solicitudDeCompra.Estado = "Solicitado";
                solicitudDeCompra.Fecha = DateTime.Now;
                solicitudDeCompra.Empresa = cuenta.Empresa;
                solicitudDeCompra.EmpresaId = cuenta.EmpresaId;
                solicitudDeCompra.Productos = "";
                string[] productos = collection["Producto"].Split(new char[] { ',' });
                string[] cantidades = collection["Cantidad"].Split(new char[] { ',' });
                Utils.SessionManager.log("Crear solicitudDeCompra: " + solicitudDeCompra.Id);
                db.SolicitudDeCompras.Add(solicitudDeCompra);
                db.SaveChanges();
                string texto = "";
                for (int i = 0; i < productos.Length; i++)
                {
                    Stock stock = new Stock();
                    string nombre = productos[i];
                    Producto producto = db.Producto.First(x => x.Nombre == nombre);
                    stock.Producto = producto.Nombre;
                    stock.ProductoId = producto.Id;
                    stock.Cantidad = Convert.ToInt32(cantidades[i]);
                    stock.SolicitudDeCompraId = solicitudDeCompra.Id;
                    stock.Ubicacion = solicitudDeCompra.Destino;
                    db.Stock.Add(stock);
                    if (i == productos.Length - 1)
                    {
                        solicitudDeCompra.Productos = solicitudDeCompra.Productos + producto.Nombre;
                        texto += stock.Cantidad + " " + producto.Nombre;
                    }
                    else
                    {
                        solicitudDeCompra.Productos = solicitudDeCompra.Productos + producto.Nombre + ", ";
                        texto += stock.Cantidad + " " + producto.Nombre + ", ";
                    }
                }
                List<String> correos = db.Cuentas.Where(x => x.Empresa == cuenta.Empresa && x.Notificacion == true).Select(x => x.Email).ToList();
                Notificacion notificacion = new Notificacion()
                {
                    Fecha = DateTime.Now,
                    Correo = string.Join(", ", correos),
                    SolicitudDeCompraId = solicitudDeCompra.Id,
                    Estado = "Solicitado",
                    CuentaId = cuenta.EmpresaId,
                    Info = "Compra",
                    Texto = "El trabajador " + cuenta.Nombre + " " + cuenta.Apellido + " ha solicitado los siguientes productos para comprar: " +
                    texto + ", para: " + solicitudDeCompra.Destino + ", el día " + solicitudDeCompra.Fecha.Value.ToLongDateString() +
                    " a las " + solicitudDeCompra.Fecha.Value.ToShortTimeString()
                };
                //enviar correo
                Utils.SessionManager.log("Crear notificacion: " + notificacion.Id);
                db.Notificacion.Add(notificacion);
                db.SaveChanges();
                ViewBag.alerta = Utils.SessionManager.enviarCorreo(correos, notificacion);
                int id = cuenta.Id;
                Utils.SessionManager.notificaciones = db.Notificacion.Where(x => x.CuentaId == id && x.Estado == "Solicitado").ToList().OrderByDescending(x => x.Fecha).ToList();
                return RedirectToAction("Index");
            }
            return View(solicitudDeCompra);
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
        /*
        // GET: SolicitudDeCompras/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SolicitudDeCompra solicitudDeCompra = db.SolicitudDeCompras.Find(id);
            if (solicitudDeCompra == null)
            {
                return HttpNotFound();
            }
            return View(solicitudDeCompra);
        }

        // POST: SolicitudDeCompras/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Trabajador,TrabajadorId,Productos,Observación,Destino,Estado,Fecha,Empresa,EmpresaId")] SolicitudDeCompra solicitudDeCompra)
        {
            if (ModelState.IsValid)
            {
                db.Entry(solicitudDeCompra).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(solicitudDeCompra);
        }

        // GET: SolicitudDeCompras/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SolicitudDeCompra solicitudDeCompra = db.SolicitudDeCompras.Find(id);
            if (solicitudDeCompra == null)
            {
                return HttpNotFound();
            }
            return View(solicitudDeCompra);
        }

        // POST: SolicitudDeCompras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SolicitudDeCompra solicitudDeCompra = db.SolicitudDeCompras.Find(id);
            db.SolicitudDeCompras.Remove(solicitudDeCompra);
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
    }
}
