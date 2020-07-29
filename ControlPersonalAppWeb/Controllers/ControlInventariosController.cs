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
            Utils.SessionManager.log("Index control inventario");
            return View(db.ControlInventario.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList());
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
            ViewBag.productos = db.Stock.Where(x => x.ControlId == id).ToList();
            Utils.SessionManager.log("Detalle control inventario: " + controlInventario.Id);
            return View(controlInventario);
        }

        // GET: ControlInventarios/Create
        public ActionResult Create()
        {
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            int empresaId = (int)cuenta.EmpresaId;
            ViewBag.productos = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto" && x.Cantidad > 0).ToList();
            return View();
        }

        // POST: ControlInventarios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection, [Bind(Include = "Id,Ubicacion,Razon")] ControlInventario controlInventario)
        {
            if (ModelState.IsValid)
            {
                controlInventario.Trabajador = cuenta.Nombre + " " + cuenta.Apellido;
                controlInventario.TrabajadorId = cuenta.Id;
                controlInventario.Fecha = DateTime.Now;
                controlInventario.Empresa = cuenta.Empresa;
                controlInventario.EmpresaId = cuenta.EmpresaId;
                controlInventario.Productos = "";
                string[] productos = collection["Producto"].Split(new char[] { ',' });
                string[] cantidades = collection["Cantidad"].Split(new char[] { ',' });
                db.ControlInventario.Add(controlInventario);
                db.SaveChanges();
                for (int i = 0; i < productos.Length; i++)
                {
                    Stock stock = new Stock();
                    string nombre = productos[i];
                    Producto producto = db.Producto.First(x => x.Nombre == nombre);
                    stock.Producto = producto.Nombre;
                    stock.ProductoId = producto.Id;
                    stock.Cantidad = Convert.ToInt32(cantidades[i]);
                    stock.ControlId = controlInventario.Id;
                    stock.Ubicacion = collection["Ubicacion"];
                    db.Stock.Add(stock);
                    if (i == productos.Length - 1)
                    {
                        controlInventario.Productos = controlInventario.Productos + producto.Nombre;
                    }
                    else
                    {
                        controlInventario.Productos = controlInventario.Productos + producto.Nombre + ", ";
                    }
                    agregarStock(producto, Convert.ToInt32(cantidades[i]), controlInventario);
                }
                db.SaveChanges();
                Utils.SessionManager.log("Crear control inventario: " + controlInventario.Id);
                return RedirectToAction("Index");
            }

            return View(controlInventario);
        }
        /*
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
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            List<Stock> stocks = db.Stock.Where(x => x.SolicitudId == id).ToList();
            ViewBag.ubicacion = stocks.ElementAt(0).Ubicacion;
            ViewBag.stocks = db.Stock.Where(x => x.SolicitudId == id).ToList();
            int empresaId = (int)cuenta.EmpresaId;
            ViewBag.productos = db.Producto.Where(x => x.EmpresaId == empresaId).ToList();
            return View(controlInventario);
        }

        // POST: ControlInventarios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection collection, [Bind(Include = "Id,Razon")] ControlInventario controlInventario)
        {
            if (ModelState.IsValid)
            {
                db.Entry(controlInventario).State = EntityState.Modified;
                controlInventario.Trabajador = cuenta.Trabajador;
                controlInventario.TrabajadorId = cuenta.TrabajadorId;
                controlInventario.Fecha = DateTime.Now;
                controlInventario.Empresa = cuenta.Empresa;
                controlInventario.EmpresaId = cuenta.EmpresaId;
                controlInventario.Productos = "";
                string[] productos = collection["Producto"].Split(new char[] { ',' });
                string[] cantidades = collection["Cantidad"].Split(new char[] { ',' });
                int idd = (int)controlInventario.Id;
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
                    stock.ControlId = controlInventario.Id;
                    stock.Ubicacion = controlInventario.Ubicacion;
                    db.Stock.Add(stock);
                    if (i == productos.Length - 1)
                    {
                        controlInventario.Productos = controlInventario.Productos + producto.Nombre;
                    }
                    else
                    {
                        controlInventario.Productos = controlInventario.Productos + producto.Nombre + ", ";
                    }
                }
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
        */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public void agregarStock(Producto producto, int cantidad, ControlInventario ingreso)
        {
            Stock total;
            try
            {
                total = db.Stock.First(x => x.Tipo == "Total" && x.EmpresaId == cuenta.EmpresaId && x.ProductoId == producto.Id);
                total.Cantidad -= cantidad;
            }
            catch
            {
            }
            Stock articulo;
            try
            {
                articulo = db.Stock.First(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto" && x.ProductoId == producto.Id && x.Ubicacion == ingreso.Ubicacion);
                articulo.Cantidad -= cantidad;
            }
            catch
            {
            }
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
