using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb;

namespace ControlPersonalAppWeb.Controllers
{
    public class StockController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: Stock 
        public ActionResult Tipos()
        {
            Utils.SessionManager.log("Index tipos");
            return View(cargarDatos("Tipos"));
        }
        [HttpPost]
        public ActionResult Tipos(FormCollection collection)
        {
            if (String.IsNullOrEmpty(collection["Llave"]))
            {
                if (!String.IsNullOrEmpty(collection["Id"]))
                {
                    int id = Convert.ToInt32(collection["Id"]);
                    string tipo = collection["Value"];
                    db.Datos.First(x => x.Id == id).Campo += "," + collection["Value"];
                }
                else
                {
                    Datos datos = new Datos()
                    {
                        Campo = collection["Value"],
                        EmpresaId = cuenta.EmpresaId,
                        Valor = collection["Tipo"]
                    };
                    db.Datos.Add(datos);
                }
            }
            else
            {
                int id = Convert.ToInt32(collection["Id"]);
                string[] tipos = db.Datos.First(x => x.Id == id).Campo.Split(new char[] { ',' });
                List<string> types = tipos.ToList();
                int idd = Convert.ToInt32(collection["Llave"]);
                types.Remove(tipos[idd]);
                string type = "";
                int i = 0;
                foreach (var tipo in types)
                {
                    if(!String.IsNullOrEmpty(tipo))
                    {
                        if (type.Length == 0)
                        {
                            type = tipo;
                        }
                        else
                        {
                            type += "," + tipo;
                        }
                    }
                    i++;
                }
                db.Datos.First(x => x.Id == id).Campo = type;
            }
            db.SaveChanges();
            if(collection["Tipo"]=="Unidades")
            {
                Utils.SessionManager.log("Crear Unidad: " + collection["Value"]);
                return RedirectToAction("Unidades", cargarDatos(collection["Tipo"]));
            }
            Utils.SessionManager.log("Crear tipo: " + collection["Value"]);
            return View(cargarDatos(collection["Tipo"]));
        }
        public ActionResult Unidades()
        {
                Utils.SessionManager.log("Index Unidades");
                return View(cargarDatos("Unidades"));
        }
        public ActionResult Index()
        {
            Utils.SessionManager.log("Index inventario");
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            string datos = "";
            try
            {
                datos = db.Datos.First(x => x.EmpresaId == cuenta.EmpresaId && x.Valor == "Tipos").Campo;
            }
            catch
            {

            }
            if(!String.IsNullOrEmpty(datos))
            {
                while (datos[0] == ',')
                {
                    datos = datos.Substring(1, datos.Length - 1);
                }
                datos = "Todos," + datos;
            }
            else
            {
                datos = "Todos";
            }
            ViewBag.tipos =datos.Split(new char[] { ',' }).ToList();
            return View(db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Total" ).ToList());
        }
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            string datos = "";
            try
            {
                datos = db.Datos.First(x => x.EmpresaId == cuenta.EmpresaId && x.Valor == "Tipos").Campo;
            }
            catch
            {

            }
            if (!String.IsNullOrEmpty(datos))
            {
                while (datos[0] == ',')
                {
                    datos = datos.Substring(1, datos.Length - 1);
                }
                datos = "Todos," + datos;
            }
            else
            {
                datos = "Todos";
            }
            ViewBag.tipos = datos.Split(new char[] { ',' }).ToList();
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            string huerto = collection["Huerto"];
            string tipo = collection["tipo"];
            ViewBag.campo = huerto;
            ViewBag.tipo = tipo;
            List<Stock> stocks;
            if (huerto == "Todos" && tipo == "Todos")
            {
                stocks = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Total").ToList();
            }
            else if(huerto == "Todos" && tipo != "Todos")
            {
                stocks = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Total" && x.Empresa == tipo ).ToList();
            }
            else if(huerto != "Todos" && tipo == "Todos")
            {
                stocks = db.Stock.Where(x => x.Ubicacion == huerto && x.EmpresaId == cuenta.EmpresaId).ToList();
            }
            else
            {
                stocks = db.Stock.Where(x => x.Ubicacion == huerto && x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto" && x.Empresa == tipo).ToList();
            }
            return View(stocks);
        }
        /*
        // GET: Stock/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stock.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }
        */
        // GET: Stock/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Stock/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Producto,ProductoId,Cantidad,Ubicacion,SolicitudId,IngresoId")] Stock stock)
        {
            if (ModelState.IsValid)
            {
                db.Stock.Add(stock);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(stock);
        }
        /*
        // GET: Stock/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stock.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        // POST: Stock/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Producto,ProductoId,Cantidad,Ubicacion,SolicitudId,IngresoId")] Stock stock)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stock).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stock);
        }

        // GET: Stock/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stock.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        // POST: Stock/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Stock stock = db.Stock.Find(id);
            db.Stock.Remove(stock);
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
            nombres = new string[campos.Count + 1];
            nombres[0] = "Todos";
            int count = 1;
            foreach (var campo in campos)
            {
                nombres[count] = campo.Nombre;
                count++;
            }
            return nombres;
        }
        public List<Datos> cargarDatos(string buscado)
        {
            Datos tipos = new Datos();
            try
            {
                tipos = db.Datos.First(x => x.EmpresaId == cuenta.Id && x.Valor == buscado);
                ViewBag.id = tipos.Id;
            }
            catch { }
            List<Datos> datosTipos = new List<Datos>();
            int i = 0;
            if (tipos.Campo != null)
            {
                foreach (var tipo in tipos.Campo.Split(new char[] { ',' }))
                {
                    if (!String.IsNullOrEmpty(tipo))
                    {
                        Datos dato = new Datos()
                        {
                            Campo = tipo,
                            Llave = Convert.ToString(i)
                        };
                        i++;
                        datosTipos.Add(dato);
                    }
                }
            }
            return (datosTipos);
        }
    }
}
