
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

namespace ControlPersonalAppWeb.Controllers
{
    public class TrabajadorController : Controller
    {

        DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();
        Dictionary<TrabajadorIndex, Ranking> rankings = new Dictionary<TrabajadorIndex, Ranking>();
        Dictionary<TrabajadorIndex, List<DateTime>> diasTrabajados = new Dictionary<TrabajadorIndex, List<DateTime>>();
        public ActionResult Borrar()
        {
            return RedirectToAction("Index");
        }
        public ActionResult Cargar()
        {
            Utils.SessionManager.log("Cargar trabajadores, ¡Nadie debe estar aquí!");
            DBManejoPersonalEntities db = new DBManejoPersonalEntities();
            string empresa = ControlPersonalAppWeb.Utils.SessionManager.CuentaAutenticada().Empresa;
            var empresas = db.Empresas.Select(x => x.Nombre).ToList(); ;
            if(empresa != "JCP")
            {
                 empresas = db.Empresas.Where(x => x.Nombre == empresa).Select(x => x.Nombre).ToList();
            }
            ViewBag.Empresas = empresas;
            return View();
        }
        [HttpPost]
        public ActionResult Cargar(FormCollection collection)
        {
            Utils.SessionManager.log("Cargar trabajadores, ¡Nadie debe estar aquí!");
            DBManejoPersonalEntities db = new DBManejoPersonalEntities();
            string empresa = collection["empresa"];
            string mensaje = "";
            HttpPostedFileBase hpf = Request.Files["csv"];
            if (hpf != null && hpf.ContentLength > 0)
            {
                StreamReader csvreader = new StreamReader(hpf.InputStream);
                var line1 = csvreader.ReadLine();
                string rut = "";
                while (!csvreader.EndOfStream)
                {
                    try
                    {
                        var line = csvreader.ReadLine();
                        var values = line.Split(';');
                        if (!String.IsNullOrEmpty(values[1]))
                        {
                            rut = values[1];
                            Trabajador trabajador = new Trabajador();
                            trabajador.CodPersonal = values[0];
                            trabajador.Rut = formatearRut(values[1]);
                            trabajador.ApellidoPaterno = values[2];
                            trabajador.ApellidoMaterno = values[3];
                            trabajador.Nombre = values[4];
                            try
                            {
                                trabajador.FechaNacimiento = DateTime.ParseExact(values[5], "dd-mm-yyyy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            trabajador.Direccion = values[7];
                            trabajador.Ciudad = values[8];
                            trabajador.Telefono = values[9];
                            try
                            {
                                trabajador.FechaIngreso = DateTime.ParseExact(values[10], "dd-mm-yyyy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            trabajador.Campo = values[12];
                            trabajador.Contrato = values[14];
                            trabajador.Empresa = empresa;
                            trabajador.Jornada = values[17] +" "+values[18];
                            trabajador.Gerente = values[19];
                            trabajador.Uid = values[20];
                            if (values[6] == "SOLTERO/A")
                            {
                                trabajador.EstadoCivil = "Soltero/a";
                            }
                            else if (values[6] == "CASADO/A")
                            {
                                trabajador.EstadoCivil = "Casado/a";
                            }
                            else if (values[6] == "VIUDO/SEPARADO")
                            {
                                trabajador.EstadoCivil = "Viudo/Separado";

                            }
                            if (values[11] == "M")
                            {
                                trabajador.Sexo = "Masculino";
                            }
                            else if (values[11] == "F")
                            {
                                trabajador.Sexo = "Femenino";
                            }
                            try
                            {
                                if (Convert.ToInt32(values[13]) == 1)
                                {
                                    trabajador.Contratado = "Si";
                                }
                                else
                                {
                                    trabajador.Contratado = "No";
                                }
                            }
                            catch
                            { }
                            db.Trabajador.Add(trabajador);
                        }
                    }
                    catch (Exception e)
                    {
                        mensaje = mensaje + " Rut: "+rut+" Error: " + e.Message;

                    }
                }
                if(String.IsNullOrEmpty(mensaje))
                {
                    ViewBag.mensaje = "Cargado correctamente ";
                }
                else
                {
                    ViewBag.mensaje = "Cargado con errores, no se pudo cargar: " + mensaje;
                }try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        ViewBag.mensaje =  ViewBag.mensaje + "Entity of type "+ eve.Entry.Entity.GetType().Name+" in state "+eve.Entry.State+" has the following validation errors:";
                        foreach (var ve in eve.ValidationErrors)
                        {
                            ViewBag.mensaje = ViewBag.mensaje + "- Property: "+ ve.PropertyName + ", Error: "+ ve.ErrorMessage;
                        }
                    }
                }
                csvreader.Close();
            }

            string empresita = ControlPersonalAppWeb.Utils.SessionManager.CuentaAutenticada().Empresa;
            var empresas = db.Empresas.Select(x => x.Nombre).ToList(); ;
            if (empresa != "JCP")
            {
                empresas = db.Empresas.Where(x => x.Nombre == empresita).Select(x => x.Nombre).ToList();
            }
            ViewBag.Empresas = empresas;
            return View();
        }
        private void AsistenciasPDF(DateTime dateTime, string[] ids, Empresas empresa)
        {
            DBManejoPersonalEntities db = new DBManejoPersonalEntities();
            string strHeader = "Asistencia mensual\n" + dateTime.ToString("MMMM yyyy");//Convert.ToDateTime(collection["Periodo"]).ToLongDateString();
            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            Document document = new Document();
            document.SetPageSize(iTextSharp.text.PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, Response.OutputStream);
            document.Open();
            foreach (var idd in ids)
            {
                int id = Convert.ToInt32(idd);

                List<TrabajadorIndex> trabajadores = db.Trabajador.Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa,
                    Entrada = x.Entrada,
                    EntradaA = x.EntradaA,
                    Salida = x.Salida,
                    SalidaA = x.SalidaA
                }).Where(x => x.Id == id).ToList();
                List<Registro> registrosLista = new List<Registro>();
                registrosLista = GetRegistroTrabajadores(trabajadores, null, null, null, dateTime);
                if( registrosLista.Count == 0)
                {
                    continue;
                }
                TrabajadorIndex trabajador = trabajadores[0];

                BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntCell = new Font(btnColumnHeader, 12);


                Image image = Image.GetInstance(Server.MapPath("~/App_Data/" + Utils.SessionManager.CuentaAutenticada().Empresa + ".png"));
                image.Alignment = Element.ALIGN_LEFT;
                image.ScaleToFit(60, 60);
                document.Add(image);

                //Report Header
                BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntHead = new Font(bfntHead, 16, 1, BaseColor.DARK_GRAY);

                Paragraph p = new Paragraph(new Chunk("\n"));
                if (!string.IsNullOrEmpty(trabajadores[0].Rut))
                {
                    //Add a line seperation

                    fntHead.Size = 14;
                    document.Add(new Paragraph("Nombre: " + trabajadores[0].Nombre + " " + trabajadores[0].ApellidoPaterno + " " + trabajadores[0].ApellidoMaterno, fntHead));
                    document.Add(new Paragraph("Rut: " + trabajadores[0].Rut, fntHead));
                    if (!String.IsNullOrEmpty(Utils.SessionManager.entrada))
                        document.Add(new Paragraph("Inicio actividades: " + Utils.SessionManager.entrada, fntHead));
                }
                //Add line break
                document.Add(p);

                //Write the table
                int num = 5;
                //Table header
                Font fntColumnHeader = new Font(btnColumnHeader, 12, 1, BaseColor.WHITE);
                List<string> titulos = new List<string>();
                titulos.Add("Fecha");
                titulos.Add("Entrada");
                titulos.Add("Salida");
                titulos.Add("H. Periodo");
                titulos.Add("H. Extras");
                titulos.Add("H. Total");
                titulos.Add("Predio");
                int i;
                PdfPTable table = new PdfPTable(titulos.Count);
                table.WidthPercentage = 100f;
                for (i = 0; i < titulos.Count; i++)
                {
                    PdfPCell cell = new PdfPCell();
                    cell.BackgroundColor = BaseColor.GRAY;
                    cell.AddElement(new Chunk(titulos[i], fntColumnHeader));
                    table.AddCell(cell);
                }
                //table Data
                foreach (var celda in registrosLista)
                {
                    DateTime horas;
                    DateTime extra;
                    DateTime.TryParse("01/01/0001 " + celda.Horas, out horas);
                    DateTime.TryParse("01/01/0001 " + celda.HorasExtras, out extra);

                    table.AddCell(new Phrase(celda.Fecha.ToShortDateString(), fntCell));
                    table.AddCell(new Phrase(celda.Entrada, fntCell));
                    table.AddCell(new Phrase(celda.Salida, fntCell));
                    table.AddCell(new Phrase(celda.Horas, fntCell));
                    if(horas.Hour>=8 && extra.Hour>=2 && extra.Minute>0)
                    {
                        table.AddCell(new Phrase("02:00", fntCell));
                        horas = horas.AddHours(2);
                    }
                    else if (horas.Hour >7 && horas.Hour < 8 && extra.Hour >= 1 && extra.Minute > 40)
                    {
                        table.AddCell(new Phrase("01:40", fntCell));
                        horas = horas.AddHours(2);
                        horas = horas.AddMinutes(40);
                    }
                    else
                    {
                        table.AddCell(new Phrase(celda.HorasExtras, fntCell));
                        horas = horas.AddHours(extra.Hour);
                        horas = horas.AddMinutes(extra.Minute);
                        horas = horas.AddSeconds(extra.Second);
                    }
                    table.AddCell(new Phrase(horas.ToShortTimeString(), fntCell));
                    table.AddCell(new Phrase(celda.Campo, fntCell));
                }

                document.Add(table);
                document.Add(p);
                PdfPTable declaracion = new PdfPTable(1);
                declaracion.WidthPercentage = 100f;
                string texto = "Yo " + trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " +
                                trabajador.ApellidoMaterno + " Rut " +
                                formatearRut(trabajador.Rut) + " declaro  y acepto que he trabajado todas" +
                                " los dias mostrados anteriormente" +
                                " en el perido de " + dateTime.ToString("MMMM yyyy") + ", en la empresa " + empresa.Nombre
                                + " " + empresa.Rut +
                    ", a mi entera satisfacción, " +
                    " no tengo cargos ni cobros posteriores asimismo acepto y reconozco " +
                    " la forma en como se determinó y las deducciones efectuadas.\n\n\n\n" +
                    "                                                               ______________________\n\n" +
                    "                                                                      Recibí conforme";
                declaracion.AddCell(new Paragraph (texto, fntCell) { Alignment = Element.ALIGN_JUSTIFIED });
                document.Add(declaracion);
                document.Add(p);
                document.NewPage();
            }
            document.Close();
            writer.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + "Asistencia mensual " + dateTime.ToString(" MMMM yyyy ") + empresa.Nombre + ".pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(document);
            Response.End();
        }
        public ActionResult Asistencias()
        {
            DBManejoPersonalEntities db = new DBManejoPersonalEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            ViewBag.Trabajador = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, Rut = x.Rut, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno,
                Entrada = x.Entrada,
                EntradaA = x.EntradaA,
                Salida = x.Salida,
                SalidaA = x.SalidaA,
                Contratado = x.Campo
            }).ToList();
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            return View();
        }

        // POST: Informes/Create
        [HttpPost]
        public ActionResult Asistencias(FormCollection collection)
        {
            if(collection["Accion"]=="Generar")
            {
                DBManejoPersonalEntities db = new DBManejoPersonalEntities();
                string nombre = collection["id"];
                Empresas empresa = db.Empresas.First(x => x.Nombre == nombre);
                string periodo = collection["Periodo"];
                Utils.SessionManager.entrada = collection["Entrada"];
                DateTime dateTime = DateTime.Now;
                dateTime = dateTime.AddMonths(-1);
                if (!String.IsNullOrEmpty(periodo))
                {
                    dateTime = Convert.ToDateTime(periodo);
                }

                string[] ids = { "" };
                if (!String.IsNullOrEmpty(collection["centros"]))
                {
                    ids = collection["centros"].Split(new char[] { ',' });
                }
                AsistenciasPDF(dateTime, ids, empresa);
                Utils.SessionManager.log("Asistencias mensuales");
                Utils.SessionManager.entrada = "8:00";
            }
            else
            {
                string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
                string campo = collection["Huerto"];
                ViewBag.campo = campo;
                if (campo == "Todos")
                {
                    ViewBag.Trabajador = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Rut = x.Rut,
                        ApellidoPaterno = x.ApellidoPaterno,
                        ApellidoMaterno = x.ApellidoMaterno,
                        Entrada = x.Entrada,
                        EntradaA = x.EntradaA,
                        Salida = x.Salida,
                        SalidaA = x.SalidaA,
                        Contratado = x.Campo
                    }).ToList();
                }
                else
                {
                    ViewBag.Trabajador = db.Trabajador.Where(x => x.Empresa == empresa && x.Campo == campo).Select(x => new TrabajadorIndex
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Rut = x.Rut,
                        ApellidoPaterno = x.ApellidoPaterno,
                        ApellidoMaterno = x.ApellidoMaterno,
                        Entrada = x.Entrada,
                        EntradaA = x.EntradaA,
                        Salida = x.Salida,
                        SalidaA = x.SalidaA,
                        Contratado = x.Campo
                    }).ToList();
                }
                ViewBag.campos = GetNombreCampos(cuenta.Empresa);
                return View();
            }
            ViewBag.campos = GetNombreCampos(cuenta.Empresa);
            return RedirectToAction("Asistencias");
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
                if(trabajador.Uid!=null && trabajador.Uid.Contains(uid))
                {
                    return trabajador;
                }
            }
            return null;
        }
        public ActionResult Hoy()
        {
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            List<TrabajadorIndex> trabajadores = database.Trabajador
                                            .Where(x => x.Empresa == empresa)
                                            .Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoMaterno = x.ApellidoMaterno, ApellidoPaterno = x.ApellidoPaterno, Uid = x.Uid, Contratado = x.Campo }).ToList();
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
                    string strHeader = "Registros del día " + DateTime.Now.ToLongDateString().Replace(",","");
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
                    tablaRegistrosxdia.SetWidths(new int[] { 1, 3,1,1,1 });
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
                    Response.Write(document);
                    Response.End();
                }
            }
            return RedirectToAction("Index", "Informes");
        }
        public void GenerarPDFRegistros(List<TrabajadorIndex> trabajadores ,List<RegistroTrabajador> registros, string strHeader, List<Registro> registrosLista)
        {
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    strHeader = strHeader +" "+ trabajadores[0].Nombre.Replace(",","");
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
                        document.Add(new Paragraph("Nombre: " + trabajadores[0].Nombre+" "+ trabajadores[0].ApellidoPaterno+" "+ trabajadores[0].ApellidoMaterno , fntHead));
                        document.Add(new Paragraph("Rut: " + trabajadores[0].Rut, fntHead));
                        p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                        document.Add(p);
                    } 
                    //Add line break
                    document.Add(new Chunk("\n", fntHead));
                    
                    List<string> titulosHorasTrabajadas = new List<string> {"Nombre","H. trabajadas" };
                    PdfPTable tablaHorasTrabajadas = new PdfPTable(titulosHorasTrabajadas.Count) {WidthPercentage = 100f};
                    tablaHorasTrabajadas.SetWidths(new int[] { 3, 1 });
                    List<string> titulosHorasExtras = new List<string> {"Nombre","H. extras" };
                    PdfPTable tablaHorasExtras = new PdfPTable(titulosHorasExtras.Count) {WidthPercentage = 100f};
                    tablaHorasExtras.SetWidths(new int[] { 3, 1 });
                    List<string> titulosAtrasos = new List<string> {"Nombre","H. atrasos" };
                    PdfPTable tablaAtrasos = new PdfPTable(titulosAtrasos.Count) {WidthPercentage = 100f};
                    tablaAtrasos.SetWidths(new int[] { 3, 1 });
                    List<string> titulosDiasTrabajados = new List<string> {"Nombre","Días asis." };
                    PdfPTable tablaDiasTrabajados = new PdfPTable(titulosDiasTrabajados.Count) {WidthPercentage = 100f};
                    tablaDiasTrabajados.SetWidths(new int[] { 3 , 1 });
                    //table.SetWidths(new int[] { 60, 150, 80, 50, 50, 50, 50, 50, 50, 50, 50, 100 });
                    //Table header
                    BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntColumnHeader = new Font(btnColumnHeader, 8, 1, BaseColor.WHITE);
                    Font fntCell = new Font(btnColumnHeader, 8, 1, BaseColor.BLACK);
                    int i;
                    for (i = 0; i < titulosHorasTrabajadas.Count; i++)
                    { 
                        PdfPCell cell = new PdfPCell() {BackgroundColor = BaseColor.GRAY} ;
                        cell.AddElement(new Chunk(titulosHorasTrabajadas[i], fntColumnHeader));
                        tablaHorasTrabajadas.AddCell(cell);
                    }
                    for (i = 0; i < titulosHorasExtras.Count; i++)
                    { 
                        PdfPCell cell = new PdfPCell() {BackgroundColor = BaseColor.GRAY} ;
                        cell.AddElement(new Chunk(titulosHorasExtras[i], fntColumnHeader));
                        tablaHorasExtras.AddCell(cell);
                    }
                    for (i = 0; i < titulosHorasTrabajadas.Count; i++)
                    { 
                        PdfPCell cell = new PdfPCell() {BackgroundColor = BaseColor.GRAY} ;
                        cell.AddElement(new Chunk(titulosAtrasos[i], fntColumnHeader));
                        tablaAtrasos.AddCell(cell);
                    }
                    for (i = 0; i < titulosDiasTrabajados.Count; i++)
                    { 
                        PdfPCell cell = new PdfPCell() {BackgroundColor = BaseColor.GRAY} ;
                        cell.AddElement(new Chunk(titulosDiasTrabajados[i], fntColumnHeader));
                        tablaDiasTrabajados.AddCell(cell);
                    }
                    //table Data
                    foreach (var ranking in rankings.OrderByDescending(key => key.Value.Atraso))
                    {
                        tablaAtrasos.AddCell(new Phrase(ranking.Key.Nombre + " " + ranking.Key.ApellidoPaterno + " " + ranking.Key.ApellidoMaterno, fntCell));
                        int hour = ranking.Value.Atraso.Hour + (ranking.Value.Atraso.Day - 1) * 24;
                        int min = ranking.Value.Atraso.Minute;
                        tablaAtrasos.AddCell(new Phrase(hour.ToString("00") + ":" + min.ToString("00"), fntCell));
                    }
                    
                    foreach (var ranking in rankings.OrderByDescending(key => key.Value.HorasExtras))
                    {
                        tablaHorasExtras.AddCell(new Phrase(ranking.Key.Nombre + " " + ranking.Key.ApellidoPaterno + " " + ranking.Key.ApellidoMaterno, fntCell));
                        int hour = ranking.Value.HorasExtras.Hour + (ranking.Value.HorasExtras.Day - 1) * 24;
                        int min = ranking.Value.HorasExtras.Minute;
                        tablaHorasExtras.AddCell(new Phrase(hour.ToString("00") + ":" + min.ToString("00"), fntCell));
                    }

                    foreach (var ranking in rankings.OrderByDescending(key => key.Value.HorasTrabajadas))
                    {
                        tablaHorasTrabajadas.AddCell(new Phrase(ranking.Key.Nombre + " " + ranking.Key.ApellidoPaterno + " " + ranking.Key.ApellidoMaterno, fntCell));
                        int hour = ranking.Value.HorasTrabajadas.Hour + (ranking.Value.HorasTrabajadas.Day - 1) * 24;
                        int min = ranking.Value.HorasTrabajadas.Minute;
                        tablaHorasTrabajadas.AddCell(new Phrase(hour.ToString("00") + ":" + min.ToString("00"), fntCell));
                    }

                    foreach (var dias in diasTrabajados.OrderByDescending(key => key.Value.Count))
                    {
                        tablaDiasTrabajados.AddCell(new Phrase(dias.Key.Nombre + " " + dias.Key.ApellidoPaterno + " " + dias.Key.ApellidoMaterno, fntCell));
                        tablaDiasTrabajados.AddCell(new Phrase(dias.Value.Count.ToString("00"), fntCell));
                    }

                    PdfPTable tablaRanking = new PdfPTable(4) { WidthPercentage = 100f };
                    tablaRanking.SplitLate = false;
                    tablaRanking.AddCell(new PdfPCell( tablaHorasTrabajadas) { Border = Rectangle.NO_BORDER });
                    tablaRanking.AddCell(new PdfPCell(tablaHorasExtras) { Border = Rectangle.NO_BORDER }) ;
                    tablaRanking.AddCell(new PdfPCell(tablaAtrasos) { Border = Rectangle.NO_BORDER });
                    tablaRanking.AddCell(new PdfPCell(tablaDiasTrabajados) { Border = Rectangle.NO_BORDER });
                    document.Add(tablaRanking);

                    document.NewPage();

                    //Write the table
                    List<string> titulos = new List<string>();
                    titulos.Add("Fecha");
                    titulos.Add("Nombre");
                    titulos.Add("Rut");
                    titulos.Add("Inicio");
                    titulos.Add("Termino");
                    titulos.Add("Entrada");
                    titulos.Add("Salida");
                    titulos.Add("H. Jornada");
                    titulos.Add("H. Extras");
                    titulos.Add("H. Totales");
                    titulos.Add("Atraso");
                    titulos.Add("Predio");
                    titulos.Add("Causa");
                    PdfPTable table = new PdfPTable(titulos.Count);
                    table.SetWidths(new int[] { 60, 150, 80, 50, 50, 50, 50, 50, 50, 50, 50, 100, 50 });
                    table.WidthPercentage = 100f;
                    //Table header
                    for ( i = 0; i < titulos.Count; i++)
                    {
                        PdfPCell cell = new PdfPCell();
                        cell.BackgroundColor = BaseColor.GRAY;
                        cell.AddElement(new Chunk(titulos[i], fntColumnHeader));
                        table.AddCell(cell);
                    }
                    //table Data
                    foreach (var celda in registrosLista)
                    {
                        if(celda.EntradaModificada!=null)
                        {
                            celda.Entrada = celda.Entrada + " *";
                        }
                        if (celda.SalidaModificada != null)
                        {
                            celda.Salida = celda.Salida + " *";
                        }
                        table.AddCell(new Phrase(celda.Fecha.ToShortDateString(), fntCell));
                        table.AddCell(new Phrase(celda.Nombre+" "+celda.ApellidoPaterno+" "+celda.ApellidoMaterno, fntCell));
                        table.AddCell(new Phrase(celda.Rut, fntCell));
                        table.AddCell(new Phrase(celda.EntradaOficial, fntCell));
                        table.AddCell(new Phrase(celda.SalidaOficial, fntCell));
                        table.AddCell(new Phrase(celda.Entrada, fntCell));
                        table.AddCell(new Phrase(celda.Salida, fntCell));
                        table.AddCell(new Phrase(celda.HorasTrabajadas, fntCell));
                        table.AddCell(new Phrase(celda.HorasExtras, fntCell));
                        table.AddCell(new Phrase(celda.Horas, fntCell));
                        table.AddCell(new Phrase(celda.Atraso, fntCell));
                        table.AddCell(new Phrase(celda.Campo, fntCell));
                        string causa = "";
                        if(!String.IsNullOrEmpty(celda.CausaEntrada))
                        {
                            causa = celda.CausaEntrada;
                            if(!String.IsNullOrEmpty(celda.CausaSalida))
                            {
                                causa = causa + "/" + celda.CausaSalida;
                            }
                        }
                        if (!String.IsNullOrEmpty(celda.CausaSalida) && String.IsNullOrEmpty(celda.CausaEntrada))
                        {
                            causa = celda.CausaSalida;
                        }
                        table.AddCell(new Phrase(causa, fntCell));
                    }

                    document.Add(table);
                    document.Close();
                    writer.Close();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename="+strHeader+" "+ DateTime.Now.ToShortDateString()+ ".pdf");
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Write(document);
                    Response.End();
                    Utils.SessionManager.entrada = "";
                    Utils.SessionManager.salida = "";
                    Utils.SessionManager.almuerzo = "";
                }
            }

        }
        public void generarPDFDetalle(Trabajador trabajador, string strHeader, int type)
        {
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    strHeader = "Solicitud de contratacion";
                    //System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    Document document = new Document();
                    document.AddTitle(strHeader);
                    document.SetPageSize(iTextSharp.text.PageSize.A4);
                    PdfWriter wri;
                    if (type==1)
                        wri = PdfWriter.GetInstance(document, Response.OutputStream);
                    else
                        wri = PdfWriter.GetInstance(document, new FileStream("C:\\Data\\doc.pdf", FileMode.Create));
                    wri.SetPdfVersion(PdfWriter.PDF_VERSION_1_5);
                    wri.CompressionLevel = PdfStream.BEST_COMPRESSION;
                    document.Open();
                    Image image = Image.GetInstance(Server.MapPath("~/App_Data/" + Utils.SessionManager.CuentaAutenticada().Empresa + ".png"));
                    image.Alignment = Element.ALIGN_LEFT;
                    image.ScaleToFit(60, 60);
                    document.Add(image);
                    //Report Header
                    BaseFont bfntHead = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntHead = new Font(bfntHead, 14, 1, BaseColor.DARK_GRAY);
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

                    //Add a line seperation
                    Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                    document.Add(p);
                    fntHead.Size = 9;
                    var font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 13, iTextSharp.text.Font.BOLD);
                    font.Size = 10;
                    p = new Paragraph("Antecedentes");
                    p.Alignment = Element.ALIGN_CENTER;
                    p.Font = fntHead;
                    document.Add(p);
                    document.Add(new Paragraph("Nombre:                 " + trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno, fntHead));
                    document.Add(new Paragraph("Rut:                    " + trabajador.Rut, fntHead));
                    document.Add(new Paragraph("Tipo de cuenta:         " + trabajador.TipoCuenta, fntHead));
                    document.Add(new Paragraph("Banco:                  " + trabajador.Banco, fntHead));
                    document.Add(new Paragraph("Número de cuenta:       " + trabajador.NumeroCuenta, fntHead));
                    document.Add(new Paragraph("Contratado:             " + trabajador.Contratado, fntHead));
                    document.Add(new Paragraph("Uid:                    " + trabajador.Uid, fntHead));
                    document.Add(new Paragraph("Código:                 " + trabajador.CodPersonal, fntHead));
                    document.Add(new Paragraph("Estado civil:           " + trabajador.EstadoCivil, fntHead));
                    if (trabajador.FechaNacimiento != null)
                    {
                        document.Add(new Paragraph("Fecha de nacimiento:    " + trabajador.FechaNacimiento.Value.ToShortDateString(), fntHead));
                    }
                    else
                    {
                        document.Add(new Paragraph("Fecha de nacimiento:       ", fntHead));
                    }
                    document.Add(new Paragraph("Dirección:              " + trabajador.Direccion, fntHead));
                    document.Add(new Paragraph("Sexo:                   " + trabajador.Sexo, fntHead));
                    document.Add(new Paragraph("Ciudad:                 " + trabajador.Ciudad, fntHead));
                    document.Add(new Paragraph("Cargas familiares:      " + trabajador.CargasFamiliares, fntHead));
                    document.Add(new Paragraph("Cuantas:                " + trabajador.CargasSimples, fntHead));
                    document.Add(new Paragraph("AFP:                    " + trabajador.AFP, fntHead));
                    document.Add(new Paragraph("Salud:                  " + trabajador.Salud, fntHead));
                    document.Add(new Paragraph("Telefono:               " + trabajador.Telefono, fntHead));

                    p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                    document.Add(p);
                    p = new Paragraph("En Caso de emergencia");
                    p.Alignment = Element.ALIGN_CENTER;
                    p.Font = font;
                    document.Add(p);
                    document.Add(new Paragraph("Nombre:                 " + trabajador.NombreEmer, fntHead));
                    document.Add(new Paragraph("Vínculo:                " + trabajador.VinculoEmer, fntHead));
                    document.Add(new Paragraph("Dirección:              " + trabajador.DireccionEmer, fntHead));
                    document.Add(new Paragraph("Telefono:               " + trabajador.TelefonoEmer, fntHead));

                    p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                    document.Add(p);
                    p = new Paragraph("Antecedentes del cargo");  
                    p.Alignment = Element.ALIGN_CENTER;
                    p.Font = font;
                    document.Add(p);
                    document.Add(new Paragraph("Cargo:                  " + trabajador.Cargo, fntHead));
                    document.Add(new Paragraph("Nombre Jefe:            " + trabajador.NombreJefe, fntHead));
                    document.Add(new Paragraph("Campo:                  " + trabajador.Campo, fntHead));
                    document.Add(new Paragraph("Empleador:              " + trabajador.Empleador, fntHead));

                    p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                    document.Add(p);
                    p = new Paragraph("Condiciones pactadas");
                    p.Alignment = Element.ALIGN_CENTER;
                    p.Font = font;
                    document.Add(p);
                    document.Add(new Paragraph("Contratado por:         " + trabajador.Contrato, fntHead));
                    document.Add(new Paragraph("Jornada:                " + trabajador.Jornada, fntHead));
                    if (trabajador.FechaIngreso != null)
                    {
                        document.Add(new Paragraph("Fecha de ingreso:       " + trabajador.FechaIngreso.Value.ToShortDateString(), fntHead));
                    }
                    else
                    {
                        document.Add(new Paragraph("Fecha de ingreso:       ", fntHead));
                    }
                    if (trabajador.FechaTermino != null)
                    {
                        document.Add(new Paragraph("Fecha de termino:       " + trabajador.FechaTermino.Value.ToShortDateString(), fntHead));
                    }
                    else
                    { 
                        document.Add(new Paragraph("Fecha de termino:       " , fntHead));
                    }
                    p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                    document.Add(p);
                    p = new Paragraph("Renta a ofrecer");
                    p.Alignment = Element.ALIGN_CENTER;
                    p.Font = font;
                    document.Add(p);
                    int sueldoBase = 0;
                    if(trabajador.SueldoBase != null)
                        sueldoBase = (int) trabajador.SueldoBase;
                    int gratificacion = 0;
                    if (trabajador.Gratificacion != null)
                        gratificacion = (int) trabajador.Gratificacion;
                    int sueldoBruto = 0;
                    if (trabajador.SueldoBruto != null)
                        sueldoBruto = (int) trabajador.SueldoBruto;

                    document.Add(new Paragraph("Sueldo base:            " + sueldoBase.ToString("C0"), fntHead));
                    document.Add(new Paragraph("Gratificacion:          " + gratificacion.ToString("C0"), fntHead));
                    document.Add(new Paragraph("Sueldo bruto:           " + sueldoBruto.ToString("C0"), fntHead));
                    
                    image = Image.GetInstance(Server.MapPath("~/App_Data/"+trabajador.Campo.Replace(" ","")+".png"));
                    image.Alignment = Element.ALIGN_LEFT;
                    image.ScaleToFit(400,60);
                    document.Add(image);
                    document.Add(new Paragraph("       Jefe "+Formato(trabajador.NombreJefe)+"Gerente Agricola                   Recursos Humanos", fntHead));
                    //XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, sr);
                    if(trabajador.FotoCarnet!=null)
                    {
                        try
                        { 
                            var ms = new MemoryStream(trabajador.FotoCarnet);
                            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                            img = resizeImage(img, new Size(1120, 630));
                            image = Image.GetInstance(img, System.Drawing.Imaging.ImageFormat.Jpeg);
                            image.Alignment = Element.ALIGN_CENTER;
                            image.ScaleAbsolute(496,279);
                            document.Add(image);
                        }
                        catch
                        {
                            document.Add(new Paragraph("Error en cargar la imagen", fntHead));
                        }
                    }
                    
                    document.Close();
                    wri.Close();
                    if (type==1)
                    {
                        Response.ContentType = "application/pdf";
                        Response.AddHeader("content-disposition", "attachment;filename="+strHeader+" "+ trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno + " "+ DateTime.Now.ToShortDateString() + ".pdf");
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        Response.Write(document);
                    }
                    Response.End();
                }
            }
        }
        public static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            return (System.Drawing.Image)(new Bitmap(imgToResize, size));
        }
        public string Formato(string nombre)
        {
            while(nombre.Length<28)
            {
                nombre = nombre + " ";
            }
            return nombre;
        }
        public void GenerarExcel(List<TrabajadorIndex> trabajadores, string strHeader, List<Registro> registrosLista)
        {

            try
            {
                //creamos el objeto SLDocument el cual creara el excel
                SLDocument sl = new SLDocument();
                SLStyle fecha = sl.CreateStyle();
                SLStyle hora = sl.CreateStyle();
                fecha.FormatCode = "dd-mm-yyyy";
                hora.FormatCode = "hh:mm";
                //creamos las celdas en diagonal
                //utilizando la función setcellvalue pueden navegar sobre el documento
                //primer parametro es la fila el segundo la columna y el tercero el dato de la celda
                sl.SetCellValue(1, 1, "Fecha");
                sl.SetCellValue(1, 2, "Nombre");
                sl.SetCellValue(1, 3, "Rut");
                sl.SetCellValue(1, 4, "Inicio");
                sl.SetCellValue(1, 5, "Termino");
                sl.SetCellValue(1, 6, "Entrada");
                sl.SetCellValue(1, 7, "Salida");
                sl.SetCellValue(1, 8, "H. Jornada");
                sl.SetCellValue(1, 9, "H. Extras");
                sl.SetCellValue(1, 10, "H. Totales");
                sl.SetCellValue(1, 11, "Atraso");
                sl.SetCellValue(1, 12, "Predio");
                sl.SetCellValue(1, 13, "Contratado");

                for (int i = 2; i < registrosLista.Count + 2; ++i)
                {
                    int j = i - 2;
                    sl.SetCellValue(i, 1, registrosLista[j].Fecha.ToShortDateString());
                    sl.SetCellStyle(i, 1, fecha);
                    sl.SetCellValue(i, 2, registrosLista[j].Nombre +" "+ registrosLista[j].ApellidoPaterno+" "+ registrosLista[j].ApellidoMaterno);
                    sl.SetCellValue(i, 3, formatearRut(registrosLista[j].Rut));
                    sl.SetCellValue(i, 4, registrosLista[j].EntradaOficial);
                    sl.SetCellStyle(i, 4, hora);
                    sl.SetCellValue(i, 5, registrosLista[j].SalidaOficial);
                    sl.SetCellStyle(i, 5, hora);
                    sl.SetCellValue(i, 6, registrosLista[j].Entrada);
                    sl.SetCellStyle(i, 6, hora);
                    sl.SetCellValue(i, 7, registrosLista[j].Salida);
                    sl.SetCellStyle(i, 7, hora);
                    sl.SetCellValue(i, 8, registrosLista[j].HorasTrabajadas);
                    sl.SetCellStyle(i, 8, hora);
                    sl.SetCellValue(i, 9, registrosLista[j].HorasExtras);
                    sl.SetCellStyle(i, 9, hora);
                    sl.SetCellValue(i, 10, registrosLista[j].Horas);
                    sl.SetCellStyle(i, 10, hora);
                    sl.SetCellValue(i, 11, registrosLista[j].Atraso);
                    sl.SetCellStyle(i, 11, hora);
                    sl.SetCellValue(i, 12, registrosLista[j].Campo);
                    sl.SetCellValue(i, 13, registrosLista[j].Contratado);

                }

                //Guardar como, y aqui ponemos la ruta de nuestro archivo
                sl.SaveAs("C:\\Data\\WorksheetOperations.xlsx");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocurrio una Excepción: " + ex.Message);
            }

            //doc.SaveAs("C:\\Data\\WorksheetOperations.xlsx");
            FileStream sourceFile = new FileStream("C:\\Data\\WorksheetOperations.xlsx", FileMode.Open);
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
            Response.AddHeader("content-disposition", "attachment;filename=" + strHeader + " " + DateTime.Now.ToShortDateString() + ".xlsx");
            Response.BinaryWrite(getContent);
            Response.Flush();
            Response.End();
            //System.Diagnostics.Process.Start("C:\\Data\\WorksheetOperations.xlsx");
        }
        public ActionResult PorDia()
        {
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
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
        public ActionResult PorDia(FormCollection collection)
        {
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            DateTime fecha = Convert.ToDateTime(collection["Inicio"]);
            DateTime fin = fecha.AddHours(Convert.ToUInt32(collection["Horas"]));
           // List<Trabajador> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).ToList();
            List<TrabajadorIndex> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoPaterno=x.ApellidoPaterno, ApellidoMaterno=x.ApellidoMaterno, Uid = x.Uid, Empresa = x.Empresa, Contratado = x.Campo,
                Entrada = x.Entrada,
                EntradaA = x.EntradaA,
                Salida = x.Salida,
                SalidaA = x.SalidaA
            }).ToList();
            int i = 0;
            while (i < trabajadores.Count)
            {
                var trabajador = trabajadores[i];
                var registry = database.RegistroTrabajador.Where(x => x.Fecha >= fecha && x.Fecha <=fin && x.Uid == trabajador.Uid).ToList();
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
            Utils.SessionManager.log("Consulta por día");
            return View(registros.OrderByDescending(x => x.Fecha));
        }
        public ActionResult PDFDetalle(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Trabajador trabajador = database.Trabajador.First(x => x.Id == id);
            List<RegistroTrabajador> registros = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid).ToList();
            Utils.SessionManager.log("Solicitud de: "+ trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno);
            generarPDFDetalle(trabajador, "Detalle", Utils.SessionManager.email);
            return RedirectToAction("Index");
        }
        public ActionResult PDFCampo(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Campos campo = database.Campos.First(x => x.Id == id);
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            var registroTrabajadors = database.RegistroTrabajador.Select(x => new { x.Uid, x.Campo, x.Empresa }).Where(x => x.Campo == campo.Nombre && x.Empresa == empresa).Distinct().ToList();
            var registros = registroTrabajadors.Select(x => x.Uid).ToList();
            List<TrabajadorIndex> trabajadores = new List<TrabajadorIndex>();

            foreach (var uid in registros)
            {
                try
                {
                    TrabajadorIndex t = database.Trabajador.Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno=x.ApellidoMaterno, Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa, Contratado = x.Campo,
                        Entrada = x.Entrada,
                        EntradaA = x.EntradaA,
                        Salida = x.Salida,
                        SalidaA = x.SalidaA
                    }).First(x => x.Uid == uid);
                    trabajadores.Add(t);
                }
                catch { }
            }

            List<Registro> registrosLista = GetRegistroTrabajadores(trabajadores, null, null, campo.Nombre, null);
            trabajadores.OrderBy(o => o.Uid);
            trabajadores.Insert(0,new TrabajadorIndex { Nombre = campo.Nombre });

            if (Utils.SessionManager.tipo == "PDF")
            {
                Utils.SessionManager.log("PDF huerto: " + campo.Nombre);
                GenerarPDFRegistros(trabajadores, null, "Huerto "+campo.Nombre, registrosLista);
            }
            else
            {
                Utils.SessionManager.log("Excel campo" + campo.Nombre);
                GenerarExcel(trabajadores, "Huerto " + campo.Nombre, registrosLista);
            }
            return RedirectToAction("Index","Campo");
        }
        public void PDFHoy()
        {
            Utils.SessionManager.log("Pdf hoy");
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            List<TrabajadorIndex> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { 
                                                 Id = x.Id, Nombre = x.Nombre, ApellidoMaterno = x.ApellidoMaterno, ApellidoPaterno = x.ApellidoPaterno,
                                                 Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa, Contratado = x.Campo,
                                                 Entrada = x.Entrada, EntradaA = x.EntradaA, Salida = x.Salida, SalidaA = x.SalidaA}).ToList();
            List<Registro> registros = GetRegistroTrabajadores(trabajadores, DateTime.Now, null, null, null);

            trabajadores.Insert(0, new TrabajadorIndex { Nombre = "Hoy" });
            GenerarPDFRegistros(trabajadores, null, "Registro",registros);
        }
        public List<Registro> GetRegistroTrabajadores(List<TrabajadorIndex> trabajadores, DateTime? inicio, DateTime? fin, string campo, DateTime? periodo)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            List<Registro> registrosLista = new List<Registro>();
            //Para cada trabajador se le cargan los registros solicitidaos
            foreach (var trabajador in trabajadores)
            {
                var registrosTrabajador = new List<RegistroPDF>();
                //Generar por periodo de tiempo
                if(periodo!=null)
                {
                    int mes = periodo.Value.Month;
                    int año = periodo.Value.Year;
                    registrosTrabajador = database.RegistroTrabajador
                                        .Where(x => x.Fecha.Month == mes && x.Fecha.Year == año && x.Uid == trabajador.Uid)
                                        .Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador}).ToList();
                }
                //Un campo y un inicio
                else if (inicio == null && fin == null && string.IsNullOrEmpty(campo))
                {
                    registrosTrabajador = database.RegistroTrabajador
                                          .Where(x => x.Uid == trabajador.Uid )
                                          .Select(x=> new RegistroPDF {Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                else if (inicio!=null && fin == null && string.IsNullOrEmpty(campo))
                {
                    DateTime end = Convert.ToDateTime(inicio);
                    registrosTrabajador = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid
                                                                            && x.Fecha.Year == end.Year
                                                                            && x.Fecha.Month == end.Month
                                                                            && x.Fecha.Day == end.Day).Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                else if(inicio!=null && fin!= null && string.IsNullOrEmpty(campo))
                {
                    DateTime end = Convert.ToDateTime(fin);
                    end = end.AddDays(1);
                    registrosTrabajador = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid && x.Fecha > inicio && x.Fecha < end).Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                else if(!string.IsNullOrEmpty(campo) && inicio != null && fin == null)
                {
                    DateTime end = Convert.ToDateTime(inicio);
                    registrosTrabajador = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid
                                                                            && x.Fecha.Year == end.Year
                                                                            && x.Fecha.Month == end.Month
                                                                            && x.Fecha.Day == end.Day && x.Campo == campo).Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                else if(!string.IsNullOrEmpty(campo) && inicio == null && fin == null)
                {
                    registrosTrabajador = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid && x.Campo == campo).Select(x => new RegistroPDF { Campo = x.Campo, Empresa = x.Empresa, Fecha = x.Fecha, Uid = x.Uid, Causa = x.NombreTrabajador, Modificado = x.IdTrabajador }).ToList();
                }
                else if(inicio != null && fin != null && !string.IsNullOrEmpty(campo))
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
                                horas = horas + (extra - t2);
                            }
                            else
                            {
                                horas = horas + (extra - primero.Fecha);
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
                            almuerzo = almuerzo + (salidaAlmuerzo - entradaAlmuerzo);
                        }
                        else
                        {
                            DateTime.TryParse("01/01/0001 " + Utils.SessionManager.almuerzo + ":00", out almuerzo);
                        }
                        DateTime horasT = new DateTime();
                        DateTime horasQL = new DateTime();
                        string raro = "";
                        DateTime salida = new DateTime();
                        DateTime.TryParse("01/01/0001 " + segundo.Fecha.Hour+":"+ segundo.Fecha.Minute, out salida);
                        if (almuerzo < horas &&  salidaAlmuerzo < salida)

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
                            HorasExtras = horasExtras.ToShortTimeString(),
                            Atraso = atrasoP.ToShortTimeString(),
                            Campo = primero.Campo,
                            //Horas totales
                            Horas = horasT.ToShortTimeString() + raro,
                            HorasTrabajadas = horasTrabajadas.ToShortTimeString(),
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
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
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
                                                .Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoMaterno = x.ApellidoMaterno,
                                                    ApellidoPaterno = x.ApellidoPaterno,
                                                    Rut = x.Rut, Uid = x.Uid , Contratado = x.Campo,
                                                    Entrada = x.Entrada,
                                                    EntradaA = x.EntradaA,
                                                    Salida = x.Salida,
                                                    SalidaA = x.SalidaA
                                                }).ToList();
            //List<RegistroTrabajador> registrosTrabajador = new List<RegistroTrabajador>();
            List<Registro> registrosLista = GetRegistroTrabajadores(trabajadores, null, null, null, null);
            trabajadores.Insert(0, new TrabajadorIndex() { Nombre = "" });
            if (Utils.SessionManager.tipo == "PDF")
            {
                GenerarPDFRegistros(trabajadores, null, "Consolidado", registrosLista);
                Utils.SessionManager.log("PDF consolidado");
            }
            else
            {
                GenerarExcel(trabajadores, "Consolidado", registrosLista);
                Utils.SessionManager.log("Excel consolidado");
            }


        }
        public void PDFRegistro(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            TrabajadorIndex trabajador = database.Trabajador.Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoMaterno = x.ApellidoMaterno,
                                                                ApellidoPaterno = x.ApellidoPaterno, Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente,
                                                                Empresa = x.Empresa, Contratado = x.Campo,
                                                                Entrada = x.Entrada,
                                                                EntradaA = x.EntradaA,
                                                                Salida = x.Salida,
                                                                SalidaA = x.SalidaA
                                                            }).First(x => x.Id == id);
            List<TrabajadorIndex> trabajadores = new List<TrabajadorIndex>
            {
                trabajador
            };
            List<Registro> registros = GetRegistroTrabajadores(trabajadores,null,null,null, null);

            if (Utils.SessionManager.tipo == "PDF")
            {
                GenerarPDFRegistros(trabajadores, null, "Registro", registros);
                Utils.SessionManager.log("PDF registro");
            }
            else
            {
                GenerarExcel(trabajadores, "Registro", registros);
                Utils.SessionManager.log("Excel registro");
            }
        }
        public ActionResult PDFRegistroFecha()
        {
            List<TrabajadorIndex> trabajadores = Utils.SessionManager.trabajadores;
            List<Registro> registros = new List<Registro>();
            if (Utils.SessionManager.inicio.ToShortDateString()=="01-01-0001" && Utils.SessionManager.fin.ToShortDateString() == "01-01-0001" && !string.IsNullOrEmpty(Utils.SessionManager.campo))
            {
                registros = GetRegistroTrabajadores(trabajadores, null, null, Utils.SessionManager.campo, null);

            }
            if (Utils.SessionManager.inicio.ToShortDateString() != "01-01-0001" && Utils.SessionManager.fin.ToShortDateString() == "01-01-0001" && string.IsNullOrEmpty(Utils.SessionManager.campo))
            {
                registros = GetRegistroTrabajadores(trabajadores, Utils.SessionManager.inicio, null, null, null);

            }
            if (Utils.SessionManager.inicio.ToShortDateString() != "01-01-0001" && Utils.SessionManager.fin.ToShortDateString() != "01-01-0001" && string.IsNullOrEmpty(Utils.SessionManager.campo))
            {
                registros = GetRegistroTrabajadores(trabajadores, Utils.SessionManager.inicio, Utils.SessionManager.fin, null, null);

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
                Utils.SessionManager.log("PDF registro por fecha");
                GenerarPDFRegistros(trabajadores, null, "Registro", registros);
            }
            else
            {
                Utils.SessionManager.log("Excel registro por fecha");
                GenerarExcel(trabajadores, "Registro", registros);
            }
            return RedirectToAction("Index");
        }
        public string formatearRut(string rut)
        {
            int cont = 0;
            string format;
            if (rut.Length == 0)
            {
                return "";
            }
            else
            {
                rut = rut.Replace(".", "");
                rut = rut.Replace("-", "");
                rut = rut.Replace(" ", "");
                format = "-" + rut.Substring(rut.Length - 1);
                for (int i = rut.Length - 2; i >= 0; i--)
                {
                    format = rut.Substring(i, 1) + format;
                    cont++;
                    if (cont == 3 && i != 0)
                    {
                        format = "." + format;
                        cont = 0;
                    }
                }
                return format;
            }
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
            nombres = new string[campos.Count+1];
            int count = 1;
            nombres[0] = "Todos";
            foreach (var campo in campos)
            {
                nombres[count] = campo.Nombre;
                count++;
            }
            return nombres;
        }
        // GET: Trabajador
        public ActionResult Index(int? id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            if (id != null)
            {
                int idb = (int)id;
                Trabajador trabajador = database.Trabajador.First(x => x.Id == id);
                string file = "C:\\Data\\doc.pdf";
                try
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient();
                    mail.From = new MailAddress("notificacionjcp@ingenieriajcp.cl",
                    "SGA JCP", System.Text.Encoding.UTF8);
                    mail.To.Add("nicolasmorales@invina.net");
                    //mail.To.Add("sebastianct36@outlook.com");
                    mail.Subject = "Solicitud de Contrato de " + trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno;
                    Utils.SessionManager.email = 2;
                    PDFDetalle(idb);
                    Utils.SessionManager.email = 1;
                    MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(file));

                    mail.Attachments.Add(new System.Net.Mail.Attachment(ms, trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno + ".pdf"));
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Port = 25;
                    SmtpServer.Host = "mail.ingenieriajcp.cl";
                    SmtpServer.Credentials = new System.Net.NetworkCredential("notificacionjcp@ingenieriajcp.cl", "notificacion");
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Send(mail);
                    mail.Dispose();
                    SmtpServer.Dispose();
                    Utils.SessionManager.mensaje = "Enviado correctamente";
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    Utils.SessionManager.mensaje = "Falló el envío";
                }
                Utils.SessionManager.log("Envió un correo");
            }

            Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();
            string empresa = cuenta.Empresa;
            string[] campos;
            List<TrabajadorIndex> trabajadores = new List<TrabajadorIndex>();
            if (empresa == "JCP")
            {
                campos = GetNombreCampos("");
            }
            else
            {
                campos = GetNombreCampos(empresa);
            }
            foreach (var campo in campos)
            {
                if (cuenta.Permisos.Contains(campo))
                {
                    trabajadores.AddRange(database.Trabajador.Where(x => x.Campo == campo).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno, Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa, Contratado = x.Campo,
                        Entrada = x.Entrada,
                        EntradaA = x.EntradaA,
                        Salida = x.Salida,
                        SalidaA = x.SalidaA
                    }).ToList());
                }
            }
            Utils.SessionManager.log("Index trabajadores");

            return View(trabajadores);
        }
        // GET: Trabajador/Details/5
        public ActionResult Details(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Trabajador trabajador = database.Trabajador.First(x => x.Id == id);
            if (Utils.SessionManager.alerta != 0)
            {
                ViewBag.alerta = Utils.SessionManager.alerta;
                Utils.SessionManager.alerta = 0;
            }
            Utils.SessionManager.log("Detalle trabajador: " + trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno);
            return View(trabajador);
        }
        // GET: Trabajador/Create
        private void datosCreate()
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
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
        public ActionResult Create()
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
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
            var empresita = database.Empresas.First(x => x.Nombre == empresa);
            var cargos = database.Cargo.Where(x => x.IdEmpresa == empresita.Id).ToList();
            List<String> nombresCargo = new List<string>();
            foreach (var cargo in cargos)
            {
                nombresCargo.Add(cargo.Nombre);
            }
            ViewBag.Cargos = nombresCargo;
            Trabajador trabajador = new Trabajador();
            return View(trabajador);
        }

        // POST: Trabajador/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection, HttpPostedFileBase file,
            [Bind(Include = "Id,Salida,SalidaA,Entrada,EntradaA,Nombre,ApellidoPaterno,ApellidoMaterno,Rut,TipoCuenta,Banco,NumeroCuenta,Contratado,CodPersonal," +
            "EstadoCivil,Nacionalidad,FechaNacimiento,Direccion,Sexo,Ciudad,Telefono,Email,CargasFamiliares,TrampoAsignacionFamiliar," +
            "CargasSimples,CargasMaternales,CargasInvalidas,RegimenPrevicional,AsignacionFamiliar,AsignacionFamiliarRetroactiva," +
            "ReintegroCargasFamiliares,SolicitudTrabajadorJoven,AFP,TipoLineaAFP,RentaImponibleAFP,CotizacionObligatoriaAFP,SIS," +
            "AhorroVoluntarioAFP,RentaImpSustitutiva,TasaPactadaSus,AporteIndemSus,NPeriodosSus,PeriodosDesdeSus,PuestoTrabajoPesado," +
            "PerodoHastaSus,PorcentajeTrabajoPesado,MontoTrabajoPesado,CodigoMovimientoDePersonal,InicioMovimientoPersonal,FinMovimientoPersonal," +
            "TrampoAFP,Jubiulado,SeguroCesantia,Mutual,Salud,NombreEmer,VinculoEmer,DireccionEmer,TelefonoEmer,Cargo,NombreJefe,Campo,Empleador," +
            "Contrato,Jornada,FechaIngreso,FechaTermino,SueldoBase,Gratificacion,SueldoBruto,TipoPago,FactorHE,Colacion,Movilizacion," +
            "DiasVacacionesAño,JefeDirecto,Gerente,Uid,Empresa,APVI,CodigoAPVI,NumeroAPVI,FormaPagoAPVI,CotizacionAPVI," +
            "CotizaciónDepositosConvenidos,APVC,CodigoAPVC,NumeroAPVC,FormaPagoAPVC,CotizacionTrabajadorAPVC,CotizacionEmpleadorAPVC," +
            "AfiliadoVoluntario,RutAV,ApellidoPaternoAV,ApellidoMaternoAV,NombresAV,CodigoMovimientoPersonalAV,DesdeAV,HastaAV,AFPAV," +
            "MontoCapitalizacionVoluntaria,MontoAV,PeriodosAV,CodigoExcaja,TasaExcaja,RentaImponibleIPS,CotizacionObligatoriaIPS," +
            "RentaImponibleDesahucio,CodigoExcajaDesahucio,TasaCotizacionDesahucio,CotizaciónDesahucio,CotizacionFonasa,CotizacionISL," +
            "BonificacionLey,DescuentoPorCargasFamiliaresISL,BonosGobierno,CodigoIntitucionSalud,NumeroFun,RentaImponibleIsapre," +
            "MonedaDelPlanPactadoIsapre,CotizacionPactada,CotizacionIsapre,CotizacionIsapreAV,MontoGES,CodigoCCAF,RentaImponibleCCAF," +
            "CreditosPersonalesCCAF,DescuentoDentalCCAF,DescuentosPorLeasing,DescuentoPorSeguroDeVidaCCAF,OtrosDescuentosCCAF," +
            "CotizacionCCAFNoAfiniladosIsapres,DescuentoCargasFamiliaresCCAF,OtrosDescuentosCCAF1,OtrosDescuentosCCAF2,BonosGobiernoCCAF," +
            "CodigoSucursalCCAF,CodigoMutualidad,RentaImponibleMutual,CotizacionAccidenteMutual,SucursalParaPagoMutual,RentaImponibleSeguroCensantia," +
            "AporteTrabajadorSeguroCensatia,AporteEmpleadorSeguroCesantia,Subsidio,DatosEmpresa")] 
            Trabajador trabajadorNew)
        {
            try
            {
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                //Trabajador trabajadorNew = new Trabajador();
                /*trabajadorNew.Nombre = collection["Nombre"].Replace(",","");
                trabajadorNew.ApellidoPaterno = collection["ApellidoPaterno"].Replace(",", "");
                trabajadorNew.ApellidoMaterno = collection["ApellidoMaterno"].Replace(",", "");
                trabajadorNew.Rut = collection["Rut"];
                trabajadorNew.Rut = formatearRut(trabajadorNew.Rut);
                trabajadorNew.TipoCuenta = collection["TipoCuenta"];
                trabajadorNew.Banco = collection["Banco"];
                trabajadorNew.NumeroCuenta = collection["NumeroCuenta"];
                trabajadorNew.Contratado = collection["Contratado"];
                trabajadorNew.CodPersonal = collection["CodPersonal"];
                trabajadorNew.EstadoCivil = collection["EstadoCivil"];
                if (DateTime.TryParse(collection["FechaNacimiento"], out temp))
                {
                    trabajadorNew.FechaNacimiento = Convert.ToDateTime(collection["FechaNacimiento"]);
                }
                trabajadorNew.Direccion = collection["Direccion"];
                trabajadorNew.Sexo = collection["Sexo"];
                trabajadorNew.Ciudad = collection["Ciudad"];
                trabajadorNew.CargasFamiliares = collection["CargasFamiliares"];
                if (collection["CargasSimples"].ToString().Length>0)
                {
                    trabajadorNew.CargasSimples = Convert.ToInt32(collection["CargasSimples"]);
                }
                trabajadorNew.AFP = collection["AFP"];
                trabajadorNew.Salud = collection["Salud"];
                trabajadorNew.CodigoIntitucionSalud = trabajadorNew.Salud;
                trabajadorNew.Telefono = collection["Telefono"];
                trabajadorNew.NombreEmer = collection["NombreEmer"];
                trabajadorNew.VinculoEmer = collection["VinculoEmer"];
                trabajadorNew.DireccionEmer = collection["DireccionEmer"];
                trabajadorNew.TelefonoEmer = collection["TelefonoEmer"];
                trabajadorNew.Cargo = collection["Cargo"];
                trabajadorNew.NombreJefe = collection["NombreJefe"];
                trabajadorNew.Campo = collection["Campo"];
                trabajadorNew.Empleador = collection["Empleador"];
                trabajadorNew.Contrato = collection["Contrato"];
                if (DateTime.TryParse(collection["FechaTermino"], out temp))
                {
                    trabajadorNew.FechaTermino = Convert.ToDateTime(collection["FechaTermino"]);
                }
                trabajadorNew.Jornada = collection["Jornada"];
                if (DateTime.TryParse(collection["FechaIngreso"], out temp))
                {
                    trabajadorNew.FechaIngreso = Convert.ToDateTime(collection["FechaIngreso"]);
                }
                
                if (collection["SueldoBase"].ToString().Length > 0)
                {
                    trabajadorNew.SueldoBase = Convert.ToInt32(collection["SueldoBase"]);
                }
                
                if (collection["Gratificacion"].ToString().Length > 0)
                {
                    trabajadorNew.Gratificacion = Convert.ToInt32(collection["Gratificacion"]);  
                }
                
                if (collection["SueldoBruto"].ToString().Length > 0)
                {
                    trabajadorNew.SueldoBruto = Convert.ToInt32(collection["SueldoBruto"]);
                }
                
                trabajadorNew.Gerente = collection["Gerente"];
                trabajadorNew.Empresa = collection["Empresa"];
                */

                trabajadorNew.Rut = formatearRut(trabajadorNew.Rut);
                HttpPostedFileBase postedFile = Request.Files["Foto"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    trabajadorNew.FotoCarnet = getImageFromPostfile(postedFile, 850);
                    ViewBag.foto = Request.Files["Foto"];
                }
                if (String.IsNullOrEmpty(collection["aviso"]))
                {
                    Trabajador trabajador1 = null;
                    try
                    {
                        string uid = collection["Uid"];
                        trabajador1 = database.Trabajador.First(x => x.Uid == uid);
                    }
                    catch { }
                    if (trabajador1 != null)
                    {
                        ViewBag.texto = "Uid en uso, en\n "+trabajador1.Nombre+" "+trabajador1.ApellidoPaterno + " " +trabajador1.ApellidoMaterno+"\n " +trabajador1.Rut;
                        ViewBag.aviso = "Avisado";
                        datosCreate();
                        return View(trabajadorNew);
                    }
                }
                if (collection["Uid"] != null && collection["Uid"] != String.Empty)
                {
                    trabajadorNew.Uid = cambiarUidCrear(collection["Uid"]);
                }
                if (!validarRut(trabajadorNew.Rut))
                {
                    ViewBag.texto = "Rut no válido";
                    datosCreate();
                    return View(trabajadorNew);
                }
                Trabajador trabajador = null;
                try
                {
                    trabajador = database.Trabajador.First(x => x.Rut == trabajadorNew.Rut);
                    ViewBag.texto = "Ya existe el rut";
                    datosCreate();
                    return View(trabajadorNew);
                }
                catch { }
                database.Trabajador.Add(trabajadorNew);
                database.SaveChanges();
                Utils.SessionManager.log("Crear trabajador: " + trabajadorNew.Nombre + " "+trabajadorNew.ApellidoPaterno + " "+ trabajadorNew.ApellidoMaterno);
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
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Trabajador trabajadorNew = database.Trabajador.First(x => x.Id == id);
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            var campos = database.Campos.Select(x => new { x.Nombre, x.Empresa, x.Encargado }).Where(x => x.Empresa == empresa).ToList();
            int count = 0;
            string[] jefes = new string[campos.Count];
            string[] nombres = new string[campos.Count];
            foreach (var campo in campos)
            {
                if (!string.IsNullOrWhiteSpace(campo.Nombre))
                    nombres[count] = campo.Nombre;
                if (!string.IsNullOrWhiteSpace(campo.Encargado))
                    jefes[count] = campo.Encargado;
                count++;
            }
            var empresita = database.Empresas.First(x => x.Nombre == empresa); 
            var cargos = database.Cargo.Where(x => x.IdEmpresa == empresita.Id).ToList();
            List<String> nombresCargo = new List<string>();
            foreach (var cargo in cargos)
            {
                nombresCargo.Add(cargo.Nombre);
            }
            ViewBag.Cargos = nombresCargo;

            ViewBag.jefes = jefes.Distinct().ToList();
            ViewBag.Campos = nombres;
            if(trabajadorNew.Gerente == null)
                trabajadorNew.Gerente = "";
            if (trabajadorNew.Uid == null)
                trabajadorNew.Uid = "";
            if (trabajadorNew.TipoCuenta==null)
                trabajadorNew.TipoCuenta = "";
            if (trabajadorNew.Banco == null)
                trabajadorNew.Banco = "";
            if (trabajadorNew.NumeroCuenta == null)
                trabajadorNew.NumeroCuenta = trabajadorNew.Rut.Substring(0, trabajadorNew.Rut.Length-2);
            if (trabajadorNew.Contratado == null)
                trabajadorNew.Contratado = "No";
            if (trabajadorNew.CodPersonal == null)
                trabajadorNew.CodPersonal = "";
            if (trabajadorNew.EstadoCivil == null)
                trabajadorNew.EstadoCivil = "";
            if (trabajadorNew.FechaNacimiento == null)
                trabajadorNew.FechaNacimiento = new DateTime();
            if (trabajadorNew.Direccion == null)
                trabajadorNew.Direccion = "";
            if (trabajadorNew.Sexo == null)
                trabajadorNew.Sexo = "";
            if (trabajadorNew.Ciudad == null)
                trabajadorNew.Ciudad = "";
            if (trabajadorNew.CargasFamiliares == null)
                trabajadorNew.CargasFamiliares = "";
            if (trabajadorNew.CargasSimples == null)
                trabajadorNew.CargasSimples = 0;
            if (trabajadorNew.AFP == null)
                trabajadorNew.AFP = "";
            if (trabajadorNew.CodigoIntitucionSalud == null)
                trabajadorNew.CodigoIntitucionSalud = "";
            if (trabajadorNew.Salud == null)
                trabajadorNew.Salud = "";
            if (trabajadorNew.Telefono == null)
                trabajadorNew.Telefono = "";
            if (trabajadorNew.NombreEmer == null)
                trabajadorNew.NombreEmer = "";
            if (trabajadorNew.VinculoEmer == null)
                trabajadorNew.VinculoEmer = "";
            if (trabajadorNew.DireccionEmer == null)
                trabajadorNew.DireccionEmer = "";
            if (trabajadorNew.TelefonoEmer == null)
                trabajadorNew.TelefonoEmer = "";
            if (trabajadorNew.Cargo == null)
                trabajadorNew.Cargo = "";
            if (trabajadorNew.NombreJefe == null)
                trabajadorNew.NombreJefe = "";
            if (trabajadorNew.Campo == null)
                trabajadorNew.Campo = "";
            if (trabajadorNew.Empleador == null)
                trabajadorNew.Empleador = "";
            if (trabajadorNew.Contrato == null)
                trabajadorNew.Contrato = "";
            if (trabajadorNew.FechaTermino == null)
                trabajadorNew.FechaTermino = new DateTime();
            if (trabajadorNew.Jornada == null)
                trabajadorNew.Jornada = "";
            if (trabajadorNew.FechaIngreso == null)
                trabajadorNew.FechaIngreso = new DateTime();
            if (trabajadorNew.SueldoBase == null)
                trabajadorNew.SueldoBase = 0;
            if (trabajadorNew.Gratificacion == null)
                trabajadorNew.Gratificacion = 0;
            if (trabajadorNew.SueldoBruto == null)
                trabajadorNew.SueldoBruto = 0;
            if(trabajadorNew.FotoCarnet!=null)
            {
                Utils.SessionManager.FotoCarnet = trabajadorNew.FotoCarnet;
            }
            return View(trabajadorNew);
        }
        // POST: Trabajador/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection, 
                [Bind(Include = "Id,Salida,SalidaA,Entrada,EntradaA,Nombre,ApellidoPaterno,ApellidoMaterno,Rut,TipoCuenta,Banco,NumeroCuenta,Contratado," +
                "CodPersonal,EstadoCivil,Nacionalidad,FechaNacimiento,Direccion,Sexo,Ciudad,Telefono,Email,CargasFamiliares," +
                "TrampoAsignacionFamiliar,CargasSimples,CargasMaternales,CargasInvalidas,RegimenPrevicional,AsignacionFamiliar," +
                "AsignacionFamiliarRetroactiva,ReintegroCargasFamiliares,SolicitudTrabajadorJoven,AFP,TipoLineaAFP,RentaImponibleAFP," +
                "CotizacionObligatoriaAFP,SIS,AhorroVoluntarioAFP,RentaImpSustitutiva,TasaPactadaSus,AporteIndemSus,NPeriodosSus," +
                "PeriodosDesdeSus,PuestoTrabajoPesado,PerodoHastaSus,PorcentajeTrabajoPesado,MontoTrabajoPesado,CodigoMovimientoDePersonal," +
                "InicioMovimientoPersonal,FinMovimientoPersonal,TrampoAFP,Jubiulado,SeguroCesantia,Mutual,Salud,NombreEmer,VinculoEmer," +
                "DireccionEmer,TelefonoEmer,Cargo,NombreJefe,Campo,Empleador,Contrato,Jornada,FechaIngreso,FechaTermino,SueldoBase," +
                "Gratificacion,SueldoBruto,TipoPago,FactorHE,Colacion,Movilizacion,DiasVacacionesAño,JefeDirecto,Gerente,Empresa," +
                "APVI,CodigoAPVI,NumeroAPVI,FormaPagoAPVI,CotizacionAPVI,CotizaciónDepositosConvenidos,APVC,CodigoAPVC,NumeroAPVC," +
                "FormaPagoAPVC,CotizacionTrabajadorAPVC,CotizacionEmpleadorAPVC,AfiliadoVoluntario,RutAV,ApellidoPaternoAV,ApellidoMaternoAV," +
                "NombresAV,CodigoMovimientoPersonalAV,DesdeAV,HastaAV,AFPAV,MontoCapitalizacionVoluntaria,MontoAV,PeriodosAV,CodigoExcaja," +
                "TasaExcaja,RentaImponibleIPS,CotizacionObligatoriaIPS,RentaImponibleDesahucio,CodigoExcajaDesahucio,TasaCotizacionDesahucio," +
                "CotizaciónDesahucio,CotizacionFonasa,CotizacionISL,BonificacionLey,DescuentoPorCargasFamiliaresISL,BonosGobierno," +
                "CodigoIntitucionSalud,NumeroFun,RentaImponibleIsapre,MonedaDelPlanPactadoIsapre,CotizacionPactada,CotizacionIsapre," +
                "CotizacionIsapreAV,MontoGES,CodigoCCAF,RentaImponibleCCAF,CreditosPersonalesCCAF,DescuentoDentalCCAF,DescuentosPorLeasing," +
                "DescuentoPorSeguroDeVidaCCAF,OtrosDescuentosCCAF,CotizacionCCAFNoAfiniladosIsapres,DescuentoCargasFamiliaresCCAF," +
                "OtrosDescuentosCCAF1,OtrosDescuentosCCAF2,BonosGobiernoCCAF,CodigoSucursalCCAF,CodigoMutualidad,RentaImponibleMutual," +
                "CotizacionAccidenteMutual,SucursalParaPagoMutual,RentaImponibleSeguroCensantia,AporteTrabajadorSeguroCensatia," +
                "AporteEmpleadorSeguroCesantia,Subsidio,DatosEmpresa")] 
                 Trabajador trabajadorNew)
        {
            try
            {
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
                //Trabajador trabajadorNew = database.Trabajador.First(x => x.Id == id);
                /*trabajadorNew.Nombre = collection["Nombre"];
                trabajadorNew.ApellidoPaterno = collection["ApellidoPaterno"];
                trabajadorNew.ApellidoMaterno = collection["ApellidoMaterno"];
                trabajadorNew.Rut = collection["Rut"];
                trabajadorNew.Rut = formatearRut(trabajadorNew.Rut);
                trabajadorNew.Gerente = collection["Gerente"];
                trabajadorNew.TipoCuenta = collection["TipoCuenta"];
                trabajadorNew.Banco = collection["Banco"];
                trabajadorNew.NumeroCuenta = collection["NumeroCuenta"];
                trabajadorNew.Contratado = collection["Contratado"];
                trabajadorNew.CodPersonal = collection["CodPersonal"];
                trabajadorNew.EstadoCivil = collection["EstadoCivil"];
                if (DateTime.TryParse(collection["FechaNacimiento"], out temp))
                {
                    trabajadorNew.FechaNacimiento = Convert.ToDateTime(collection["FechaNacimiento"]);
                }
                trabajadorNew.Direccion = collection["Direccion"];
                trabajadorNew.Sexo = collection["Sexo"];
                trabajadorNew.Ciudad = collection["Ciudad"];
                trabajadorNew.CargasFamiliares = collection["CargasFamiliares"];
                trabajadorNew.CargasSimples = Convert.ToInt32(collection["CargasSimples"]);
                trabajadorNew.AFP = collection["AFP"];
                trabajadorNew.Salud = collection["Salud"];
                trabajadorNew.CodigoIntitucionSalud = trabajadorNew.Salud;
                trabajadorNew.Telefono = collection["Telefono"];
                trabajadorNew.NombreEmer = collection["NombreEmer"];
                trabajadorNew.VinculoEmer = collection["VinculoEmer"];
                trabajadorNew.DireccionEmer = collection["DireccionEmer"];
                trabajadorNew.TelefonoEmer = collection["TelefonoEmer"];
                trabajadorNew.Cargo = collection["Cargo"];
                trabajadorNew.NombreJefe = collection["NombreJefe"];
                trabajadorNew.Campo = collection["Campo"];
                trabajadorNew.Empleador = collection["Empleador"];
                trabajadorNew.Contrato = collection["Contrato"];
                if (DateTime.TryParse(collection["FechaTermino"], out temp))
                {
                    trabajadorNew.FechaTermino = Convert.ToDateTime(collection["FechaTermino"]);
                }
                else
                {
                    trabajadorNew.FechaTermino = null;
                }
                trabajadorNew.Jornada = collection["Jornada"];
                if (DateTime.TryParse(collection["FechaIngreso"], out temp))
                {
                    trabajadorNew.FechaIngreso = Convert.ToDateTime(collection["FechaIngreso"]);
                }

                trabajadorNew.SueldoBase = Convert.ToInt32(collection["SueldoBase"]);
                trabajadorNew.Gratificacion = Convert.ToInt32(collection["Gratificacion"]);
                trabajadorNew.SueldoBruto = Convert.ToInt32(collection["SueldoBruto"]);
                trabajadorNew.Gerente = collection["Gerente"];*/

                Utils.SessionManager.log("Editar trabajador: " + trabajadorNew.Nombre + " " + trabajadorNew.ApellidoPaterno + " " + trabajadorNew.ApellidoMaterno);
                trabajadorNew.Rut = formatearRut(trabajadorNew.Rut);
                HttpPostedFileBase postedFile = Request.Files["Foto"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    trabajadorNew.FotoCarnet = getImageFromPostfile(postedFile, 850);
                }
                trabajadorNew.Empresa = empresa;
                if (trabajadorNew.FotoCarnet == null && Utils.SessionManager.FotoCarnet != null)
                {
                    trabajadorNew.FotoCarnet = Utils.SessionManager.FotoCarnet;
                    Utils.SessionManager.FotoCarnet = null;
                }
                if (String.IsNullOrEmpty(collection["aviso"]))
                {
                    Trabajador trabajador1 = null;
                    try
                    {
                        string uid = collection["Uid"];
                        trabajador1 = database.Trabajador.First(x => x.Uid == uid);
                    }
                    catch { }
                    if (trabajador1 != null && collection["Uid"] != trabajador1.Uid)
                    {
                        ViewBag.texto = "Uid en uso, en\n " + trabajador1.Nombre + " " + trabajador1.ApellidoPaterno + " " + trabajador1.ApellidoMaterno + "\n " + trabajador1.Rut;
                        ViewBag.aviso = "Avisado";
                        datosCreate();
                        return View(trabajadorNew);
                    }
                }
                if (collection["Uid"] != null && collection["Uid"] != String.Empty)
                {
                    Trabajador trabajadorUid = new Trabajador();
                    try
                    {
                        trabajadorUid = database.Trabajador.First(x => x.Id == trabajadorNew.Id);
                    }
                    catch { }

                    if (trabajadorUid != null)
                    {
                        if (trabajadorUid.Uid == collection["Uid"])
                            trabajadorNew.Uid = collection["Uid"];
                        else
                            trabajadorNew.Uid = cambiarUidRegistros(collection["Uid"], trabajadorUid.Uid);
                    }
                    /*else
                    {
                        cambiarUidCrear(collection["Uid"]);
                    }*/
                }
                if (!validarRut(trabajadorNew.Rut))
                {
                    ViewBag.texto = "Rut no válido";
                    datosCreate();
                    return View(trabajadorNew);
                }
                try
                {
                    Trabajador trabajador = database.Trabajador.First(x => x.Rut == trabajadorNew.Rut);
                    if(trabajador.Id != trabajadorNew.Id)
                    {
                        ViewBag.texto = "Ya existe el rut";
                        datosCreate();
                        return View(trabajadorNew);
                    }
                }
                catch { }
                if (ModelState.IsValid)
                {
                    db.Entry(trabajadorNew).State = EntityState.Modified;
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
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Trabajador trabajador = database.Trabajador.First(x => x.Id == id);
            Utils.SessionManager.log("Eliminar trabajador: " + trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno);
            database.Trabajador.Remove(trabajador);
            database.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        private string cambiarUidCrear(string Uid)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
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
                if(registros.Count>0)
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
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            List<Empresas> nombre = database.Empresas.Where(x => x.Id == 1005).ToList();
            var registros = database.RegistroTrabajador.Select(x => new { x.Uid }).Where(x => x.Uid == Uid).Distinct().ToList();
            var trabajadores = database.Trabajador.Select(x => new { x.Uid }).Where(x => x.Uid == Uid).Distinct().ToList();
            bool exist = false;
            string result = "";
            if(trabajadores.Count>0)
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
            if (registros.Count>0)
            {
                var registers = database.RegistroTrabajador.Where(x => x.Uid == Uid).ToList();
                foreach(var registro in registers)
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
            if(exist)
            {
                int numero = Convert.ToInt32(nombre[0].Nombre);
                nombre[0].Nombre = (numero + 1).ToString();
                database.SaveChanges();
            }
            return result;
        }

        private string cambiarUidRegistros(string UidNuevo, string UidAterior)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            try
            {
                Trabajador trabajador = database.Trabajador.First(x => x.Uid == UidNuevo);
                if (trabajador != null)
                {
                    cambiarUidCrear(UidNuevo);
                }
            }
            catch{}

            List<RegistroTrabajador> registros = database.RegistroTrabajador.Where(x => x.Uid == UidAterior).ToList();
            foreach (var registro in registros)
            {
                registro.Uid = UidNuevo;
            }
            database.SaveChanges();
            return UidNuevo;

        }
        // POST: Trabajador/Delete/5
    }
}