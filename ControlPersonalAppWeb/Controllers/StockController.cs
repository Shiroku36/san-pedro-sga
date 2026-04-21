using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb;
using SpreadsheetLight;

namespace ControlPersonalAppWeb.Controllers
{/*
    public class StockController : Controller
    {
        private SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: Stock 
        public ActionResult Tipos()
        {
            Utils.SessionManager.log("Index tipos");
            return View(cargarDatos("Tipos"));
        }
        public ActionResult Menu()
        {
            return View();
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
            List<Stock> inventario = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Total").OrderBy(x => x.Producto).ToList();
            //inventario = ActualizarInventario(inventario);
            /*
            inventario = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList();
            List<Stock> inventario2 = new List<Stock>();
            foreach (var unidad in inventario)
            {
                try
                {
                    Producto producto = db.Producto.First(x => x.Id == unidad.ProductoId);
                    unidad.Unidad = producto.Unidad;
                }
                catch {
                    inventario2.Add(unidad);
                }
            }
            foreach(var producto in inventario2)
            {
                db.Stock.Remove(producto);
            }
            db.SaveChanges();*/
    /* return View(CargarSolicitados(inventario, ""));
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
         stocks = db.Stock.Where(x => x.Ubicacion == huerto && x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto").ToList();
         return View(CargarSolicitados(stocks, huerto));
     }
     else
     {
         stocks = db.Stock.Where(x => x.Ubicacion == huerto && x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto" && x.Empresa == tipo).ToList();
         return View(CargarSolicitados(stocks, huerto));
     }
     return View(CargarSolicitados(stocks,""));
 }
 List<Stock> ActualizarInventario(List<Stock> inventario)
 {
     foreach(var productoInventario in inventario)
     {
         productoInventario.Cantidad = 0;
         List<Stock> productos = db.Stock.Where(x => x.ProductoId == productoInventario.ProductoId).ToList();
         foreach (var producto in productos)
         {
             productoInventario.Cantidad += producto.Cantidad;
         }
     }
     return inventario;
 }
 List<Stock> CargarSolicitados(List<Stock> inventario, string huerto)
 {
     foreach (var producto in inventario)
     {
         producto.Solicitados = 0;
     }
     db.SaveChanges();
     List<Solicitud> solicitudes = new List<Solicitud>();
     if (huerto!="")
     {
         solicitudes = db.Solicitud.Where(x => x.Estado == "Solicitado" && x.Origen == huerto).ToList();
     }
     else
     {
         solicitudes = db.Solicitud.Where(x => x.Estado == "Solicitado").ToList();
     }
     foreach (var solicitud in solicitudes)
     {
         List<Stock> solicitados = db.Stock.Where(x => x.SolicitudId == solicitud.Id).ToList();
         foreach (var solicitado in solicitados)
         {
             foreach (var producto in inventario)
             {
                 if (producto.ProductoId == solicitado.ProductoId && solicitado.Cantidad > 0)
                 {
                     producto.Solicitados += solicitado.Cantidad;
                     break;
                 }
             }
         }
     }
     return inventario;
 }
 public void GenerarExcelInventario()
 {
     List<Stock> inventario = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Total").OrderBy(x => x.Producto).ToList();
     try
     {
         //creamos el objeto SLDocument el cual creara el excel
         SLDocument sl = new SLDocument();
         sl.RenameWorksheet(SLDocument.DefaultFirstSheetName, "Solicitudes");
         SLStyle fecha = sl.CreateStyle();
         SLStyle hora = sl.CreateStyle();
         fecha.FormatCode = "dd-mm-yyyy";
         hora.FormatCode = "hh:mm";
         //creamos las celdas en diagonal
         //utilizando la función setcellvalue pueden navegar sobre el documento
         //primer parametro es la fila el segundo la columna y el tercero el dato de la celda
         sl.SetCellValue(1, 1, "Producto");
         sl.SetCellValue(1, 2, "Cantidad");
         sl.SetCellValue(1, 3, "Unidad");
         sl.SetCellValue(1, 4, "Tipo");
         int j = 2;
         for (int i = 0; i < inventario.Count; ++i)
         {
             j = i + 2;
             sl.SetCellValue(j, 1, inventario[i].Producto);
             sl.SetCellValue(j, 2, (int)inventario[i].Cantidad);
             sl.SetCellValue(j, 3, inventario[i].Unidad);
             sl.SetCellValue(j, 4, inventario[i].Empresa);

         }
         //Guardar como, y aqui ponemos la ruta de nuestro archivo
         sl.SaveAs("C:\\Data\\Informe.xlsx");

     }
     catch (Exception ex)
     {
         Console.WriteLine("Ocurrio una Excepción: " + ex.Message);
     }

     //doc.SaveAs("C:\\Data\\WorksheetOperations.xlsx");
     FileStream sourceFile = new FileStream("C:\\Data\\Informe.xlsx", FileMode.Open);
     float FileSize;
     FileSize = sourceFile.Length;
     byte[] getContent = new byte[(int)FileSize];
     sourceFile.Read(getContent, 0, (int)sourceFile.Length);
     sourceFile.Close();
     Response.ClearContent();
     Response.ClearHeaders();
     Response.Buffer = true;
     Response.ContentType = "application/vnd.ms-excel";
     Response.AddHeader("Content-Length", getContent.Length.ToString());
     Response.AddHeader("content-disposition", "attachment;filename=" + "Productos inventario" + " " + DateTime.Now.ToShortDateString() + ".xlsx");
     Response.BinaryWrite(getContent);
     Response.Flush();
     Response.End();
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
 */ /*
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
         tipos = db.Datos.First(x => x.EmpresaId == cuenta.EmpresaId && x.Valor == buscado);
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
*/
}
