using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb;
using SpreadsheetLight;

namespace ControlPersonalAppWeb.Controllers
{/*
    public class SolicitudesController : Controller
    {
        private SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: Solicitudes
        public ActionResult Index()
        {
            Utils.SessionManager.log("Index solicitudes");
            ViewBag.alerta = 0;
            return View(db.Solicitud.Where(x => x.EmpresaId == cuenta.EmpresaId).OrderByDescending(x => x.Fecha).ToList());
        }

        public void Consolidado()
        {
            List<Solicitud> solicitudes = db.Solicitud.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList();
            List<ControlInventario> controlInventarios = db.ControlInventario.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList();
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
                sl.SetCellValue(1, 1, "Fecha");
                sl.SetCellValue(1, 2, "Trabajador");
                sl.SetCellValue(1, 3, "Observación");
                sl.SetCellValue(1, 4, "Origen");
                sl.SetCellValue(1, 5, "Destino");
                sl.SetCellValue(1, 6, "Estado");
                sl.SetCellValue(1, 7, "ID");
                sl.SetCellValue(1, 8, "Productos");
                sl.SetCellValue(1, 9, "Cantidad");
                int j = 2;
                for (int i = 0; i < solicitudes.Count; ++i)
                {
                    j = i + 2;
                    int id = solicitudes[i].Id;
                    List<Stock> productos = db.Stock.Where(x => x.SolicitudId == id).ToList();
                    if(productos.Count !=0)
                    {
                        foreach (var producto in productos)
                        {
                            sl.SetCellValue(j, 1, solicitudes[i].Fecha.Value.ToShortDateString());
                            sl.SetCellStyle(j, 1, fecha);
                            sl.SetCellValue(j, 2, solicitudes[i].Trabajador);
                            sl.SetCellValue(j, 3, solicitudes[i].Observación);
                            sl.SetCellValue(j, 4, solicitudes[i].Origen);
                            sl.SetCellValue(j, 5, solicitudes[i].Destino);
                            sl.SetCellValue(j, 6, solicitudes[i].Estado);
                            sl.SetCellValue(j, 7, solicitudes[i].Id);
                            sl.SetCellValue(j, 8, producto.Producto);
                            sl.SetCellValue(j, 9, Convert.ToString(producto.Cantidad));
                            j++;
                        }
                    }

                }
                sl.AddWorksheet("Control invetario");

                sl.SetCellValue(1, 1, "Fecha");
                sl.SetCellValue(1, 2, "Trabajador");
                sl.SetCellValue(1, 3, "Razón");
                sl.SetCellValue(1, 4, "ID");
                sl.SetCellValue(1, 5, "Productos");
                sl.SetCellValue(1, 6, "Cantidad");
                j = 2;
                for (int i = 0; i < controlInventarios.Count; ++i)
                {
                    int id = controlInventarios[i].Id;
                    List<Stock> productos = db.Stock.Where(x => x.ControlId == id).ToList();
                    if (productos.Count != 0)
                    {
                        foreach (var producto in productos)
                        {
                            sl.SetCellStyle(j, 1, fecha);
                            sl.SetCellValue(j, 1, controlInventarios[i].Fecha.Value.ToShortDateString());
                            sl.SetCellValue(j, 4, controlInventarios[i].Id);
                            sl.SetCellValue(j, 2, controlInventarios[i].Trabajador);
                            sl.SetCellValue(j, 3, controlInventarios[i].Razon);
                            sl.SetCellValue(j, 5, producto.Producto);
                            sl.SetCellValue(j, 6, Convert.ToString(producto.Cantidad));
                            j++;
                        }
                    }

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
            Response.AddHeader("content-disposition", "attachment;filename=" + "Consolidado bodega" + " " + DateTime.Now.ToShortDateString() + ".xlsx");
            Response.BinaryWrite(getContent);
            Response.Flush();
            Response.End();
            //System.Diagnostics.Process.Start("C:\\Data\\WorksheetOperations.xlsx");
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
            int idd = solicitud.Id;
            ViewBag.tipo = "producto";
            ViewBag.comentarios = db.Comentario.Where(x => x.SolicitudId == idd).ToList();
            return View(solicitud);
        }

        // GET: Solicitudes/Create
        public ActionResult Create()
        {
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            int empresaId =(int) cuenta.EmpresaId;
            ViewBag.productos = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto" && x.Cantidad>0).OrderBy(x => x.Producto).ToList();
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
                int contador = db.Solicitud.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList().Count + 1;
                solicitud.Trabajador = cuenta.Nombre + " " + cuenta.Apellido;
                solicitud.TrabajadorId = cuenta.Id;
                solicitud.Estado = "Solicitado";
                solicitud.Fecha = DateTime.Now ;
                solicitud.Empresa = contador.ToString();
                solicitud.EmpresaId = cuenta.EmpresaId;
                solicitud.Productos = "";
                string[] productos = collection["Producto"].Split(new char[] { ',' });
                string[] cantidades = collection["Cantidad"].Split(new char[] { ',' });
                db.Solicitud.Add(solicitud);
                db.SaveChanges();
                Utils.SessionManager.log("Crear solicitud: " + solicitud.Id);
                string texto = "";
                for (int i = 0; i <productos.Length; i++)
                {
                    Stock stock = new Stock();
                    string nombre = productos[i];
                    Producto producto = db.Producto.First(x => x.Nombre == nombre );
                    stock.Producto = producto.Nombre;
                    stock.ProductoId = producto.Id;
                    stock.Cantidad = Convert.ToInt32(cantidades[i]);
                    stock.EmpresaId = cuenta.EmpresaId;
                    stock.SolicitudId = solicitud.Id;
                    stock.Unidad = producto.Unidad;
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
                List<String> correos = db.Cuentas.Where(x => x.Empresa == cuenta.Empresa && x.Notificacion == true).Select(x => x.Email).ToList();
                Notificacion notificacion = new Notificacion()
                {
                    Fecha = DateTime.Now,
                    Correo = string.Join(", ", correos),
                    SolicitudId = solicitud.Id,
                    Estado = "Solicitado",
                    CuentaId = cuenta.EmpresaId,
                    Info = "Producto"
                };
                if(!string.IsNullOrEmpty(solicitud.Observación))
                {
                    notificacion.Texto = "El trabajador " + cuenta.Nombre + " " + cuenta.Apellido + " ha solicitado los siguientes productos: " +
                    texto + ", desde: " + solicitud.Origen + " a " + solicitud.Destino + ", el día " + solicitud.Fecha.Value.ToLongDateString() +
                    " a las " + solicitud.Fecha.Value.ToShortTimeString() +
                    " Observación: " + solicitud.Observación;
                }
                else
                {
                    notificacion.Texto = "El trabajador " + cuenta.Nombre + " " + cuenta.Apellido + " ha solicitado los siguientes productos: " +
                    texto + ", desde: " + solicitud.Origen + " a " + solicitud.Destino + ", el día " + solicitud.Fecha.Value.ToLongDateString() +
                    " a las " + solicitud.Fecha.Value.ToShortTimeString();
                }
                //enviar correo
                db.Notificacion.Add(notificacion);
                db.SaveChanges();
                Utils.SessionManager.log("Crear notificacion: " + notificacion.Id);
                ViewBag.alerta = Utils.SessionManager.enviarCorreo(correos, notificacion);
                int id = cuenta.Id; 
                Utils.SessionManager.notificaciones = db.Notificacion.Where(x => x.CuentaId == id && x.Estado == "Solicitado").ToList().OrderByDescending(x => x.Fecha).ToList();
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
            nombres = new string[campos.Count];
            int count = 0;
            foreach (var campo in campos)
            {
                nombres[count] = campo.Nombre;
                count++;
            }
            return nombres;
        }
    }*/
}
