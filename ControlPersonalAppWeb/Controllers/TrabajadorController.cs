
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Font = iTextSharp.text.Font;
using ControlPersonalAppWeb.Models;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SpreadsheetLight;
using System.Data;
using Image = iTextSharp.text.Image;
using System.Net.Mail;
using System.Drawing;
using System.Globalization;
using System.Data.Entity;
using Rectangle = iTextSharp.text.Rectangle;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using static System.Net.WebRequestMethods;
using Microsoft.Win32;
using WebGrease.Css.Extensions;

namespace ControlPersonalAppWeb.Controllers
{
    public class TrabajadorController : Controller
    {

        SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta => Utils.SessionManager.CuentaAutenticada();
        Dictionary<TrabajadorIndex, Ranking> rankings = new Dictionary<TrabajadorIndex, Ranking>();

        private void DeshabilitarExpirados()
        {
            try
            {
                db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.Trabajador SET Habilitado = 0 WHERE Expiración IS NOT NULL AND Expiración < @p0 AND Habilitado = 1",
                    DateTime.Today);
            }
            catch { }
        }
        Dictionary<TrabajadorIndex, List<DateTime>> diasTrabajados = new Dictionary<TrabajadorIndex, List<DateTime>>();
        string path = Infrastructure.AppSettings.FileStoragePath;

