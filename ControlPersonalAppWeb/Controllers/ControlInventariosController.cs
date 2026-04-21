using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using ControlPersonalAppWeb;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ControlPersonalAppWeb.Controllers
{/*
    public class ControlInventariosController : Controller
    {
        private SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: ControlInventarios
        public ActionResult Index()
        {
            Utils.SessionManager.log("Index control inventario");
            return View(db.ControlInventario.Where(x => x.EmpresaId == cuenta.EmpresaId).OrderByDescending(x => x.Id).ToList());
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
            int idd = controlInventario.Id;
            ViewBag.tipo = "control";
            ViewBag.comentarios = db.Comentario.Where(x => x.ControlInventarioId == idd).ToList();
            return View(controlInventario);
        }

        // GET: ControlInventarios/Create
        public ActionResult Create()
        {
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            int empresaId = (int)cuenta.EmpresaId;
            ViewBag.productos = db.Stock.Where(x => x.EmpresaId == cuenta.EmpresaId && x.Tipo == "Producto" && x.Cantidad > 0).OrderBy(x => x.Producto).ToList();
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
                int contador = db.ControlInventario.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList().Count + 1;
                controlInventario.Trabajador = cuenta.Nombre + " " + cuenta.Apellido;
                controlInventario.TrabajadorId = cuenta.Id;
                controlInventario.Fecha = DateTime.Now;
                controlInventario.Empresa = contador.ToString();
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
                    stock.EmpresaId = cuenta.EmpresaId;
                    stock.Unidad = producto.Unidad;
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
        public ActionResult Informe()
        {
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    List<ControlInventario> controlInventarios = db.ControlInventario.Where(x => x.EmpresaId == cuenta.EmpresaId).ToList();
                    string strHeader = "Registros de uso de productos";
                    //System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    Document document = new Document();
                    PdfWriter writer = PdfWriter.GetInstance(document, Response.OutputStream);
                    document.Open();

                    Image image = Image.GetInstance(Server.MapPath("~/App_Data/" + Utils.SessionManager.CuentaAutenticada().Empresa + ".png"));
                    image.Alignment = Element.ALIGN_LEFT;
                    image.ScaleToFit(60, 60);
                    document.Add(image);

                    //Report Header
                    BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntHead = new Font(bfntHead, 16, 1, BaseColor.DARK_GRAY);
                    Paragraph prgHeading = new Paragraph();
                    prgHeading.Alignment = Element.ALIGN_CENTER;
                    prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
                    document.Add(prgHeading);

                    //Author
                    Paragraph prgAuthor = new Paragraph();
                    BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntAuthor = new Font(btnAuthor, 8, 2, BaseColor.DARK_GRAY);
                    prgAuthor.Alignment = Element.ALIGN_RIGHT;
                    prgAuthor.Add(new Chunk("Autor : " + Utils.SessionManager.CuentaAutenticada().Usuario, fntAuthor));
                    prgAuthor.Add(new Chunk("\nFecha : " + DateTime.Now.ToShortDateString(), fntAuthor));
                    document.Add(prgAuthor);
                    Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                    document.Add(p);
                    //Add line break
                    document.Add(new Chunk("\n", fntHead));

                    List<string> titulos = new List<string> { "Trabajador", "Fecha", "Razón", "Productos"};
                    PdfPTable tabla = new PdfPTable(titulos.Count) { WidthPercentage = 100f };
                    //tablaHorasTrabajadas.SetWidths(new int[] { 3, 1 });
                    //Table header
                    BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntColumnHeader = new Font(btnColumnHeader, 8, 1, BaseColor.WHITE);
                    Font fntCell = new Font(btnColumnHeader, 8, 1, BaseColor.BLACK);
                    for (int i = 0; i < titulos.Count; i++)
                    {
                        PdfPCell cell = new PdfPCell() { BackgroundColor = BaseColor.GRAY };
                        cell.AddElement(new Chunk(titulos[i], fntColumnHeader));
                        tabla.AddCell(cell);
                    }
                    //table Data
                    foreach (var celda in controlInventarios.OrderByDescending(x => x.Fecha))
                    {
                        tabla.AddCell(new Phrase(celda.Trabajador, fntCell));
                        tabla.AddCell(new Phrase(celda.Fecha.Value.ToString(), fntCell));
                        tabla.AddCell(new Phrase(celda.Razon, fntCell));
                        List<Stock> productos = db.Stock.Where(x => x.ControlId == celda.Id).ToList();
                        PdfPTable tablaProductos = new PdfPTable(1) { WidthPercentage = 100f };
                        foreach (var producto in productos)
                        {
                            tablaProductos.AddCell(new PdfPCell(new Phrase(producto.Producto + ": " + producto.Cantidad, fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                        }
                        tabla.AddCell(new PdfPCell(tablaProductos));
                        
                    }

                    document.Add(tabla);
                    document.Close();
                    writer.Close();
                    Response.ContentType = "application/pdf";
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.AddHeader("content-disposition", "attachment;filename=" + strHeader + ".pdf");
                    Response.Write(document);
                    Response.End();
                }
                return RedirectToAction("Index");
            }
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
    /*
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
    }*/
}
