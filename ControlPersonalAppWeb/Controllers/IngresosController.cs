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
    public class IngresosController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: Ingresos
        public ActionResult Index()
        {
            Utils.SessionManager.log("Index ingresos");
            return View(db.Ingreso.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList());
        }

        // GET: Ingresos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingreso ingreso = db.Ingreso.Find(id);
            if (ingreso == null)
            {
                return HttpNotFound();
            }
            Utils.SessionManager.log("Detalle ingreso: " + ingreso.Id);
            ViewBag.productos = db.Stock.Where(x => x.IngresoId == id).ToList();
            return View(ingreso);
        }

        // GET: Ingresos/Create
        public ActionResult Create()
        {
            ViewBag.ingreso = true;
            ViewBag.empresas = GetNombreEmpresa((int)cuenta.EmpresaId);
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            int empresaId = (int)cuenta.EmpresaId;
            ViewBag.productos = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto").ToList();
            ViewBag.products = db.Producto.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList();
            return View();
        }

        // POST: Ingresos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection, [Bind(Include = "Id,Ubicacion,Proveedor")] Ingreso ingreso)
        {
            if (ModelState.IsValid)
            {
                ingreso.Trabajador = cuenta.Trabajador;
                ingreso.TrabajadorId = cuenta.TrabajadorId;
                ingreso.Fecha = DateTime.Now;
                string nombreEmpresa = ingreso.Proveedor;
                Empresas empresa = db.Empresas.First(x => x.Nombre == nombreEmpresa);
                ingreso.ProveedorId = empresa.Id;
                ingreso.ProveedorRut = empresa.Rut;
                ingreso.Empresa = cuenta.Empresa;
                ingreso.EmpresaId = cuenta.EmpresaId;
                ingreso.Productos = "";
                string[] productos = collection["Producto"].Split(new char[] { ',' });
                string[] cantidades = collection["Cantidad"].Split(new char[] { ',' });
                db.Ingreso.Add(ingreso);
                db.SaveChanges();
                for (int i = 0; i < productos.Length; i++)
                {
                    Stock stock = new Stock();
                    string nombre = productos[i];
                    Producto producto = db.Producto.First(x => x.Nombre == nombre);
                    stock.Producto = producto.Nombre;
                    stock.ProductoId = producto.Id;
                    stock.Cantidad = Convert.ToInt32(cantidades[i]);
                    stock.IngresoId = ingreso.Id;
                    stock.Ubicacion = ingreso.Ubicacion;
                    db.Stock.Add(stock);
                    if (i == productos.Length - 1)
                    {
                        ingreso.Productos = ingreso.Productos + producto.Nombre;
                    }
                    else
                    {
                        ingreso.Productos = ingreso.Productos + producto.Nombre + ", ";
                    }
                    agregarStock(producto,(int)stock.Cantidad, ingreso);
                }
                db.SaveChanges();
                Utils.SessionManager.log("Crear ingreso: " + ingreso.Id);
                return RedirectToAction("Index");
            }

            return View(ingreso);
        }
        /*
        // GET: Ingresos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingreso ingreso = db.Ingreso.Find(id);
            if (ingreso == null)
            {
                return HttpNotFound();
            }
            ViewBag.empresas = GetNombreEmpresa((int)cuenta.EmpresaId);
            ViewBag.campos = GetNombreCampos(cuenta.Empresa); 
            ViewBag.stocks = db.Stock.Where(x => x.IngresoId == id).ToList();
            int empresaId = (int)cuenta.EmpresaId;
            ViewBag.productos = db.Producto.Where(x => x.EmpresaId == empresaId).ToList();
            return View(ingreso);
        }

        // POST: Ingresos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection collection, [Bind(Include = "Id,Ubicacion,Proveedor")] Ingreso ingreso)
        {
            if (ModelState.IsValid)
            {
                ingreso.Trabajador = cuenta.Trabajador;
                ingreso.TrabajadorId = cuenta.TrabajadorId;
                ingreso.Fecha = DateTime.Now;
                string nombreEmpresa = ingreso.Proveedor;
                Empresas empresa = db.Empresas.First(x => x.Nombre == nombreEmpresa);
                ingreso.ProveedorId = empresa.Id;
                ingreso.ProveedorRut = empresa.Rut;
                ingreso.Empresa = cuenta.Empresa;
                ingreso.EmpresaId = cuenta.EmpresaId;
                ingreso.Productos = "";
                string[] productos = collection["Producto"].Split(new char[] { ',' });
                string[] cantidades = collection["Cantidad"].Split(new char[] { ',' });
                int idd = (int)ingreso.Id;
                List<Stock> stocks = db.Stock.Where(x => x.IngresoId == idd).ToList();
                db.Stock.RemoveRange(stocks);
                int id = (int)ingreso.Id;
                Ingreso anterior = db.Ingreso.First(x => x.Id == id);

                for (int i = 0; i < productos.Length; i++)
                {
                    Stock stock = new Stock();
                    string nombre = productos[i];
                    Producto producto = db.Producto.First(x => x.Nombre == nombre);
                    stock.Producto = producto.Nombre;
                    stock.ProductoId = producto.Id;
                    stock.Cantidad = Convert.ToInt32(cantidades[i]);
                    stock.IngresoId = ingreso.Id;
                    db.Stock.Add(stock);
                    if (i == productos.Length - 1)
                    {
                        ingreso.Productos = ingreso.Productos + producto.Nombre;
                    }
                    else
                    {
                        ingreso.Productos = ingreso.Productos + producto.Nombre + ", ";
                    }
                }
                db.Entry(ingreso).State = EntityState.Modified;
                db.SaveChanges();
                Utils.SessionManager.log("Editar ingreso: " + ingreso.Id);
                return RedirectToAction("Index");
            }
            return View(ingreso);
        }

        // GET: Ingresos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingreso ingreso = db.Ingreso.Find(id);
            if (ingreso == null)
            {
                return HttpNotFound();
            }
            return View(ingreso);
        }

        // POST: Ingresos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ingreso ingreso = db.Ingreso.Find(id);
            Utils.SessionManager.log("Eliminar ingreso: " + ingreso.Id);
            db.Ingreso.Remove(ingreso);
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

        public void agregarStock(Producto producto, int cantidad, Ingreso ingreso)
        {
            Stock total;
            try
            {
                total = db.Stock.First(x => x.Tipo == "Total" && x.EmpresaId == cuenta.EmpresaId && x.ProductoId == producto.Id);
                total.Cantidad += cantidad;
            }
            catch
            {
                total = new Stock()
                {
                    Tipo = "Total",
                    Empresa = producto.Tipo,
                    EmpresaId = cuenta.EmpresaId,
                    Producto = producto.Nombre,
                    ProductoId = producto.Id,
                    Cantidad = 0
                };
                total.Cantidad += cantidad;
                db.Stock.Add(total);
            }
            Stock articulo;
            try
            {
                articulo = db.Stock.First(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto" && x.ProductoId == producto.Id && x.Ubicacion == ingreso.Ubicacion);
                articulo.Cantidad += cantidad;
            }
            catch
            {
                articulo = new Stock()
                {
                    Tipo = "Producto",
                    Ubicacion = ingreso.Ubicacion,
                    Empresa = producto.Tipo,
                    EmpresaId = cuenta.EmpresaId,
                    Activo = true,
                    Producto = producto.Nombre,
                    ProductoId = producto.Id,
                    RetirarBajo0 = false,
                    Cantidad = 0
                };
                articulo.Cantidad += cantidad;
                db.Stock.Add(articulo);
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
        public string[] GetNombreEmpresa(int id)
        {

            string[] nombres;
            var empresas = db.Empresas.Where(x => x.InvocadaId == id).Select(x => new { x.Nombre }).ToList();
            //var empresas = db.Empresas.Select(x => new { x.Nombre }).ToList();
            nombres = new string[empresas.Count];
            int count = 0;
            foreach (var empresa in empresas)
            {
                nombres[count] = empresa.Nombre;
                count++;
            }
            return nombres;
        }
    }
}
