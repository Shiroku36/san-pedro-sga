using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using ControlPersonalAppWeb.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SpreadsheetLight;
using System.Web.Helpers;

namespace ControlPersonalAppWeb.Controllers
{
    public class InformesController : Controller
    {
        DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();
        public ActionResult Horas()
        {
            HttpPostedFileBase postedFile = Request.Files["Foto"];
            if (postedFile != null && postedFile.ContentLength > 0)
            {
                var documento = postedFile;
                ViewBag.foto = Request.Files["Foto"];
            }
            return View();
        }
        [HttpPost]
        public ActionResult Horas(FormCollection collection)
        {
            int horario = 0;
            if(!String.IsNullOrEmpty(collection["horario"]))
            {
                horario = 1;
            }
            string strHeader = "Informe Horas extras\n";//Convert.ToDateTime(collection["Periodo"]).ToLongDateString();
            DBManejoPersonalEntities db = new DBManejoPersonalEntities();
            if (!String.IsNullOrEmpty(collection["titulo"]))
            {
                strHeader = collection["titulo"];
            }
            
            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            Document document = new Document();
            document.SetPageSize(iTextSharp.text.PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, Response.OutputStream);
            document.Open();
            BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fntHead = new Font(bfntHead, 12, 1, BaseColor.DARK_GRAY);
            Paragraph prgHeading = new Paragraph();
            prgHeading.Alignment = Element.ALIGN_CENTER;
            prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
            document.Add(prgHeading);
            Font fnthorario= new Font(bfntHead, 10, 1, BaseColor.DARK_GRAY);
            document.Add((new Paragraph("Horario"+ "\n"+" Lunes a jueves 08:00 a 12:00 de 13:30 a 17:30\n"
                                        +"Viernes de 07:30 a 12:00 de 13:30 a 17:30\n"+
                                        "Sabado de 07:30 a 12:00", fnthorario) { Alignment = Element.ALIGN_CENTER }));
            document.Add(new Paragraph(new Chunk("\n")));
            string nombreEquipoHorasExtras = "";
            string nombreEquipoHorasTrabajadas = "";
            string nombreEquipoHorasMuertas = "";
            DateTime maxHorasTrabajadas = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime maxHorasExtras = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime maxHorasMuertas = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            List<Equipo> listaHorasExtras = new List<Equipo>();
            List<Equipo> listaHorasTrabajadas = new List<Equipo>();
            List<Equipo> listaHorasMuertas = new List<Equipo>();
            BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fntColumnHeader = new Font(btnColumnHeader, 8, 1, BaseColor.WHITE);
            Font fntCell = new Font(btnColumnHeader, 10);
            for (var i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase hpf = Request.Files[i];

                if (hpf != null && hpf.ContentLength > 0)
                {
                    StreamReader csvreader = new StreamReader(hpf.InputStream);
                    var line1 = csvreader.ReadLine();
                    line1 = csvreader.ReadLine();
                    if(!String.IsNullOrEmpty(line1))
                    {
                        var nombre = line1.Split(';');
                        string nombreEquipo = nombre[2];
                        document.Add((new Paragraph("Equipo: "+ nombreEquipo, fntHead) { Alignment = Element.ALIGN_CENTER }));
                        document.Add(new Paragraph(new Chunk("\n")));
                        hpf.InputStream.Position = 0;
                        csvreader.DiscardBufferedData();
                        line1 = csvreader.ReadLine();

                        PdfPTable declaracion = new PdfPTable(1);
                        //declaracion.WidthPercentage = 100f;
                        int num = 4;
                        PdfPTable table = new PdfPTable(num);
                        //table.SetWidths(new int[] { 1, 2, 1, 1, 1, 1, 1 });
                        List<string> titulos = new List<string>();
                        titulos.Add("Fecha");
                        titulos.Add("Horas trabajadas");
                        titulos.Add("Tiempo inactivo");
                        titulos.Add("Horas extras");
                        int j;
                        for (j = 0; j < num; j++)
                        {
                            PdfPCell cell = new PdfPCell();
                            cell.BackgroundColor = BaseColor.GRAY;
                            cell.AddElement(new Chunk(titulos[j], fntColumnHeader));
                            table.AddCell(cell);
                        }

                    
                        DateTime tiempoMuerto = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime horaAnterior = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime diaMaxHorasExtras = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime diaMaxTrabajado = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime salidaSabado = DateTime.ParseExact("2000-01-02 12:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime salida = DateTime.ParseExact("2000-01-02 17:30:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime trabajado = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime totalTrabajado = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime totalHoras = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime totalMuerto = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime horasxdia = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime maxHoras = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime maxDia = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        string anterior = "";
                        while (!csvreader.EndOfStream)
                        {
                            var line = csvreader.ReadLine();
                            var values = line.Split(';');
                            string hora = values[7];
                            string fecha = values[3];
                            string date = fecha +" "+ hora;
                            DateTime dateTime = DateTime.Parse(date);//, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        

                            if (anterior != fecha && anterior != "")
                            {
                                table.AddCell(new Phrase(anterior, fntCell));
                                table.AddCell(new Phrase(trabajado.ToShortTimeString(), fntCell));
                                table.AddCell(new Phrase(tiempoMuerto.ToShortTimeString(), fntCell));
                                table.AddCell(new Phrase(horasxdia.ToShortTimeString(), fntCell));
                                DateTime dia = DateTime.Parse(anterior);//, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                if (TimeSpan.Compare(trabajado.TimeOfDay, maxDia.TimeOfDay) > 0)
                                {
                                    maxDia = DateTime.Parse(anterior+" "+trabajado.ToLongTimeString());//, "yyyy-MM-dd H:mm:ss", CultureInfo.InvariantCulture);
                                }
                                if (TimeSpan.Compare(horasxdia.TimeOfDay, maxHoras.TimeOfDay) > 0)
                                {
                                    maxHoras = DateTime.Parse(anterior + " " + horasxdia.ToLongTimeString());//, "yyyy-MM-dd H:mm:ss", CultureInfo.InvariantCulture);
                                }
                                totalHoras = totalHoras.AddHours(horasxdia.Hour);
                                totalHoras = totalHoras.AddMinutes(horasxdia.Minute);
                                totalHoras = totalHoras.AddSeconds(horasxdia.Second);


                                totalTrabajado = totalTrabajado.AddHours(trabajado.Hour);
                                totalTrabajado = totalTrabajado.AddMinutes(trabajado.Minute);
                                totalTrabajado = totalTrabajado.AddSeconds(trabajado.Second);

                                totalMuerto = totalMuerto.AddHours(tiempoMuerto.Hour);
                                totalMuerto = totalMuerto.AddMinutes(tiempoMuerto.Minute);
                                totalMuerto = totalMuerto.AddSeconds(tiempoMuerto.Second);

                                trabajado = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                horasxdia = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                tiempoMuerto = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                horaAnterior = DateTime.ParseExact("2000-01-02 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                            DateTime desde = new DateTime();
                            DateTime hasta = new DateTime();
                            try
                            {
                                desde = DateTime.Parse(values[3] + " " + values[4]);//, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                hasta = DateTime.Parse(values[5] + " " + values[6]);//, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                ViewBag.texto = "Archivo "+nombreEquipo +" no válido";
                                document.Dispose();
                                document.Close();
                                writer.Dispose();
                                writer.Close();
                                hpf = null;
                                return View();
                            }
                            if(horaAnterior.Year!=2000 )
                            {
                                tiempoMuerto = tiempoMuerto.Add((desde-horaAnterior));
                            }
                            horaAnterior = hasta;
                            desde = desde.AddHours(horario);
                            hasta = hasta.AddHours(horario);
                            int diaHorario = (int)desde.DayOfWeek;

                            if (diaHorario == 6)
                            {
                                if (TimeSpan.Compare(desde.TimeOfDay, salidaSabado.TimeOfDay) > 0)
                                {
                                    horasxdia = horasxdia.AddHours(dateTime.Hour);
                                    horasxdia = horasxdia.AddMinutes(dateTime.Minute);
                                    horasxdia = horasxdia.AddSeconds(dateTime.Second);
                                }
                                else if (TimeSpan.Compare(hasta.TimeOfDay, salidaSabado.TimeOfDay) > 0)
                                {
                                    horasxdia = horasxdia.AddHours(hasta.Hour - 12);
                                    horasxdia = horasxdia.AddMinutes(hasta.Minute);
                                    horasxdia = horasxdia.AddSeconds(hasta.Second);
                                }
                            }
                            else
                            {
                                if (TimeSpan.Compare(desde.TimeOfDay, salida.TimeOfDay) > 0)
                                {
                                    horasxdia = horasxdia.AddHours(dateTime.Hour);
                                    horasxdia = horasxdia.AddMinutes(dateTime.Minute);
                                    horasxdia = horasxdia.AddSeconds(dateTime.Second);
                                }
                                else if (TimeSpan.Compare(hasta.TimeOfDay, salida.TimeOfDay) > 0)
                                {
                                    horasxdia = horasxdia.AddHours(hasta.Hour - 17);
                                    horasxdia = horasxdia.AddMinutes(hasta.Minute - 30);
                                    horasxdia = horasxdia.AddSeconds(hasta.Second);
                                }
                            }

                            trabajado = trabajado.AddHours(dateTime.Hour);
                            trabajado = trabajado.AddMinutes(dateTime.Minute);
                            trabajado = trabajado.AddSeconds(dateTime.Second);


                            anterior = fecha;

                        }

                        document.Add(table);
                        document.Add(new Paragraph(new Chunk("\n")));
                         PdfPTable totales = new PdfPTable(2);

                        totales.AddCell((new Paragraph("Total horas trabajadas", fntCell) { Alignment = Element.ALIGN_CENTER }));
                        if (totalTrabajado.Day != 2)
                        {
                            int hour = totalTrabajado.Hour + (totalTrabajado.Day-2)*24;
                            int min = totalTrabajado.Minute;
                            totales.AddCell((new Paragraph(hour + ":" + min, fntCell) { Alignment = Element.ALIGN_CENTER }));
                        }
                        else
                        {
                            totales.AddCell((new Paragraph(totalTrabajado.ToShortTimeString(), fntCell) { Alignment = Element.ALIGN_CENTER }));
                        }

                        totales.AddCell((new Paragraph("Total horas extras", fntCell) { Alignment = Element.ALIGN_CENTER }));
                        if (totalHoras.Day != 2)
                        {
                            int hour = totalHoras.Hour + (totalHoras.Day - 2) * 24;
                            int min = totalHoras.Minute;
                            totales.AddCell((new Paragraph(hour + ":" + min, fntCell) { Alignment = Element.ALIGN_CENTER }));
                        }
                        else
                        {
                            totales.AddCell((new Paragraph(totalHoras.ToShortTimeString(), fntCell) { Alignment = Element.ALIGN_CENTER }));
                        }

                        totales.AddCell((new Paragraph("Total tiempo inactivo", fntCell) { Alignment = Element.ALIGN_CENTER }));
                        if (totalMuerto.Day != 2)
                        {
                            int hour = totalMuerto.Hour + (totalMuerto.Day - 2) * 24;
                            int min = totalMuerto.Minute;
                            totales.AddCell((new Paragraph(hour + ":" + min, fntCell) { Alignment = Element.ALIGN_CENTER }));
                        }
                        else
                        {
                            totales.AddCell((new Paragraph(totalHoras.ToShortTimeString(), fntCell) { Alignment = Element.ALIGN_CENTER }));
                        }
                        totales.AddCell((new Paragraph("Día más trabajado", fntCell) { Alignment = Element.ALIGN_CENTER }));
                        totales.AddCell((new Paragraph(maxDia.ToShortDateString(), fntCell) { Alignment = Element.ALIGN_CENTER }));
                        document.Add(totales);
                        document.Add(new Paragraph(new Chunk("\n")));
                        document.NewPage();
                        listaHorasTrabajadas.Add(new Equipo() { Nombre = nombreEquipo, Horas = totalTrabajado });
                        listaHorasExtras.Add(new Equipo() { Nombre = nombreEquipo, Horas = totalHoras });
                        listaHorasMuertas.Add(new Equipo() { Nombre = nombreEquipo, Horas = totalMuerto });
                        if (DateTime.Compare(totalHoras, maxHorasExtras) > 0)
                        {
                            nombreEquipoHorasExtras = nombreEquipo;
                            maxHorasExtras = totalHoras;
                        }
                        if (DateTime.Compare(totalTrabajado, maxHorasTrabajadas) > 0)
                        {
                            nombreEquipoHorasTrabajadas = nombreEquipo;
                            maxHorasTrabajadas = totalTrabajado;
                        }
                        if (DateTime.Compare(totalMuerto, maxHorasMuertas) > 0)
                        {
                            nombreEquipoHorasMuertas = nombreEquipo;
                            maxHorasMuertas = totalMuerto;
                        }
                    }
                }

            }
            document.NewPage();
            PdfPTable tablaHorasExtras = new PdfPTable(2);
            PdfPTable tablaHorasTotales = new PdfPTable(2);
            PdfPTable tablaHorasMuertas = new PdfPTable(2);

            listaHorasExtras = listaHorasExtras.OrderByDescending(x => x.Horas).ToList();
            listaHorasTrabajadas = listaHorasTrabajadas.OrderByDescending(x => x.Horas).ToList();
            listaHorasMuertas = listaHorasMuertas.OrderByDescending(x => x.Horas).ToList();

            document.Add(new Paragraph(new Chunk("Horas extras")) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph(new Chunk("\n")));
            tablaHorasExtras.AddCell((new Paragraph("Equipo", fntCell) { Alignment = Element.ALIGN_CENTER }));
            tablaHorasExtras.AddCell((new Paragraph("Horas", fntCell) { Alignment = Element.ALIGN_CENTER }));
            foreach ( var horas in listaHorasExtras)
            {
                if (horas.Horas.Day != 2)
                {
                    int hour = horas.Horas.Hour + (horas.Horas.Day - 2) * 24;
                    int min = horas.Horas.Minute;
                    tablaHorasExtras.AddCell((new Paragraph(horas.Nombre, fntCell) { Alignment = Element.ALIGN_CENTER }));
                    tablaHorasExtras.AddCell((new Paragraph(hour + ":" + min, fntCell) { Alignment = Element.ALIGN_CENTER }));
                }
                else
                {
                    tablaHorasExtras.AddCell((new Paragraph(horas.Nombre, fntCell) { Alignment = Element.ALIGN_CENTER }));
                    tablaHorasExtras.AddCell((new Paragraph(horas.Horas.ToShortTimeString(), fntCell) { Alignment = Element.ALIGN_CENTER }));
                }
            }
            document.Add(tablaHorasExtras);
            document.Add(new Paragraph(new Chunk("\n")));
            document.NewPage();

            document.Add(new Paragraph(new Chunk("Horas trabajadas")) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph(new Chunk("\n")));
            tablaHorasTotales.AddCell((new Paragraph("Equipo", fntCell) { Alignment = Element.ALIGN_CENTER }));
            tablaHorasTotales.AddCell((new Paragraph("Horas", fntCell) { Alignment = Element.ALIGN_CENTER }));
            foreach (var horas in listaHorasTrabajadas)
            {
                if (horas.Horas.Day != 2)
                {
                    int hour = horas.Horas.Hour + (horas.Horas.Day - 2) * 24;
                    int min = horas.Horas.Minute;
                    tablaHorasTotales.AddCell((new Paragraph(horas.Nombre, fntCell) { Alignment = Element.ALIGN_CENTER }));
                    tablaHorasTotales.AddCell((new Paragraph(hour + ":" + min, fntCell) { Alignment = Element.ALIGN_CENTER }));
                }
                else
                {
                    tablaHorasTotales.AddCell((new Paragraph(horas.Nombre, fntCell) { Alignment = Element.ALIGN_CENTER }));
                    tablaHorasTotales.AddCell((new Paragraph(horas.Horas.ToShortTimeString(), fntCell) { Alignment = Element.ALIGN_CENTER }));
                }
            }

            document.Add(tablaHorasTotales);
            document.Add(new Paragraph(new Chunk("\n")));
            document.NewPage();

            document.Add(new Paragraph(new Chunk("Horas de inactividad")) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph(new Chunk("\n")));
            tablaHorasMuertas.AddCell((new Paragraph("Equipo", fntCell) { Alignment = Element.ALIGN_CENTER }));
            tablaHorasMuertas.AddCell((new Paragraph("Horas", fntCell) { Alignment = Element.ALIGN_CENTER }));
            foreach (var horas in listaHorasMuertas)
            {
                if (horas.Horas.Day != 2)
                {
                    int hour = horas.Horas.Hour + (horas.Horas.Day - 2) * 24;
                    int min = horas.Horas.Minute;
                    tablaHorasMuertas.AddCell((new Paragraph(horas.Nombre, fntCell) { Alignment = Element.ALIGN_CENTER }));
                    tablaHorasMuertas.AddCell((new Paragraph(hour + ":" + min, fntCell) { Alignment = Element.ALIGN_CENTER }));
                }
                else
                {
                    tablaHorasMuertas.AddCell((new Paragraph(horas.Nombre, fntCell) { Alignment = Element.ALIGN_CENTER }));
                    tablaHorasMuertas.AddCell((new Paragraph(horas.Horas.ToShortTimeString(), fntCell) { Alignment = Element.ALIGN_CENTER }));
                }
            }
            document.Add(tablaHorasMuertas);
            document.Add(new Paragraph(new Chunk("\n")));
            //document.NewPage();

            /*

            PdfPTable final = new PdfPTable(3);
            final.AddCell((new Paragraph("Más horas trabajadas en el mes", fntCell) { Alignment = Element.ALIGN_CENTER }));
            final.AddCell((new Paragraph(nombreEquipoHorasTrabajadas, fntCell) { Alignment = Element.ALIGN_CENTER }));
            if (maxHorasTrabajadas.Day != 2)
            {
                int hour = maxHorasTrabajadas.Hour + (maxHorasTrabajadas.Day - 2) * 24;
                int min = maxHorasTrabajadas.Minute;
                final.AddCell((new Paragraph(hour + ":" + min, fntCell) { Alignment = Element.ALIGN_CENTER }));
            }
            else
            {
                final.AddCell((new Paragraph(nombreEquipoHorasTrabajadas, fntCell) { Alignment = Element.ALIGN_CENTER }));
                final.AddCell((new Paragraph(maxHorasTrabajadas.ToShortTimeString(), fntCell) { Alignment = Element.ALIGN_CENTER }));
            }
            final.AddCell((new Paragraph("Más horas extras en el mes", fntCell) { Alignment = Element.ALIGN_CENTER }));
            final.AddCell((new Paragraph(nombreEquipoHorasExtras, fntCell) { Alignment = Element.ALIGN_CENTER }));
            if (maxHorasExtras.Day != 2)
            {
                int hour = maxHorasExtras.Hour + (maxHorasExtras.Day - 2) * 24;
                int min = maxHorasExtras.Minute;
                final.AddCell((new Paragraph(hour + ":" + min, fntCell) { Alignment = Element.ALIGN_CENTER }));
            }
            else
            {
                final.AddCell((new Paragraph(maxHorasExtras.ToShortTimeString(), fntCell) { Alignment = Element.ALIGN_CENTER }));
            }

            final.AddCell((new Paragraph("Más horas inactivo en el mes", fntCell) { Alignment = Element.ALIGN_CENTER }));
            final.AddCell((new Paragraph(nombreEquipoHorasMuertas, fntCell) { Alignment = Element.ALIGN_CENTER }));
            if (maxHorasMuertas.Day != 2)
            {
                int hour = maxHorasMuertas.Hour + (maxHorasMuertas.Day - 2) * 24;
                int min = maxHorasMuertas.Minute;
                final.AddCell((new Paragraph(hour + ":" + min, fntCell) { Alignment = Element.ALIGN_CENTER }));
            }
            else
            {
                final.AddCell((new Paragraph(maxHorasMuertas.ToShortTimeString(), fntCell) { Alignment = Element.ALIGN_CENTER }));
            }

            document.Add(final);*/
            document.Add(new Paragraph(new Chunk("\n")));

            document.Close();
            writer.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + "Informe horas extras.pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(document);
            Response.End();

            return View();
        }// GET: Informes

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

        public ActionResult Index()
        {

            //List<Campos> campos = new List<Campos>();

            Utils.SessionManager.log("Index informes");
            Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();
            string empresa = cuenta.Empresa;
            string[] nombres;
            List<string> listaNombres = new List<string>();
            List<Campos> campos = new List<Campos>();
            if (empresa == "JCP")
            {
                nombres = GetNombreCampos("");
            }
            else
            {
                nombres = GetNombreCampos(empresa);
            }
            listaNombres.Add("Todos");
            foreach (var campo in nombres)
            {
                if (cuenta.Permisos.Contains(campo))
                {
                    //campos.AddRange(db.Campos.Where(x => x.Nombre == campo).ToList());
                    listaNombres.Add(campo);
                }
            }
            ViewBag.campos = listaNombres;
            return View();


        }
        [HttpPost]
        public ActionResult Index(FormCollection collection,int? id= null)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            var rut = string.IsNullOrWhiteSpace(collection["Rut"]);
            var inicio = string.IsNullOrWhiteSpace(collection["Inicio"]);
            var fin = string.IsNullOrWhiteSpace(collection["Fin"]);
            var campo = string.IsNullOrWhiteSpace(collection["Campo"]);

            if (!string.IsNullOrWhiteSpace(collection["Entrada"]))
            {
                Utils.SessionManager.entrada = collection["Entrada"];
            }
            if (!string.IsNullOrWhiteSpace(collection["Salida"]))
            {
                Utils.SessionManager.salida = collection["Salida"];
            }
            if (!string.IsNullOrWhiteSpace(collection["Almuerzo"]))
            {
                Utils.SessionManager.almuerzo = collection["Almuerzo"];
            }
            if(!campo)
            {
                if(collection["Campo"]=="Todos")
                {
                    campo = true;
                }
            }
            //Una persona
            if (!rut && inicio && fin && campo) 
            {
                string rutIn = formatearRut(collection["Rut"]);
                Trabajador trabajadorNew = database.Trabajador.First(x => x.Rut == rutIn);
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFRegistro", "Trabajador", new { id = trabajadorNew.Id });
            }
            //Una persona y una campo
            if (!rut && inicio && fin && !campo) 
            {
                string rutIn = collection["Rut"];
                TrabajadorIndex trabajadorNew = database.Trabajador.Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoMaterno = x.ApellidoMaterno, ApellidoPaterno = x.ApellidoPaterno , Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa,
                    Entrada = x.Entrada,
                    EntradaA = x.EntradaA,
                    Salida = x.Salida,
                    SalidaA = x.SalidaA
                }).First(x => x.Rut == rutIn);
                Utils.SessionManager.trabajadores = new List<TrabajadorIndex> { trabajadorNew };
                Utils.SessionManager.campo = collection["Campo"];
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFRegistroFecha", "Trabajador");
            }
            //Un día
            if(!inicio && fin && rut && campo  )
            {
                DateTime inicioIn = Convert.ToDateTime(collection["Inicio"]);
                string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
                List<TrabajadorIndex> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre,
                    ApellidoMaterno = x.ApellidoMaterno,
                    ApellidoPaterno = x.ApellidoPaterno,
                    Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa,
                    Entrada = x.Entrada,
                    EntradaA = x.EntradaA,
                    Salida = x.Salida,
                    SalidaA = x.SalidaA
                }).ToList();
                trabajadores.Insert(0, new TrabajadorIndex { Nombre = inicioIn.ToShortDateString() });
                Utils.SessionManager.trabajadores = trabajadores;
                Utils.SessionManager.inicio = inicioIn;
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFRegistroFecha", "Trabajador");
            }
            //Un día en un campo
            if (!inicio && fin && rut && !campo)
            {
                DateTime inicioIn = Convert.ToDateTime(collection["Inicio"]);
                string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
                List<TrabajadorIndex> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre,
                    ApellidoMaterno = x.ApellidoMaterno,
                    ApellidoPaterno = x.ApellidoPaterno,
                    Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa,
                    Entrada = x.Entrada,
                    EntradaA = x.EntradaA,
                    Salida = x.Salida,
                    SalidaA = x.SalidaA
                }).ToList();
                trabajadores.Insert(0, new TrabajadorIndex { Nombre = inicioIn.ToShortDateString() });
                Utils.SessionManager.trabajadores = trabajadores;
                Utils.SessionManager.inicio = inicioIn;
                Utils.SessionManager.tipo = collection["subject"];
                Utils.SessionManager.campo = collection["Campo"];
                return RedirectToAction("PDFRegistroFecha", "Trabajador");
            }
            //Un campo
            if (!campo && inicio && fin && rut)
            {
                string campoIn = collection["Campo"];
                var idCampo = database.Campos.First(x => x.Nombre == campoIn);
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFCampo", "Trabajador", new { id = idCampo.Id  });
            }
            //Rando de fecha
            if (!inicio && !fin && rut && campo)
            {
                string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
                List<TrabajadorIndex> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre,
                    ApellidoMaterno = x.ApellidoMaterno,
                    ApellidoPaterno = x.ApellidoPaterno,
                    Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa,
                    Entrada = x.Entrada,
                    EntradaA = x.EntradaA,
                    Salida = x.Salida,
                    SalidaA = x.SalidaA
                }).ToList();
                trabajadores.Insert(0, new TrabajadorIndex { Nombre = collection["Inicio"] +" a "+ collection["Fin"] });
                Utils.SessionManager.trabajadores = trabajadores;
                Utils.SessionManager.inicio = Convert.ToDateTime(collection["Inicio"]);
                Utils.SessionManager.fin = Convert.ToDateTime(collection["Fin"]); ;
                Utils.SessionManager.campo = null;
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFRegistroFecha", "Trabajador");
            }
            //Rango de fecha y un campo
            if (!inicio && !fin && rut && !campo)
            {
                string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
                List<TrabajadorIndex> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre,
                    ApellidoMaterno = x.ApellidoMaterno,
                    ApellidoPaterno = x.ApellidoPaterno,
                    Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa,
                    Entrada = x.Entrada,
                    EntradaA = x.EntradaA,
                    Salida = x.Salida,
                    SalidaA = x.SalidaA
                }).ToList();
                trabajadores.Insert(0, new TrabajadorIndex { Nombre = collection["Inicio"] + " a " + collection["Fin"] +" en "+ collection["Campo"] });
                Utils.SessionManager.trabajadores = trabajadores;
                Utils.SessionManager.inicio = Convert.ToDateTime(collection["Inicio"]);
                Utils.SessionManager.fin = Convert.ToDateTime(collection["Fin"]);
                Utils.SessionManager.campo = collection["Campo"];
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFRegistroFecha", "Trabajador");
            }
            //Rando de fecha, un campo y una persona
            if (!inicio && !fin && !rut && !campo)
            {

                string rutIn = collection["Rut"];
                TrabajadorIndex trabajadorNew = database.Trabajador.Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre,
                    ApellidoMaterno = x.ApellidoMaterno,
                    ApellidoPaterno = x.ApellidoPaterno,
                    Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa,
                    Entrada = x.Entrada,
                    EntradaA = x.EntradaA,
                    Salida = x.Salida,
                    SalidaA = x.SalidaA
                }).First(x => x.Rut == rutIn);
                Utils.SessionManager.trabajadores = new List<TrabajadorIndex> { trabajadorNew };
                Utils.SessionManager.inicio = Convert.ToDateTime(collection["Inicio"]);
                Utils.SessionManager.fin = Convert.ToDateTime(collection["Fin"]);
                Utils.SessionManager.campo = collection["Campo"];
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFRegistroFecha", "Trabajador");
            }
            //Rango de fecha de una persona
            if (!inicio && !fin && !rut && campo)
            {

                string rutIn = collection["Rut"];
                TrabajadorIndex trabajadorNew = database.Trabajador.Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre,
                    ApellidoMaterno = x.ApellidoMaterno,
                    ApellidoPaterno = x.ApellidoPaterno,
                    Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa,
                    Entrada = x.Entrada,
                    EntradaA = x.EntradaA,
                    Salida = x.Salida,
                    SalidaA = x.SalidaA
                }).First(x => x.Rut == rutIn);
                Utils.SessionManager.trabajadores = new List<TrabajadorIndex> { trabajadorNew };
                Utils.SessionManager.inicio = Convert.ToDateTime(collection["Inicio"]);
                Utils.SessionManager.fin = Convert.ToDateTime(collection["Fin"]);
                Utils.SessionManager.campo = null;
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFRegistroFecha", "Trabajador");
            }
            //Un día una persona
            if (!inicio && fin && !rut && campo)
            {

                string rutIn = collection["Rut"];
                TrabajadorIndex trabajadorNew = database.Trabajador.Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre,
                    ApellidoMaterno = x.ApellidoMaterno,
                    ApellidoPaterno = x.ApellidoPaterno,
                    Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa,
                    Entrada = x.Entrada,
                    EntradaA = x.EntradaA,
                    Salida = x.Salida,
                    SalidaA = x.SalidaA
                }).First(x => x.Rut == rutIn);
                Utils.SessionManager.trabajadores = new List<TrabajadorIndex> { trabajadorNew };
                Utils.SessionManager.inicio = Convert.ToDateTime(collection["Inicio"]);
                Utils.SessionManager.fin = Convert.ToDateTime("01-01-0001");
                Utils.SessionManager.campo = null;
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFRegistroFecha", "Trabajador");
            }
            //Un día una persona un campo
            if (!inicio && fin && !rut && !campo)
            {

                string rutIn = collection["Rut"];
                TrabajadorIndex trabajadorNew = database.Trabajador.Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre,
                    ApellidoMaterno = x.ApellidoMaterno,
                    ApellidoPaterno = x.ApellidoPaterno,
                    Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa,
                    Entrada = x.Entrada,
                    EntradaA = x.EntradaA,
                    Salida = x.Salida,
                    SalidaA = x.SalidaA
                }).First(x => x.Rut == rutIn);
                Utils.SessionManager.trabajadores = new List<TrabajadorIndex> { trabajadorNew };
                Utils.SessionManager.inicio = Convert.ToDateTime(collection["Inicio"]);
                Utils.SessionManager.fin = Convert.ToDateTime("01-01-0001");
                Utils.SessionManager.campo = collection["Campo"];
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFRegistroFecha", "Trabajador");
            }
            //Sin nada
            if(inicio && fin && rut && campo)
            {
                Utils.SessionManager.tipo = collection["subject"];
                return RedirectToAction("PDFConsolidado", "Trabajador");
            }
            return RedirectToAction("Index");
        }
        // GET: Informes/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Informes/Create
        public ActionResult Asistencias()
        {
            DBManejoPersonalEntities db = new DBManejoPersonalEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa; 
            ViewBag.Trabajador = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoMaterno = x.ApellidoMaterno, ApellidoPaterno = x.ApellidoPaterno , Rut = x.Rut, 
                Entrada = x.Entrada,
                EntradaA = x.EntradaA,
                Salida = x.Salida,
                SalidaA = x.SalidaA
            }).ToList();
            return View();
        }

        // POST: Informes/Create
        [HttpPost]
        public ActionResult Asistencias(FormCollection collection)
        {
            try
            {
                DBManejoPersonalEntities db = new DBManejoPersonalEntities();
                string nombre = collection["id"];
                Empresas empresa = db.Empresas.First(x => x.Nombre == nombre);
                string periodo = collection["Periodo"];
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
                Utils.SessionManager.log("Asistencias informe");
                AsistenciasPDF(dateTime, ids, empresa);
                return RedirectToAction("Asistencias");
            }
            catch
            {
                return View();
            }
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
                Trabajador trabajador = db.Trabajador.First(x => x.Id == id);

                BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntCell = new Font(btnColumnHeader, 10);
                PdfPTable declaracion = new PdfPTable(1);
                declaracion.WidthPercentage = 100f;
                string texto = "Yo " + trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " +
                                trabajador.ApellidoMaterno + " Rut " +
                                formatearRut(trabajador.Rut) + " declaro  y acepto que he trabajado " +
                                GetDias(trabajador, dateTime) + " dias en el perido de " + dateTime.ToString("MMMM yyyy") +" , en la empresa " + empresa.Nombre 
                                + " " + empresa.Rut +
                    ", a mi entera satisfacción, la cantidad de días acá mostradas " +
                    "y no tengo cargos ni cobros posteriores asimismo acepto y reconozco " +
                    " la forma en como se determinó y las deducciones efectuadas.\n\n\n\n" +
                    "                                                               ______________________\n\n" +
                    "                                                                      Recibí conforme";
                declaracion.AddCell(new Phrase(texto, fntCell));
                document.Add(declaracion);
                document.Add(new Paragraph(new Chunk("\n")));
                //document.NewPage();
            }
            document.Close();
            writer.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + "Asistencia mensual " + dateTime.ToString(" MMMM yyyy ") + empresa.Nombre + ".pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(document);
            Response.End();
        }
        private string GetDias(Trabajador trabajador, DateTime dateTime)
        {
            DBManejoPersonalEntities db = new DBManejoPersonalEntities();
            int mes = dateTime.Month;
            var registros = db.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid && x.Fecha.Month == mes).Select(x => new { Fecha = x.Fecha }).ToList();
            registros = registros.OrderBy(o => o.Fecha).ToList();
            int dias = 0;
            DateTime fecha = DateTime.Now;
            foreach (var registro in registros)
            {

                if (registro.Fecha.Day != fecha.Day)
                {
                    dias++;
                }
                fecha = registro.Fecha;
            }
            return dias.ToString();
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
        // GET: Informes/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Informes/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Informes/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Informes/Delete/5
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
    }
}