        private static readonly HashSet<string> _allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".txt", ".csv"
        };

        private string GuardarArchivoSeguro(HttpPostedFileBase postedFile, int trabajadorId)
        {
            string safeFileName = Path.GetFileName(postedFile.FileName);
            string extension = Path.GetExtension(safeFileName);
            if (!_allowedExtensions.Contains(extension))
                return null;
            string dirPath = Path.Combine(path, trabajadorId.ToString());
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            string fullPath = Path.Combine(dirPath, safeFileName);
            postedFile.SaveAs(fullPath);
            return safeFileName;
        }

        public ActionResult Borrar()
        {
            return RedirectToAction("Index");
        }
        public ActionResult DescargarArchivo()
        {
            // Ruta del archivo CSV en el servidor
            string rutaArchivo = Server.MapPath("~/App_Data/Carga_masiva.xlsx");

            // Configuración de la respuesta HTTP
            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AppendHeader("Content-Disposition", "attachment; filename=Carga masiva trabajadores.xlsx");

            // Transmitir el contenido del archivo al cliente
            Response.TransmitFile(rutaArchivo);

            // Finalizar la respuesta
            Response.Flush();
            HttpContext.ApplicationInstance.CompleteRequest();

            return new EmptyResult();
        }
        public ActionResult Cargar()
        {
            SgajcpEntities db = new SgajcpEntities();
            string empresa = ControlPersonalAppWeb.Utils.SessionManager.CuentaAutenticada().Empresa;
            var empresas = db.Empresas.Select(x => x.Nombre).ToList(); ;
            if (empresa != "JCP")
            {
                empresas = db.Empresas.Where(x => x.Nombre == empresa).Select(x => x.Nombre).ToList();
            }
            ViewBag.Empresas = empresas;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cargar(FormCollection collection)
        {
            SgajcpEntities db = new SgajcpEntities();
            /*
            using (SLDocument sl = new SLDocument())
            {
                HttpPostedFileBase hpf = Request.Files["csv"];
                FileStream fs = new FileStream(hpf.FileName, FileMode.Open);
                SLDocument sheet = new SLDocument(fs, "sheet1");

                SLWorksheetStatistics stats = sheet.GetWorksheetStatistics();
                for (int j = 2; j < stats.EndRowIndex; j++)
                {
                    // Get the first column of the row (SLS is a 1-based index)
                    string numero = sheet.GetCellValueAsString(j, 1);
                    string uid = sheet.GetCellValueAsString(j, 2);
                    Pulseras pulsera = new Pulseras() { Numero = numero, Uid = uid};
                    db.Pulseras.Add(pulsera);
                }
                //db.SaveChanges();
            }
            */
            string empresa = cuenta.Empresa;
            string mensaje = "";
            HttpPostedFileBase hpf = Request.Files["csv"];
            if (hpf != null && hpf.ContentLength > 0)
            {
                if (hpf.ContentLength > 5 * 1024 * 1024)
                {
                    ViewBag.mensaje = "El archivo excede el tamaño máximo permitido (5 MB).";
                    return View();
                }
                StreamReader csvreader = new StreamReader(hpf.InputStream);
                var line1 = csvreader.ReadLine();
                string rut = "";
                while (!csvreader.EndOfStream)
                {
                    try
                    {
                        var line = csvreader.ReadLine();
                        string[] values = new string[line.Length];
                        if (line.Contains(","))
                        {
                            values = line.Split(',');
                        }
                        if (line.Contains(";"))
                        {
                            values = line.Split(';');
                        }
                        if (line.Contains("\t"))
                        {
                            values = line.Split('\t');
                        }
                        if (!String.IsNullOrEmpty(values[1]))
                        {
                            Trabajador trabajador = new Trabajador();
                            trabajador.Rut = formatearRut(values[0]);
                            trabajador.ApellidoPaterno = values[1]?.Trim();
                            trabajador.ApellidoMaterno = values[2]?.Trim();
                            trabajador.Nombre = values[3]?.Trim();
                            trabajador.Numero = values[4]?.Trim();
                            trabajador.Email = values[5]?.Trim();
                            trabajador.Direccion = values[6];
                            trabajador.Telefono = values[7]?.Trim();
                            if (values[8] != null)
                            {
                                trabajador.Habilitado = true;
                            }
                            else
                            {
                                trabajador.Habilitado = false;
                            }
                            trabajador.Empresa = values[9];
                            if (!String.IsNullOrEmpty(trabajador.Numero))
                                trabajador.Uid = db.Pulseras.First(x => x.Numero == trabajador.Numero).Uid;
                            db.Trabajador.Add(trabajador);
                        }
                    }
                    catch (DbEntityValidationException e)
                    {
                        mensaje = mensaje + "\n Rut: " + rut + " Error: " + e.Message;
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            ViewBag.mensaje = ViewBag.mensaje + "Entity of type " + eve.Entry.Entity.GetType().Name + " in state " + eve.Entry.State + " has the following validation errors:";
                            foreach (var ve in eve.ValidationErrors)
                            {
                                ViewBag.mensaje = ViewBag.mensaje + "- Property: " + ve.PropertyName + ", Error: " + ve.ErrorMessage;
                            }
                        }

                    }
                }
                if (String.IsNullOrEmpty(mensaje))
                {
                    ViewBag.mensaje = true;
                }
                else
                {
                    ViewBag.mensaje = false;
                }
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        ViewBag.mensaje = ViewBag.mensaje + "Entity of type " + eve.Entry.Entity.GetType().Name + " in state " + eve.Entry.State + " has the following validation errors:";
                        foreach (var ve in eve.ValidationErrors)
                        {
                            ViewBag.mensaje = ViewBag.mensaje + "- Property: " + ve.PropertyName + ", Error: " + ve.ErrorMessage;
                        }
                    }
                }
                csvreader.Close();
            }

            Utils.SessionManager.log("Carga trabajador");
            return View();
        }
        public System.Drawing.Image GetImage(byte[] imageBytes)
        {
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes))
            {
                return System.Drawing.Image.FromStream(ms);
            }
        }
        public Trabajador GetTrabajador(List<Trabajador> trabajadores, string uid)
        {
            foreach (var trabajador in trabajadores)
            {
                if (trabajador.Uid != null && trabajador.Uid.Contains(uid))
                {
                    return trabajador;
                }
            }
            return null;
        }
        public ActionResult Hoy()
        {
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            SgajcpEntities database = new SgajcpEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            List<TrabajadorIndex> trabajadores = database.Trabajador
                                            .Where(x => x.Empresa == empresa)
                                            .Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoMaterno = x.ApellidoMaterno, ApellidoPaterno = x.ApellidoPaterno, Uid = x.Uid }).ToList();
            string date = DateTime.Now.ToShortDateString();
            DateTime dateTime = Convert.ToDateTime(date);
            int i = 0;
            while (i < trabajadores.Count)
            {
                var trabajador = trabajadores[i];
                var registry = database.RegistroTrabajador.Where(x => x.Fecha >= dateTime && x.Uid == trabajador.Uid).ToList();
                if (registry.Count > 0)
                {
                    foreach (RegistroTrabajador registro in registry)
                    {
                        registro.IdTrabajador = trabajador.Id;
                        registro.NombreTrabajador = trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno;
                    }
                    registros.AddRange(registry);
                    i++;
                }
                else
                {
                    trabajadores.Remove(trabajador);
                }
            }
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    string strHeader = "Registros del día " + DateTime.Now.ToLongDateString().Replace(",", "");
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

                    List<string> titulosRegistrosxdia = new List<string> { "Fecha", "Nombre", "Tipo", "Hora", "Campo" };
                    PdfPTable tablaRegistrosxdia = new PdfPTable(titulosRegistrosxdia.Count) { WidthPercentage = 100f };
                    tablaRegistrosxdia.SetWidths(new int[] { 1, 3, 1, 1, 1 });
                    //Table header
                    BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntColumnHeader = new Font(btnColumnHeader, 8, 1, BaseColor.WHITE);
                    Font fntCell = new Font(btnColumnHeader, 8, 1, BaseColor.BLACK);
                    for (i = 0; i < titulosRegistrosxdia.Count; i++)
                    {
                        PdfPCell cell = new PdfPCell() { BackgroundColor = BaseColor.GRAY };
                        cell.AddElement(new Chunk(titulosRegistrosxdia[i], fntColumnHeader));
                        tablaRegistrosxdia.AddCell(cell);
                    }
                    //table Data
                    foreach (var celda in registros.OrderByDescending(x => x.Fecha))
                    {
                        tablaRegistrosxdia.AddCell(new Phrase(celda.Fecha.ToShortDateString(), fntCell));
                        tablaRegistrosxdia.AddCell(new Phrase(celda.NombreTrabajador, fntCell));
                        if (celda.Fecha.Hour < 12)
                        {
                            tablaRegistrosxdia.AddCell(new Phrase("Entrada", fntCell));
                        }
                        else
                        {
                            tablaRegistrosxdia.AddCell(new Phrase("Salida", fntCell));
                        }
                        tablaRegistrosxdia.AddCell(new Phrase(celda.Fecha.ToShortTimeString(), fntCell));
                        tablaRegistrosxdia.AddCell(new Phrase(celda.Campo, fntCell));
                    }

                    document.Add(tablaRegistrosxdia);
                    document.Close();
                    writer.Close();
                    Response.ContentType = "application/pdf";
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.AddHeader("content-disposition", "attachment;filename=" + strHeader + ".pdf");
                    Response.Flush();
                    HttpContext.ApplicationInstance.CompleteRequest();
                }
            }
            return RedirectToAction("Index", "Informes");
        }
        public void GenerarPDFRegistros(List<TrabajadorIndex> trabajadores, List<RegistroTrabajador> registros, string strHeader, List<Registro> registrosLista)
        {
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    strHeader = strHeader + " " + trabajadores[0].Nombre.Replace(",", "");
                    //System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    Document document = new Document();
                    document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
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
                    if (!string.IsNullOrEmpty(trabajadores[0].Rut))
                    {
                        //Add a line seperation

                        fntHead.Size = 14;
                        document.Add(new Paragraph("Nombre: " + trabajadores[0].Nombre + " " + trabajadores[0].ApellidoPaterno + " " + trabajadores[0].ApellidoMaterno, fntHead));
                        document.Add(new Paragraph("Rut: " + trabajadores[0].Rut, fntHead));
                        p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                        document.Add(p);
                    }
                    //Add line break
                    document.Add(new Chunk("\n", fntHead));
                    BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntColumnHeader = new Font(btnColumnHeader, 8, 1, BaseColor.WHITE);
                    Font fntCell = new Font(btnColumnHeader, 8, 1, BaseColor.BLACK);

                    //Write the table
                    List<string> titulos = new List<string>();
                    titulos.Add("N°");
                    titulos.Add("Fecha");
                    titulos.Add("Nombre");
                    titulos.Add("Rut");
                    titulos.Add("Hora");
                    titulos.Add("Empresa");
                    titulos.Add("Predio");
                    titulos.Add("Habilitado");
                    PdfPTable table = new PdfPTable(titulos.Count);
                    table.WidthPercentage = 100f;
                    //Table heade
                    int i = 0;
                    for (i = 0; i < titulos.Count; i++)
                    {
                        PdfPCell cell = new PdfPCell();
                        cell.BackgroundColor = BaseColor.GRAY;
                        cell.AddElement(new Chunk(titulos[i], fntColumnHeader));
                        table.AddCell(cell);
                    }
                    //table Data
                    i = 1;
                    foreach (var celda in registrosLista)
                    {
                        table.AddCell(new Phrase(i.ToString(), fntCell));
                        i++;
                        if (celda.EntradaModificada != null)
                        {
                            celda.Entrada = celda.Entrada + " *";
                        }
                        if (celda.SalidaModificada != null)
                        {
                            celda.Salida = celda.Salida + " *";
                        }
                        table.AddCell(new Phrase(celda.Fecha.ToShortDateString(), fntCell));
                        table.AddCell(new Phrase(celda.Nombre + " " + celda.ApellidoPaterno + " " + celda.ApellidoMaterno, fntCell));
                        table.AddCell(new Phrase(celda.Rut, fntCell));
                        table.AddCell(new Phrase(celda.Entrada, fntCell));
                    }

                    document.Add(table);
                    document.Close();
                    writer.Close();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strHeader + " " + DateTime.Now.ToShortDateString() + ".pdf");
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Flush();
                    HttpContext.ApplicationInstance.CompleteRequest();
                    Utils.SessionManager.entrada = "";
                    Utils.SessionManager.salida = "";
                    Utils.SessionManager.almuerzo = "";
                    Utils.SessionManager.log("Generar pfd registros");
                }
            }

        }
        public void PDFRegistros(string campo, string empresa, string inicio, string fin, string tipo)
        {
            DeshabilitarExpirados();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    DateTime inicioIn = Convert.ToDateTime(inicio);
                    DateTime finIn = Convert.ToDateTime(fin);
                    List<TrabajadorIndex> trabajadores = new List<TrabajadorIndex>();
                    List<Trabajador> workers = new List<Trabajador>();
                    List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
                    List<Registro> registrosLista = new List<Registro>();
                    List<RegistroPDF> registrosTrabajador = new List<RegistroPDF>();
                    string empresaAutor = cuenta.Empresa;
                    string strHeader = "Registro trabajadores ";
                    try
                    {
                        trabajadores = db.Trabajador.Select(x => new TrabajadorIndex
                        {
                            Nombre = x.Nombre,
                            Uid = x.Uid,
                            Rut = x.Rut,
                            ApellidoMaterno = x.ApellidoMaterno,
                            ApellidoPaterno = x.ApellidoPaterno,
                            Habilitado = (bool) x.Habilitado,
                            Empresa = x.Empresa
                        }).ToList();
                    }
                    catch { }
                    if (empresa == null)
                    {
                        strHeader = strHeader + " de " + empresaAutor;
                        trabajadores = db.Trabajador.Select(x => new TrabajadorIndex
                        {
                            Nombre = x.Nombre,
                            ApellidoPaterno = x.ApellidoPaterno,
                            ApellidoMaterno = x.ApellidoMaterno,
                            Rut = x.Rut,
                            Empresa = x.Empresa,
                            Habilitado = (bool)x.Habilitado,
                            Uid = x.Uid,
                        }).Where(x => x.Empresa == empresaAutor).ToList();
                    }
                    if (campo == "Todos" && empresa == "Todos")
                    {

                        strHeader = strHeader + " " + inicioIn.ToShortDateString() +" a " + finIn.ToShortDateString();
                        registrosTrabajador = db.RegistroTrabajador.
                                                   Select(x => new RegistroPDF
                                                   {
                                                       Campo = x.Campo,
                                                       Empresa = x.Empresa,
                                                       Fecha = x.Fecha,
                                                       Uid = x.Uid,
                                                       Modificado = x.IdTrabajador
                                                   }).
                                                   Where(x => x.Fecha >= inicioIn &&
                                                              x.Fecha <= finIn ).ToList();
                    }

                    if (campo != "Todos" && empresa == "Todos")
                    {

                        strHeader = strHeader + " " + inicioIn.ToShortDateString() + " a " + finIn.ToShortDateString()+ " en "+campo;
                        registrosTrabajador = db.RegistroTrabajador.
                                                   Select(x => new RegistroPDF
                                                   {
                                                       Campo = x.Campo,
                                                       Empresa = x.Empresa,
                                                       Fecha = x.Fecha,
                                                       Uid = x.Uid,
                                                       Modificado = x.IdTrabajador
                                                   }).
                                                   Where(x => x.Fecha >= inicioIn &&
                                                              x.Fecha <= finIn && 
                                                              x.Campo == campo).ToList();
                    }

                    if (campo == "Todos" && empresa != "Todos")
                    {

                        strHeader = strHeader + " " + inicioIn.ToShortDateString() + " a " + finIn.ToShortDateString() + " de " + empresa;
                        registrosTrabajador = db.RegistroTrabajador.
                                                   Select(x => new RegistroPDF
                                                   {
                                                       Campo = x.Campo,
                                                       Empresa = x.Empresa,
                                                       Fecha = x.Fecha,
                                                       Uid = x.Uid,
                                                       Modificado = x.IdTrabajador
                                                   }).
                                                   Where(x => x.Fecha >= inicioIn &&
                                                              x.Fecha <= finIn).ToList();
                    }


                    if (campo != "Todos" && empresa != "Todos")
                    {
                        strHeader = strHeader + " " + inicioIn.ToShortDateString() + " a " + finIn.ToShortDateString() + " en " + campo + " de " +empresa;
                        registrosTrabajador = db.RegistroTrabajador.
                                                   Select(x => new RegistroPDF
                                                   {
                                                       Campo = x.Campo,
                                                       Empresa = x.Empresa,
                                                       Fecha = x.Fecha,
                                                       Uid = x.Uid,
                                                       Modificado = x.IdTrabajador
                                                   }).
                                                   Where(x => x.Fecha >= inicioIn &&
                                                              x.Fecha <= finIn &&
                                                              x.Campo == campo).ToList();
                    }
                    foreach (var registro in registrosTrabajador)
                    {
                        TrabajadorIndex trabajador = null;

                        // Intento 1: buscar por IdTrabajador (si existe, es la referencia mas estable)
                        if (registro.Modificado != null && registro.Modificado != 1 && registro.Modificado > 0)
                        {
                            try
                            {
                                int idBuscar = (int)registro.Modificado;
                                trabajador = db.Trabajador.Select(x => new TrabajadorIndex{
                                    Nombre = x.Nombre,
                                    Uid = x.Uid,
                                    Rut = x.Rut,
                                    ApellidoMaterno = x.ApellidoMaterno,
                                    ApellidoPaterno = x.ApellidoPaterno,
                                    Habilitado = (bool)x.Habilitado,
                                    Empresa = x.Empresa,
                                    Id = x.Id
                                }).First(x => x.Id == idBuscar);
                            }
                            catch { }
                        }

                        // Intento 2: buscar por Uid directo
                        if (trabajador == null)
                        {
                            try
                            {
                                trabajador = db.Trabajador.Select(x => new TrabajadorIndex{
                                    Nombre = x.Nombre,
                                    Uid = x.Uid,
                                    Rut = x.Rut,
                                    ApellidoMaterno = x.ApellidoMaterno,
                                    ApellidoPaterno = x.ApellidoPaterno,
                                    Habilitado = (bool)x.Habilitado,
                                    Empresa = x.Empresa,
                                    Id = x.Id
                                }).First(x => x.Uid == registro.Uid);
                            }
                            catch { }
                        }

                        // Intento 3: Uid es numerico = fue mutado al Id del trabajador
                        if (trabajador == null)
                        {
                            int idFromUid;
                            if (int.TryParse(registro.Uid, out idFromUid) && idFromUid > 0)
                            {
                                try
                                {
                                    trabajador = db.Trabajador.Select(x => new TrabajadorIndex{
                                        Nombre = x.Nombre,
                                        Uid = x.Uid,
                                        Rut = x.Rut,
                                        ApellidoMaterno = x.ApellidoMaterno,
                                        ApellidoPaterno = x.ApellidoPaterno,
                                        Habilitado = (bool)x.Habilitado,
                                        Empresa = x.Empresa,
                                        Id = x.Id
                                    }).First(x => x.Id == idFromUid);
                                }
                                catch { }
                            }
                        }

                        if (trabajador == null) continue;

                        if (empresa != "Todos" && trabajador.Empresa != empresa)
                        {
                            continue;
                        }

                        Registro registroPDF = new Registro()
                        {
                            Campo = registro.Campo,
                            Uid = registro.Uid,
                            Empresa = trabajador.Empresa,
                            Fecha = registro.Fecha,
                            Nombre = trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno,
                            Rut = trabajador.Rut
                        };
                        if (trabajador.Habilitado == true)
                        {
                            registroPDF.Habilitado = "Si";
                        }
                        else
                        {
                            registroPDF.Habilitado = "No";
                        }
                        registrosLista.Add(registroPDF);
                    }
                    if (tipo == "EXCEL")
                    {
                        GenerarExcel(registrosLista, strHeader);
                        return;
                    }

                    //System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    Document document = new Document();
                    document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                    PdfWriter writer = PdfWriter.GetInstance(document, Response.OutputStream);
                    document.Open();
                    /*
                    Image image = Image.GetInstance(Server.MapPath("~/App_Data/" + Utils.SessionManager.CuentaAutenticada().Empresa + ".png"));
                    image.Alignment = Element.ALIGN_LEFT;
                    image.ScaleToFit(60, 60);
                    document.Add(image);
                    */

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
                    BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntColumnHeader = new Font(btnColumnHeader, 8, 1, BaseColor.WHITE);
                    Font fntCell = new Font(btnColumnHeader, 8, 1, BaseColor.BLACK);

                    //Write the table
                    List<string> titulos = new List<string>();
                    titulos.Add("N°");
                    titulos.Add("Fecha");
                    titulos.Add("Nombre");
                    titulos.Add("Rut");
                    titulos.Add("Hora");
                    titulos.Add("Contratista");
                    titulos.Add("Predio");
                    titulos.Add("Habilitado");
                    PdfPTable table = new PdfPTable(titulos.Count);
                    table.WidthPercentage = 100f;
                    table.SetWidths(new int[] { 30, 60, 150, 80, 50, 80, 80, 50 });
                    int i;
                    for (i = 0; i < titulos.Count; i++)
                    {
                        PdfPCell cell = new PdfPCell();
                        cell.BackgroundColor = BaseColor.GRAY;
                        cell.AddElement(new Chunk(titulos[i], fntColumnHeader));
                        table.AddCell(cell);
                    }
                    //table Data
                    i = 1;
                    foreach (var celda in registrosLista)
                    {
                        table.AddCell(new Phrase(i.ToString(), fntCell));
                        table.AddCell(new Phrase(celda.Fecha.ToString("dd/MM/yyy"), fntCell));
                        table.AddCell(new Phrase(celda.Nombre + " " + celda.ApellidoPaterno + " " + celda.ApellidoMaterno, fntCell));
                        table.AddCell(new Phrase(celda.Rut, fntCell));
                        table.AddCell(new Phrase(celda.Fecha.ToString("HH:mm"), fntCell));
                        table.AddCell(new Phrase(celda.Empresa, fntCell));
                        table.AddCell(new Phrase(celda.Campo, fntCell));
                        table.AddCell(new Phrase(celda.Habilitado, fntCell));
                        i++;
                    }

                    document.Add(table);
                    document.Close();
                    writer.Close();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strHeader + " " + DateTime.Now.ToShortDateString() + ".pdf");
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Flush();
                    HttpContext.ApplicationInstance.CompleteRequest();
                    Utils.SessionManager.entrada = "";
                    Utils.SessionManager.salida = "";
                    Utils.SessionManager.almuerzo = "";
                    Utils.SessionManager.log("Generar pfd registros");
                }
            }

        }
        public List<RegistroTrabajador> BuscarRegistroTrabajadores(string nombreEmpresa, string nombreCampo, DateTime? fechaInicio, DateTime? fechaFin)
        {
            using (var context = db)
            {
                // Iniciar la consulta como IQueryable
                var query = context.RegistroTrabajador.AsQueryable();

                // Filtrar por nombre de empresa si no es "todos"
                if (!string.IsNullOrEmpty(nombreEmpresa) && !nombreEmpresa.Equals("todos", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(rt => rt.Empresa == nombreEmpresa);
                }

                // Filtrar por nombre de campo si no es "todos"
                if (!string.IsNullOrEmpty(nombreCampo) && !nombreCampo.Equals("todos", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(rt => rt.Campo == nombreCampo);
                }

                // Filtrar por fecha de inicio si se proporciona
                if (fechaInicio.HasValue)
                {
                    query = query.Where(rt => rt.Fecha >= fechaInicio.Value);
                }

                // Filtrar por fecha de fin si se proporciona
                if (fechaFin.HasValue)
                {
                    query = query.Where(rt => rt.Fecha <= fechaFin.Value);
                }

                // Ejecutar la consulta y devolver los resultados como lista
                return query.ToList();
            }
        }


        public void generarRegistros()
        {

        }
        public static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            return (System.Drawing.Image)(new Bitmap(imgToResize, size));
        }
        public string Formato(string nombre)
        {
            while (nombre.Length < 28)
            {
                nombre = nombre + " ";
            }
            return nombre;
        }
        public void GenerarExcel(List<Registro> registrosLista, string strHeader)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xlsx");
            try
            {
                SLDocument sl = new SLDocument();
                SLStyle fecha = sl.CreateStyle();
                SLStyle hora = sl.CreateStyle();
                fecha.FormatCode = "dd-mm-yyyy";
                hora.FormatCode = "hh:mm";
                sl.SetCellValue(1, 1, "Fecha");
                sl.SetCellValue(1, 2, "Nombre");
                sl.SetCellValue(1, 3, "Rut");
                sl.SetCellValue(1, 4, "Hora");
                sl.SetCellValue(1, 5, "Contratista");
                sl.SetCellValue(1, 6, "Predio");
                sl.SetCellValue(1, 7, "Habilitado");

                for (int i = 2; i < registrosLista.Count + 2; ++i)
                {
                    int j = i - 2;
                    sl.SetCellValue(i, 1, registrosLista[j].Fecha.ToShortDateString());
                    sl.SetCellStyle(i, 1, fecha);
                    sl.SetCellValue(i, 2, registrosLista[j].Nombre + " " + registrosLista[j].ApellidoPaterno + " " + registrosLista[j].ApellidoMaterno);
                    sl.SetCellValue(i, 3, formatearRut(registrosLista[j].Rut));
                    sl.SetCellValue(i, 4, registrosLista[j].Fecha.ToString("HH:mm"));
                    sl.SetCellStyle(i, 4, hora);
                    sl.SetCellValue(i, 5, registrosLista[j].Empresa);
                    sl.SetCellValue(i, 6, registrosLista[j].Campo);
                    sl.SetCellValue(i, 7, registrosLista[j].Habilitado);
                }

                sl.SaveAs(tempFile);
            }
            catch
            {
                return;
            }

            byte[] getContent = System.IO.File.ReadAllBytes(tempFile);
            try { System.IO.File.Delete(tempFile); } catch { }
            Response.ClearContent();
            Response.ClearHeaders();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Length", getContent.Length.ToString());
            Response.AddHeader("content-disposition", "attachment;filename=" + strHeader + ".xlsx");
            Response.BinaryWrite(getContent);
            Response.Flush();
            HttpContext.ApplicationInstance.CompleteRequest();
            Utils.SessionManager.log("Generar excel registros");
        }
        public ActionResult PorDia()
        {
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            SgajcpEntities database = new SgajcpEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            List<Trabajador> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).ToList();
            string date = DateTime.Now.ToShortDateString();
            DateTime dateTime = Convert.ToDateTime(date);
            dateTime = dateTime.AddHours(8);
            dateTime = dateTime.AddMinutes(15);
            int i = 0;
            while (i < trabajadores.Count)
            {
                var trabajador = trabajadores[i];
                var registry = database.RegistroTrabajador.Where(x => x.Fecha >= dateTime && x.Uid == trabajador.Uid).ToList();
                if (registry.Count > 0)
                {
                    registros.AddRange(registry);
                    i++;
                }
                else
                {
                    trabajadores.Remove(trabajadores[i]);
                }
            }
            ViewBag.trabajadores = trabajadores;
            return View(registros.OrderByDescending(x => x.Fecha));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PorDia(FormCollection collection)
        {
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            SgajcpEntities database = new SgajcpEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            DateTime fecha = Convert.ToDateTime(collection["Inicio"]);
            DateTime fin = fecha.AddHours(Convert.ToUInt32(collection["Horas"]));
            // List<Trabajador> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).ToList();
            List<TrabajadorIndex> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno, Uid = x.Uid, Empresa = x.Empresa }).ToList();
            int i = 0;
            while (i < trabajadores.Count)
            {
                var trabajador = trabajadores[i];
                var registry = database.RegistroTrabajador.Where(x => x.Fecha >= fecha && x.Fecha <= fin && x.Uid == trabajador.Uid).ToList();
                if (registry.Count > 0)
                {
                    registros.AddRange(registry);
                    i++;
                }
                else
                {
                    trabajadores.Remove(trabajadores[i]);
                }
            }
            ViewBag.trabajadores = trabajadores;
            return View(registros.OrderByDescending(x => x.Fecha));
        }
        public void PDFHoy()
        {
            SgajcpEntities database = new SgajcpEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            List<TrabajadorIndex> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex
            {
                Id = x.Id,
                Nombre = x.Nombre,
                ApellidoMaterno = x.ApellidoMaterno,
                ApellidoPaterno = x.ApellidoPaterno,
                Rut = x.Rut,
                Uid = x.Uid,
                Numero = x.Numero,
                Empresa = x.Empresa
            }).ToList();
            List<Registro> registros = GetRegistroTrabajadores(trabajadores, DateTime.Now, null, null, null);

            trabajadores.Insert(0, new TrabajadorIndex { Nombre = "Hoy" });
            GenerarPDFRegistros(trabajadores, null, "Registro", registros);
        }
        public List<Registro> GetRegistroTrabajadores(List<TrabajadorIndex> trabajadores, DateTime? inicio, DateTime? fin, string campo, DateTime? periodo)
        {
            SgajcpEntities database = new SgajcpEntities();
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            List<Registro> registrosLista = new List<Registro>();
            //Para cada trabajador se le cargan los registros solicitidaos
            foreach (var trabajador in trabajadores)
            {
                var registrosTrabajador = new List<RegistroPDF>();
                //Generar por periodo de tiempo
                if (periodo != null)
                {
                    int mes = periodo.Value.Month;
                    int año = periodo.Value.Year;
                    registrosTrabajador = database.RegistroTrabajador
                                        .Where(x => x.Fecha.Month == mes && x.Fecha.Year == año && x.Uid == trabajador.Uid)
                                        .Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                //Un campo y un inicio
                else if (inicio == null && fin == null && string.IsNullOrEmpty(campo))
                {
                    registrosTrabajador = database.RegistroTrabajador
                                          .Where(x => x.Uid == trabajador.Uid)
                                          .Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                else if (inicio != null && fin == null && string.IsNullOrEmpty(campo))
                {
                    DateTime end = Convert.ToDateTime(inicio);
                    registrosTrabajador = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid
                                                                            && x.Fecha.Year == end.Year
                                                                            && x.Fecha.Month == end.Month
                                                                            && x.Fecha.Day == end.Day).Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                else if (inicio != null && fin != null && string.IsNullOrEmpty(campo))
                {
                    DateTime end = Convert.ToDateTime(fin);
                    end = end.AddDays(1);
                    registrosTrabajador = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid && x.Fecha > inicio && x.Fecha < end).Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                else if (!string.IsNullOrEmpty(campo) && inicio != null && fin == null)
                {
                    DateTime end = Convert.ToDateTime(inicio);
                    registrosTrabajador = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid
                                                                            && x.Fecha.Year == end.Year
                                                                            && x.Fecha.Month == end.Month
                                                                            && x.Fecha.Day == end.Day && x.Campo == campo).Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                else if (!string.IsNullOrEmpty(campo) && inicio == null && fin == null)
                {
                    registrosTrabajador = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid && x.Campo == campo).Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                else if (inicio != null && fin != null && !string.IsNullOrEmpty(campo))
                {
                    DateTime end = Convert.ToDateTime(fin);
                    end = end.AddDays(1);
                    registrosTrabajador = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid && x.Fecha >= inicio && x.Fecha <= end && x.Campo == campo).Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }

                //registrosTrabajador = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid).ToList();
                //Diccionario para guardar los registros por fecha de cada persona
                Dictionary<string, List<RegistroPDF>> registrosDia = new Dictionary<string, List<RegistroPDF>>();
                //Se ordenan por fecha
                registrosTrabajador = registrosTrabajador.OrderBy(x => x.Fecha).ToList();
                //Si tiene registros, se ordenan en el diccionario por fecha
                if (registrosTrabajador.Count > 1)
                {
                    foreach (var registrito in registrosTrabajador)
                    {
                        if (registrosDia.ContainsKey(registrito.Fecha.ToShortDateString()))
                        {
                            registrosDia[registrito.Fecha.ToShortDateString()].Add(registrito);
                        }
                        else
                        {
                            registrosDia.Add(registrito.Fecha.ToShortDateString(), new List<RegistroPDF> { registrito });
                        }
                    }
                }
                //Se busca el primero y el último para determinar las horas trabajadas
                foreach (var registritos in registrosDia.Values)
                {
                    var primero = registritos.First();
                    var segundo = registritos.Last();
                    if (registritos.Count > 1)
                    {
                        DateTime horas = new DateTime();
                        DateTime horasExtras = new DateTime();
                        DateTime atraso = new DateTime();
                        DateTime atrasoP = new DateTime();
                        DateTime extra = new DateTime();
                        DateTime t2 = new DateTime();
                        DateTime entradaAlmuerzo = new DateTime();
                        DateTime salidaAlmuerzo = new DateTime();
                        DateTime almuerzo = new DateTime();
                        //Comprueba si tiene asignada una entrada
                        if (!String.IsNullOrEmpty(trabajador.Entrada))
                        {
                            DateTime.TryParse(primero.Fecha.ToShortDateString() + " " + trabajador.Entrada, out atraso);
                        }
                        //Si no, pone la general
                        else
                        {
                            DateTime.TryParse(primero.Fecha.ToShortDateString() + " " + Utils.SessionManager.entrada + ":00", out atraso);
                        }
                        //Verifica que llegó después de la hora
                        if (primero.Fecha > atraso)
                        {
                            atrasoP = atrasoP + (primero.Fecha - atraso);
                        }
                        //Si el primero es realmente primero
                        if (segundo.Fecha > primero.Fecha)
                        {
                            //Si tiene salida
                            if (!String.IsNullOrEmpty(trabajador.SalidaA))
                            {
                                DateTime.TryParse(segundo.Fecha.ToShortDateString() + " " + trabajador.SalidaA, out extra);
                            }
                            else
                            {
                                DateTime.TryParse(segundo.Fecha.ToShortDateString() + " " + Utils.SessionManager.salida + ":00", out extra);
                            }
                            //Ver si salió después de la hora, el segundo debe ser más grande
                            if (extra < segundo.Fecha)
                            {
                                if (!String.IsNullOrEmpty(trabajador.SalidaA) || !String.IsNullOrEmpty(Utils.SessionManager.salida))
                                    horasExtras = horasExtras + (segundo.Fecha - extra);
                            }
                            if (!String.IsNullOrEmpty(trabajador.Entrada))
                            {
                                DateTime.TryParse(primero.Fecha.ToShortDateString() + " " + trabajador.Entrada, out t2);
                            }
                            else
                            {
                                DateTime.TryParse(primero.Fecha.ToShortDateString() + " " + Utils.SessionManager.entrada + ":00", out t2);
                            }
                            //Ve si llegó antes de la hora para no sumar demás
                            if (0 > DateTime.Compare(primero.Fecha, t2) && 0 > DateTime.Compare(t2, segundo.Fecha))
                            {
                                if (extra > segundo.Fecha)
                                {
                                    horas = horas + (segundo.Fecha - t2);
                                }
                                else
                                {
                                    horas = horas + (extra - t2);
                                }
                            }
                            else
                            {
                                if (extra > primero.Fecha)
                                    horas = horas + (extra - primero.Fecha);
                                else
                                    horas = horas + (primero.Fecha - extra);
                            }
                        }
                        else
                        {
                            //Horario nocturno, revisar
                            horas = horas + (segundo.Fecha - primero.Fecha);
                        }
                        if (!String.IsNullOrEmpty(trabajador.Salida) && !String.IsNullOrEmpty(trabajador.EntradaA))
                        {
                            DateTime.TryParse("01/01/0001 " + trabajador.Salida, out entradaAlmuerzo);
                            DateTime.TryParse("01/01/0001 " + trabajador.EntradaA, out salidaAlmuerzo);
                            try
                            {
                                almuerzo = almuerzo + (salidaAlmuerzo - entradaAlmuerzo);
                            }
                            catch
                            {
                                DateTime.TryParse("01/01/0001 00:00", out almuerzo);
                            }
                        }
                        else
                        {
                            DateTime.TryParse("01/01/0001 " + Utils.SessionManager.almuerzo + ":00", out almuerzo);
                        }
                        DateTime horasT = new DateTime();
                        DateTime horasQL = new DateTime();
                        string raro = "";
                        DateTime salida = new DateTime();
                        DateTime.TryParse("01/01/0001 " + segundo.Fecha.Hour + ":" + segundo.Fecha.Minute, out salida);
                        if (almuerzo < horas && salidaAlmuerzo < salida)

                            horasQL = horasQL + (horas - almuerzo);
                        else
                            horasQL = horas;
                        horasT = horasQL;
                        /*if (atrasoP < horasQL)
                        {
                            horasT = horasT + (horasQL - atrasoP);
                        }
                        else
                        {
                            horasT = horasQL;
                            raro = "*";
                        }*/
                        DateTime horasTrabajadas = new DateTime();
                        if (horasT > horasExtras)
                        {
                            horasTrabajadas = horasT.AddHours(horasExtras.Hour);
                            horasTrabajadas = horasT.AddMinutes(horasExtras.Minute);
                            horasTrabajadas = horasT.AddSeconds(horasExtras.Second);
                        }

                        Registro registro = new Registro
                        {
                            Uid = trabajador.Uid,
                            Rut = trabajador.Rut,
                            Nombre = trabajador.Nombre,
                            ApellidoPaterno = trabajador.ApellidoPaterno,
                            ApellidoMaterno = trabajador.ApellidoMaterno,
                            Fecha = segundo.Fecha,
                            Entrada = primero.Fecha.ToShortTimeString(),
                            EntradaModificada = primero.Modificado,
                            CausaEntrada = primero.Causa,
                            EntradaOficial = atraso.ToShortTimeString(),
                            Salida = segundo.Fecha.ToShortTimeString(),
                            SalidaModificada = segundo.Modificado,
                            CausaSalida = segundo.Causa,
                            SalidaOficial = extra.ToShortTimeString(),
                            Atraso = atrasoP.ToShortTimeString(),
                            Campo = primero.Campo,
                            //Horas totales
                            Horas = horasT.ToShortTimeString() + raro,
                            Contratado = trabajador.Contratado

                        };
                        registrosLista.Add(registro);
                        if (rankings.ContainsKey(trabajador))
                        {
                            rankings[trabajador].Atraso = rankings[trabajador].Atraso.AddHours(atrasoP.Hour);
                            rankings[trabajador].Atraso = rankings[trabajador].Atraso.AddMinutes(atrasoP.Minute);
                            rankings[trabajador].Atraso = rankings[trabajador].Atraso.AddSeconds(atrasoP.Second);
                            rankings[trabajador].HorasExtras = rankings[trabajador].HorasExtras.AddHours(horasExtras.Hour);
                            rankings[trabajador].HorasExtras = rankings[trabajador].HorasExtras.AddMinutes(horasExtras.Minute);
                            rankings[trabajador].HorasExtras = rankings[trabajador].HorasExtras.AddSeconds(horasExtras.Second);
                            rankings[trabajador].HorasTrabajadas = rankings[trabajador].HorasTrabajadas.AddHours(horasTrabajadas.Hour);
                            rankings[trabajador].HorasTrabajadas = rankings[trabajador].HorasTrabajadas.AddMinutes(horasTrabajadas.Minute);
                            rankings[trabajador].HorasTrabajadas = rankings[trabajador].HorasTrabajadas.AddSeconds(horasTrabajadas.Second);
                        }
                        else
                        {
                            DateTime Atraso = new DateTime();
                            DateTime HorasExtras = new DateTime();
                            DateTime HorasTrabajadas = new DateTime();
                            Atraso = Atraso.AddHours(atrasoP.Hour);
                            Atraso = Atraso.AddMinutes(atrasoP.Minute);
                            Atraso = Atraso.AddSeconds(atrasoP.Second);
                            HorasExtras = HorasExtras.AddHours(horasExtras.Hour);
                            HorasExtras = HorasExtras.AddMinutes(horasExtras.Minute);
                            HorasExtras = HorasExtras.AddSeconds(horasExtras.Second);
                            HorasTrabajadas = HorasTrabajadas.AddHours(horasTrabajadas.Hour);
                            HorasTrabajadas = HorasTrabajadas.AddMinutes(horasTrabajadas.Minute);
                            HorasTrabajadas = HorasTrabajadas.AddSeconds(horasTrabajadas.Second);
                            Ranking ranking = new Ranking { Atraso = Atraso, HorasExtras = HorasExtras, HorasTrabajadas = HorasTrabajadas };
                            rankings.Add(trabajador, ranking);
                        }

                        if (diasTrabajados.ContainsKey(trabajador))
                        {
                            diasTrabajados[trabajador].Add(primero.Fecha);
                        }
                        else
                        {
                            diasTrabajados.Add(trabajador, new List<DateTime> { primero.Fecha });
                        }
                    }
                }
            }
            return registrosLista.OrderBy(o => o.Fecha).ToList();
        }
        public void PDFConsolidado()
        {
            SgajcpEntities database = new SgajcpEntities();
            //var workers = database.RegistroTrabajador.Where(x => x.Uid == "63000000").ToList();
            //foreach (var registry in workers)
            //{

            //    database.RegistroTrabajador.Remove(registry);
            //    var ms = new MemoryStream(registry.Foto);
            //    var image = System.Drawing.Image.FromStream(ms);
            //    ms = new MemoryStream();
            //    image.Save(ms, ImageFormat.Jpeg);
            //    registry.Foto = ms.ToArray();
            //    database.SaveChanges();
            //}

            string Empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            List<TrabajadorIndex> trabajadores = database.Trabajador.Where(x => x.Empresa == Empresa)
                                                .Select(x => new TrabajadorIndex
                                                {
                                                    Id = x.Id,
                                                    Nombre = x.Nombre,
                                                    ApellidoMaterno = x.ApellidoMaterno,
                                                    ApellidoPaterno = x.ApellidoPaterno,
                                                    Rut = x.Rut,
                                                    Uid = x.Uid
                                                }).ToList();
            //List<RegistroTrabajador> registrosTrabajador = new List<RegistroTrabajador>();
            List<Registro> registrosLista = GetRegistroTrabajadores(trabajadores, null, null, null, null);
            trabajadores.Insert(0, new TrabajadorIndex() { Nombre = "" });
            if (Utils.SessionManager.tipo == "PDF")
            {
                GenerarPDFRegistros(trabajadores, null, "Consolidado", registrosLista);
            }
            else
            {
                //GenerarExcel(trabajadores, "Consolidado", registrosLista);
            }


        }
        public void PDFRegistro(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            TrabajadorIndex trabajador = database.Trabajador.Select(x => new TrabajadorIndex
            {
                Id = x.Id,
                Nombre = x.Nombre,
                ApellidoMaterno = x.ApellidoMaterno,
                ApellidoPaterno = x.ApellidoPaterno,
                Rut = x.Rut,
                Uid = x.Uid,
                Numero = x.Numero,
                Empresa = x.Empresa,
            }).FirstOrDefault(x => x.Id == id);
            if (trabajador == null)
                return;
            List<TrabajadorIndex> trabajadores = new List<TrabajadorIndex>
            {
                trabajador
            };
            List<Registro> registros = GetRegistroTrabajadores(trabajadores, null, null, null, null);

            if (Utils.SessionManager.tipo == "PDF")
            {
                GenerarPDFRegistros(trabajadores, null, "Registro", registros);
            }
            else
            {
                //GenerarExcel(trabajadores, "Registro", registros);
            }
        }
        public ActionResult PDFRegistroFecha()
        {
            List<TrabajadorIndex> trabajadores = Utils.SessionManager.trabajadores;
            List<Registro> registros = new List<Registro>();
            if (Utils.SessionManager.inicio.ToShortDateString() == "01-01-0001" && Utils.SessionManager.fin.ToShortDateString() == "01-01-0001" && !string.IsNullOrEmpty(Utils.SessionManager.campo))
            {
                registros = GetRegistroTrabajadores(trabajadores, null, null, Utils.SessionManager.campo, null);

            }
            if (Utils.SessionManager.inicio.ToShortDateString() != "01-01-0001" && Utils.SessionManager.fin.ToShortDateString() == "01-01-0001" && string.IsNullOrEmpty(Utils.SessionManager.campo))
            {
                registros = GetRegistroTrabajadores(trabajadores, Utils.SessionManager.inicio, null, null, null);

            }
            if (Utils.SessionManager.inicio.ToShortDateString() != "01-01-0001" && Utils.SessionManager.fin.ToShortDateString() != "01-01-0001" && string.IsNullOrEmpty(Utils.SessionManager.campo))
            {
                if (Utils.SessionManager.inicio.ToShortDateString() == Utils.SessionManager.fin.ToShortDateString())
                {
                    registros = GetRegistroTrabajadores(trabajadores, Utils.SessionManager.inicio, null, null, null);
                }
                else
                {
                    registros = GetRegistroTrabajadores(trabajadores, Utils.SessionManager.inicio, Utils.SessionManager.fin, null, null);
                }

            }
            if (Utils.SessionManager.inicio.ToShortDateString() != "01-01-0001" && Utils.SessionManager.fin.ToShortDateString() != "01-01-0001" && !string.IsNullOrEmpty(Utils.SessionManager.campo))
            {
                registros = GetRegistroTrabajadores(trabajadores, Utils.SessionManager.inicio, Utils.SessionManager.fin, Utils.SessionManager.campo, null);

            }
            if (Utils.SessionManager.inicio.ToShortDateString() != "01-01-0001" && Utils.SessionManager.fin.ToShortDateString() == "01-01-0001" && !string.IsNullOrEmpty(Utils.SessionManager.campo))
            {
                registros = GetRegistroTrabajadores(trabajadores, Utils.SessionManager.inicio, null, Utils.SessionManager.campo, null);

            }
            if (Utils.SessionManager.tipo == "PDF")
            {
                GenerarPDFRegistros(trabajadores, null, "Registro", registros);
            }
            else
            {
                //GenerarExcel(trabajadores, "Registro", registros);
            }
            return RedirectToAction("Index");
        }
        public string formatearRut(string rut)
        {
            // Si el RUT está vacío, retornar cadena vacía
            if (string.IsNullOrEmpty(rut))
            {
                return "";
            }

            // Eliminar puntos, guiones y espacios
            rut = rut.Replace(".", "");
            rut = rut.Replace("-", "");
            rut = rut.Replace(" ", "");
            rut = rut.Replace("\t", "");

            // Verificar que haya caracteres después de la limpieza
            if (rut.Length <= 1)
            {
                return rut;
            }

            // Obtener el dígito verificador (último carácter)
            string dv = rut.Substring(rut.Length - 1);

            // Obtener el número del RUT (sin dígito verificador)
            string numero = rut.Substring(0, rut.Length - 1);

            // Formatear el número con puntos cada tres dígitos desde la derecha
            string numeroFormateado = "";
            int contador = 0;

            for (int i = numero.Length - 1; i >= 0; i--)
            {
                contador++;
                numeroFormateado = numero[i] + numeroFormateado;

                // Agregar punto después de cada tercer dígito, excepto al final
                if (contador == 3 && i > 0)
                {
                    numeroFormateado = "." + numeroFormateado;
                    contador = 0;
                }
            }

            // Retornar el RUT formateado
            return numeroFormateado + "-" + dv;
        }
        public bool validarRut(string rut)
        {
            bool validacion = false;
            try
            {
                rut = rut.ToUpper();
                rut = rut.Replace(".", "");
                rut = rut.Replace("-", "");
                rut = rut.Replace(" ", "");
                int rutAux = int.Parse(rut.Substring(0, rut.Length - 1));

                char dv = char.Parse(rut.Substring(rut.Length - 1, 1));

                int m = 0, s = 1;
                for (; rutAux != 0; rutAux /= 10)
                {
                    s = (s + rutAux % 10 * (9 - m++ % 6)) % 11;
                }
                if (dv == (char)(s != 0 ? s + 47 : 75))
                {
                    validacion = true;
                }
            }
            catch (Exception)
            {
            }
            return validacion;
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
            int count = 1;
            nombres[0] = "Todos";
            foreach (var campo in campos)
            {
                nombres[count] = campo.Nombre;
                count++;
            }
            return nombres;
        }

        public byte[] getImage()
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(Server.MapPath("~/App_Data/Foto.png"));
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
        // GET: Trabajador
        public ActionResult Index(int? id)
        {
            DeshabilitarExpirados();
            Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();
            string empresa = cuenta.Empresa;
            List<TrabajadorIndex> trabajadores = new List<TrabajadorIndex>();
            if (empresa == "VSP")
            {
                trabajadores = db.Trabajador.Select(x => new TrabajadorIndex
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    ApellidoPaterno = x.ApellidoPaterno,
                    ApellidoMaterno = x.ApellidoMaterno,
                    Rut = x.Rut,
                    Uid = x.Uid,
                    Numero = x.Numero,
                    Empresa = x.Empresa,
                    Contratista = x.Contratista
                }).OrderBy(x => x.ApellidoPaterno).ToList();

            }
            else
            {

                trabajadores = db.Trabajador.Select(x => new TrabajadorIndex
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    ApellidoPaterno = x.ApellidoPaterno,
                    ApellidoMaterno = x.ApellidoMaterno,
                    Rut = x.Rut,
                    Uid = x.Uid,
                    Numero = x.Numero,
                    Empresa = x.Empresa
                }).Where(x => x.Empresa == cuenta.Empresa).ToList();
            }
            return View(trabajadores);
        }
        // GET: Trabajador/Details/5
        public ActionResult Details(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Trabajador trabajador = database.Trabajador.FirstOrDefault(x => x.Id == id);
            if (trabajador == null)
                return HttpNotFound();
            if (Utils.SessionManager.alerta != 0)
            {
                ViewBag.alerta = Utils.SessionManager.alerta;
                Utils.SessionManager.alerta = 0;
            }
            return View(trabajador);
        }
        // GET: Trabajador/Create
        private void datosCreate()
        {
            SgajcpEntities database = new SgajcpEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            var campos = database.Campos.Select(x => new { x.Nombre, x.Empresa, x.Encargado }).Where(x => x.Empresa == empresa).ToList();
            int count = 0;
            string[] jefes = new string[campos.Count];
            string[] nombres = new string[campos.Count];
            foreach (var campo in campos)
            {
                nombres[count] = campo.Nombre;
                jefes[count] = campo.Encargado;
                count++;
            }

            ViewBag.jefes = jefes.Distinct().ToList();
            ViewBag.Campos = nombres;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CheckRut(string rut)
        {
            // Aquí deberías implementar la lógica para verificar si el RUT ya existe en tu lista
            // Puedes usar ViewBag.trabajadores o llamar a un servicio/repositorio para obtener la lista

            // Supongamos que existe una lista llamada trabajadores
            List<string> trabajadores = db.Trabajador.Select(x => x.Rut).ToList();

            // Verificar si el RUT ya existe
            bool isRutAvailable = !trabajadores.Contains(rut);

            // Devolver el resultado como un objeto JSON
            return Json(new { isAvailable = isRutAvailable });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CheckPulsera(string numero)
        {
            // Aquí deberías implementar la lógica para verificar si el RUT ya existe en tu lista
            // Puedes usar ViewBag.trabajadores o llamar a un servicio/repositorio para obtener la lista

            // Supongamos que existe una lista llamada trabajadores
            List<string> pulseras = db.Trabajador.Select(x => x.Numero).ToList();

            // Verificar si el RUT ya existe
            bool isRutAvailable = !pulseras.Contains(numero);

            // Devolver el resultado como un objeto JSON
            return Json(new { isAvailable = isRutAvailable });
        }
        public ActionResult Create()
        {
            SgajcpEntities database = new SgajcpEntities();
            ViewBag.empresas = db.Empresas.Select(x => x.Nombre).OrderBy(x => x).ToList();
            Trabajador trabajador = new Trabajador();
            ViewBag.ruts = db.Trabajador.Select(x => x.Rut).ToList();
            ViewBag.pulseras = db.Trabajador.Select(x => x.Numero).ToList();
            return View(trabajador);
        }

        // POST: Trabajador/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection, HttpPostedFileBase file,
            [Bind(Include = "Id,Nombre,ApellidoPaterno,ApellidoMaterno,Habilitado,Rut,Nacionalidad,Expiración,Direccion,Sexo,Ciudad,Telefono,Email,Numero,Empresa")] Trabajador trabajadorNew)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                ViewBag.empresas = db.Empresas.Select(x => x.Nombre).OrderBy(x => x).ToList();
                trabajadorNew.Rut = formatearRut(trabajadorNew.Rut);
                HttpPostedFileBase postedFile = Request.Files["Foto"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    trabajadorNew.FotoCarnet = getImageFromPostfile(postedFile, 850);
                    ViewBag.foto = Request.Files["Foto"];
                }
                postedFile = Request.Files["FotoCarnet"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    trabajadorNew.Foto = getImageFromPostfile(postedFile, 850);
                    ViewBag.fotocarnet = Request.Files["FotoCarnet"];
                }
                if (trabajadorNew.Habilitado != true)
                {
                    trabajadorNew.Habilitado = false;
                }
                /*
                if (!validarRut(trabajadorNew.Rut))
                {
                    ViewBag.texto = "Rut no válido";
                    datosCreate();
                    return View(trabajadorNew);
                }/*
                Trabajador trabajador = null;
                try
                {
                    trabajador = database.Trabajador.First(x => x.Rut == trabajadorNew.Rut);
                    ViewBag.texto = "Ya existe el rut";
                    datosCreate();
                    return View(trabajadorNew);
                }
                catch { }*/
                database.Trabajador.Add(trabajadorNew);
                database.SaveChanges();
                postedFile = Request.Files["Nomina"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Nomina = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Contrato"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Contrato = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Anexo"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Anexo = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Odi"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Odi = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Registro_Epp"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Registro_Epp = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Registro_RIOHS"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Registro_RIOHS = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Registro_capacitación"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Registro_capacitación = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Examen_altura_física"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Examen_altura_física = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Procedimientos_de_trabajo"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Procedimientos_de_trabajo = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Documento_Covid_19"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Documento_Covid_19 = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                Utils.SessionManager.log("Trabajador crear: " + trabajadorNew.Nombre + " " + trabajadorNew.ApellidoPaterno + " " + trabajadorNew.ApellidoMaterno);
                if (!string.IsNullOrEmpty(trabajadorNew.Numero))
                {
                    string numero = trabajadorNew.Numero;
                    try
                    {
                        trabajadorNew.Uid = db.Pulseras.First(x => x.Numero == numero).Uid;
                    }
                    catch
                    {

                    }
                    Trabajador trabajador1 = null;
                    try
                    {
                        trabajador1 = database.Trabajador.First(x => x.Numero == numero);
                    }
                    catch { }
                    if (trabajador1 != null && trabajador1.Rut != trabajadorNew.Rut && trabajadorNew.Uid != null)
                    {
                        trabajador1.Numero = trabajador1.Id.ToString();
                        trabajador1.Uid = trabajador1.Id.ToString();
                        db.Entry(trabajador1).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    db.Entry(trabajadorNew).State = EntityState.Modified;
                    db.SaveChanges();

                }

                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                return View();
            }
        }
        // GET: Trabajador/Edit/5
        public ActionResult Edit(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            ViewBag.empresas = db.Empresas.Select(x => x.Nombre).OrderBy(x => x).ToList();
            Trabajador trabajadorNew = database.Trabajador.FirstOrDefault(x => x.Id == id);
            if (trabajadorNew == null)
                return HttpNotFound();
            if (trabajadorNew.FotoCarnet != null)
            {
                Utils.SessionManager.FotoCarnet = trabajadorNew.FotoCarnet;
            }
            if (trabajadorNew.Foto != null)
            {
                Utils.SessionManager.Foto = trabajadorNew.Foto;
            }
            return View(trabajadorNew);
        }
        // POST: Trabajador/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, FormCollection collection,
                [Bind(Include = "Id,Nombre,ApellidoPaterno,ApellidoMaterno,Habilitado,Rut,Uid,Nacionalidad,Direccion,Sexo,Ciudad,Telefono,Email,Numero,Empresa" +
                                "Nomina,Contrato,Anexo,Odi,Registro_Epp,Registro_RIOHS,Registro_capacitación,Expiración,Examen_altura_física,Procedimientos_de_trabajo,Documento_Covid_19")]
                                Trabajador trabajadorNew)
        {
            ViewBag.empresas = db.Empresas.Select(x => x.Nombre).OrderBy(x => x).ToList();
            try
            {
                SgajcpEntities database = new SgajcpEntities();

                trabajadorNew.Rut = formatearRut(trabajadorNew.Rut);
                trabajadorNew.Empresa = collection["Empresa"];
                string numero = trabajadorNew.Numero;
                string uid = db.Pulseras.First(x => x.Numero == numero).Uid;
                Trabajador trabajador1 = null;
                try
                {
                    trabajador1 = database.Trabajador.First(x => x.Numero == trabajadorNew.Numero);
                }
                catch { }
                if (trabajador1 != null && trabajador1.Id != trabajadorNew.Id)
                {
                    trabajador1.Numero = trabajador1.Id.ToString();
                    trabajador1.Uid = trabajador1.Id.ToString();
                    db.Entry(trabajador1).State = EntityState.Modified;
                    db.SaveChanges();
                }
                trabajadorNew.Uid = uid;
                if (trabajadorNew.FotoCarnet == null && Utils.SessionManager.FotoCarnet != null)
                {
                    trabajadorNew.FotoCarnet = Utils.SessionManager.FotoCarnet;
                    Utils.SessionManager.FotoCarnet = null;
                }
                HttpPostedFileBase postedFile = Request.Files["FotoCarnet"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    trabajadorNew.FotoCarnet = getImageFromPostfile(postedFile, 850);
                }

                if (trabajadorNew.Foto == null && Utils.SessionManager.Foto != null)
                {
                    trabajadorNew.Foto = Utils.SessionManager.Foto;
                    Utils.SessionManager.Foto = null;
                }
                postedFile = Request.Files["Foto"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    trabajadorNew.Foto = getImageFromPostfile(postedFile, 850);
                }
                if (trabajadorNew.Habilitado != true)
                {
                    trabajadorNew.Habilitado = false;
                }
                postedFile = Request.Files["Nomina1"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Nomina = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Contrato1"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Contrato = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Anexo1"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Anexo = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Odi1"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Odi = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Registro_Epp1"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Registro_Epp = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Registro_RIOHS1"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Registro_RIOHS = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Registro_capacitación1"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Registro_capacitación = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Examen_altura_física1"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Examen_altura_física = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Procedimientos_de_trabajo1"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Procedimientos_de_trabajo = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                postedFile = Request.Files["Documento_Covid_191"];
                if (postedFile != null && postedFile.ContentLength > 0)
                    trabajadorNew.Documento_Covid_19 = GuardarArchivoSeguro(postedFile, trabajadorNew.Id);
                if (!validarRut(trabajadorNew.Rut))
                {
                    ViewBag.texto = "Rut no válido";
                    //return View(trabajadorNew);
                }
                try
                {
                    Trabajador trabajador = database.Trabajador.First(x => x.Rut == trabajadorNew.Rut);
                    if (trabajador.Id != trabajadorNew.Id)
                    {
                        //ViewBag.texto = "Ya existe el rut";
                        //return View(trabajadorNew);
                    }
                }
                catch { }
                if (ModelState.IsValid)
                {
                    db.Entry(trabajadorNew).State = EntityState.Modified;
                    Utils.SessionManager.log("Trabajador editar: " + trabajadorNew.Nombre + " " + trabajadorNew.ApellidoPaterno + " " + trabajadorNew.ApellidoMaterno);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                //database.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                return View();
            }
        }
        public byte[] getImageFromPostfile(HttpPostedFileBase postedFile, int tamaño)
        {
            MemoryStream target = new MemoryStream();
            System.Drawing.Image image = System.Drawing.Image.FromStream(postedFile.InputStream, true, true);
            SizeF dimensiones = image.PhysicalDimension;
            for (int i = 100; i > 0; i--)
            {
                float dimension = (dimensiones.Width * i) / 100;
                if (dimension <= tamaño)
                {
                    int ancho = (int)dimensiones.Width * i / 100;
                    int alto = (int)dimensiones.Height * i / 100;
                    image = resizeImage(image, new Size(ancho, alto));
                    i = 0;
                }
            }
            image.Save(target, ImageFormat.Jpeg);
            //postedFile.InputStream.CopyTo(target);
            return target.ToArray();
        }
        // GET: Trabajador/Delete/5
        public ActionResult Delete(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Trabajador trabajador = database.Trabajador.FirstOrDefault(x => x.Id == id);
            if (trabajador == null)
                return HttpNotFound();
            return View(trabajador);
        }

        // POST: Trabajador/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Trabajador trabajador = database.Trabajador.FirstOrDefault(x => x.Id == id);
            if (trabajador == null)
                return HttpNotFound();
            Utils.SessionManager.log("Trabajador eliminado: " + trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno);
            database.Trabajador.Remove(trabajador);
            database.SaveChanges();
            return RedirectToAction("Index");
        }
        private string cambiarUidCrear(string Uid)
        {
            SgajcpEntities database = new SgajcpEntities();
            Empresas empresa = database.Empresas.First(x => x.Id == 1005);
            List<RegistroPDF> registros = database.RegistroTrabajador
                                         .Where(x => x.Uid == Uid)
                                         .Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid }).ToList();

            Trabajador trabajador = null;
            try
            {
                trabajador = database.Trabajador.First(x => x.Uid == Uid);
            }
            catch { }
            if (trabajador != null)
            {
                trabajador.Uid = empresa.Nombre;
                if (registros.Count > 0)
                {
                    cambiarUidRegistros(empresa.Nombre, Uid);
                }
                empresa.Nombre = ((Convert.ToInt32(empresa.Nombre)) + 1).ToString();
                database.SaveChanges();
            }
            return Uid;
        }
        private string cambiarUidEditar(string UidNuevo, string UidAnterior)
        {

            return UidNuevo;
        }
        private string cambiarUid(string Uid, string Uid2)
        {
            SgajcpEntities database = new SgajcpEntities();
            List<Empresas> nombre = database.Empresas.Where(x => x.Id == 1005).ToList();
            var registros = database.RegistroTrabajador.Select(x => new { x.Uid }).Where(x => x.Uid == Uid).Distinct().ToList();
            var trabajadores = database.Trabajador.Select(x => new { x.Uid }).Where(x => x.Uid == Uid).Distinct().ToList();
            bool exist = false;
            string result = "";
            if (trabajadores.Count > 0)
            {
                var trabajador = database.Trabajador.Where(x => x.Uid == Uid).ToList();
                trabajador[0].Uid = nombre[0].Nombre;
                database.SaveChanges();
                exist = true;
                result = Uid;
            }
            else
            {
                //var trabajador = //database.Trabajador.Where(x => x.Uid == Uid2).ToList();
                //trabajador[0].Uid = Uid;
                //database.SaveChanges();
                return Uid;
            }
            if (registros.Count > 0)
            {
                var registers = database.RegistroTrabajador.Where(x => x.Uid == Uid).ToList();
                foreach (var registro in registers)
                {
                    registro.Uid = nombre[0].Nombre;
                }
                database.SaveChanges();
                exist = true;
            }
            else
            {
                var registers = database.RegistroTrabajador.Where(x => x.Uid == Uid2).ToList();
                foreach (var registro in registers)
                {
                    registro.Uid = Uid;
                }
                database.SaveChanges();
            }
            if (exist)
            {
                int numero = Convert.ToInt32(nombre[0].Nombre);
                nombre[0].Nombre = (numero + 1).ToString();
                database.SaveChanges();
            }
            return result;
        }

        private string cambiarUidRegistros(string UidNuevo, string UidAterior)
        {
            SgajcpEntities database = new SgajcpEntities();
            try
            {
                Trabajador trabajador = database.Trabajador.First(x => x.Uid == UidNuevo);
                if (trabajador != null)
                {
                    cambiarUidCrear(UidNuevo);
                }
            }
            catch { }

            List<RegistroTrabajador> registros = database.RegistroTrabajador.Where(x => x.Uid == UidAterior).ToList();
            foreach (var registro in registros)
            {
                registro.Uid = UidNuevo;
            }
            database.SaveChanges();
            return UidNuevo;

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}