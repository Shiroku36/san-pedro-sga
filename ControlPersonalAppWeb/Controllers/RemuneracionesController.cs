using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using ControlPersonalAppWeb.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SpreadsheetLight;

namespace ControlPersonalAppWeb.Controllers
{
    public class RemuneracionesController : Controller
    {
        DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        public ActionResult Liquidacion()
        {

            ViewBag.Remuneracion = "Remuneraciones";
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            int id = db.Empresas.First(x => x.Nombre == empresa).Id;
            List<CentroDeCostos> centrosDeCostos = new List<CentroDeCostos>();
            ViewBag.centros = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, Rut = x.Rut, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno }).ToList();
            ViewBag.trabajador = new Trabajador { Nombre = "", Rut = "", Id = 0 };
            ViewBag.personas = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Rut = x.Rut, Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno }).ToList();

            return View();
        }
        private void LiquidacionPDF(DateTime dateTime, string[] ids, Empresas empresa)
        {
            string strHeader = "Liquidacion de remuneracion\n" + dateTime.ToString("MMMM yyyy");//Convert.ToDateTime(collection["Periodo"]).ToLongDateString();
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
                HorasExtras horasExtras = new HorasExtras();
                try
                { 
                   horasExtras = db.HorasExtras.First(x => x.IdTrabajador == id && x.Periodo.Value.Month == dateTime.Month);
                }
                catch { }

                var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontBoldPequeña = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontPequeña = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                PdfPTable tablaEmpresa = new PdfPTable(2);
                tablaEmpresa.WidthPercentage = 100f;
                tablaEmpresa.AddCell(new PdfPCell(new Phrase(empresa.Nombre, fontBold)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.NO_BORDER });
                tablaEmpresa.AddCell(new PdfPCell(new Phrase(DateTime.Now.ToString("dd 'de' MMMM, yyyy"), fontBold)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });
                tablaEmpresa.AddCell(new PdfPCell(new Phrase(empresa.Rut, fontPequeña)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.NO_BORDER });
                tablaEmpresa.AddCell(new PdfPCell(new Phrase("UF: $27.592", fontPequeña)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });
                tablaEmpresa.AddCell(new PdfPCell(new Phrase(" ", fontPequeña)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.NO_BORDER });
                tablaEmpresa.AddCell(new PdfPCell(new Phrase("UTM: $49.033", fontPequeña)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });
                document.Add(tablaEmpresa);
                Paragraph p = new Paragraph(new Chunk("\n"));
                //Report Header
                BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntHead = new Font(bfntHead, 16, 1, BaseColor.DARK_GRAY);
                fntHead.Size = 14;
                Paragraph prgHeading = new Paragraph();
                prgHeading.Alignment = Element.ALIGN_CENTER;
                prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
                document.Add(prgHeading);
                document.Add(p);

                int sueldo = Convert.ToInt32(trabajador.SueldoBruto);
                int horitas = GetHoras(trabajador, dateTime);
                if(horitas==0)
                {
                   horitas = Convert.ToInt32(horasExtras.Horas);
                }
                int horas = 0;
                int bono = 0;
                if (horitas > 40 )
                {
                    horas = Convert.ToInt32( 40 * ((sueldo / 30) / 8) * 1.5);
                    bono = Convert.ToInt32((horitas-40) * ((sueldo / 30) / 8) * 1.5);
                    horitas = 40;
                }
                else if (horitas>0)
                {
                    horas = Convert.ToInt32(horitas * ((sueldo / 30) / 8) * 1.5);
                }
                BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntColumnHeader = new Font(btnColumnHeader, 14, 1, BaseColor.WHITE);
                PdfPTable datosTrabajador = new PdfPTable(2);
                datosTrabajador.WidthPercentage = 100f;
                datosTrabajador.SetWidths(new int[] { 1, 3 });
                string titles = "Nombre\nRUT\nCargo\nCentro de Costos\nContratado\nRenta Base (30)";
                string data = trabajador.Nombre +" "+ trabajador.ApellidoPaterno +" "+ trabajador.ApellidoMaterno + "\n" +
                                formatearRut(trabajador.Rut) + "\n" +
                                trabajador.Cargo + "\n" +
                                trabajador.Campo + "\n" +
                                Convert.ToDateTime(trabajador.FechaIngreso).ToShortDateString() + ", " + trabajador.Contrato + "\n" +
                                sueldo.ToString("C0");
                PdfPCell celdaTitles = new PdfPCell();
                PdfPCell celdaData = new PdfPCell();
                //dato.Border = Rectangle.NO_BORDER;
                celdaTitles.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                celdaTitles.AddElement(new Phrase(titles, fontPequeña));
                celdaData.AddElement(new Phrase(data, fontBoldPequeña));
                datosTrabajador.AddCell(celdaTitles);
                datosTrabajador.AddCell(celdaData);
                document.Add(datosTrabajador);
                //document.Add(new Paragraph("Rut: " + formatearRut(trabajador.Rut), fntHead));
                //p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p);
                //Add line break

                //Write the table

                PdfPTable tablaHaberes = new PdfPTable(1);
                PdfPTable tablaDescuentos = new PdfPTable(1);
                tablaHaberes.AddCell(new PdfPCell(new Phrase("Haberes", fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
                tablaDescuentos.AddCell(new PdfPCell(new Phrase("Descuentos", fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });



                PdfPTable haberes = new PdfPTable(3);
                haberes.SetWidths(new int[] { 2, 4, 3 });
                //haberes.WidthPercentage = 100f;
                PdfPTable descuentos = new PdfPTable(3);
                descuentos.SetWidths(new int[] { 2, 4, 3 });
                //descuentos.WidthPercentage = 50f;
                //Table header
                Font fntSalto = new Font(btnColumnHeader, 6, 1, BaseColor.WHITE);
                Font fntCell = new Font(btnColumnHeader, 10);//table Data;
                List<HyD> hyDs = db.HyD.Where(x => x.IdTrabajador == trabajador.Id).ToList();
                List<AFP> AFPs = db.AFP.ToList();
                float factorAFP = 0;
                string afpNombre = "";
                foreach (var AFP in AFPs)
                {
                    if (trabajador.AFP!=null && trabajador.AFP.Contains(AFP.Nombre))
                    {
                        factorAFP = float.Parse(AFP.Porcentaje) / 100;
                        afpNombre = AFP.Nombre;
                    }
                }
                int totalHaberes = 0;
                int totalDescuentos = 0;
                int countH = 1;
                int countD = 2;
                haberes.AddCell(new Phrase("30", fntCell));
                haberes.AddCell(new Phrase("Sueldo mensual", fntCell));
                haberes.AddCell(new Phrase(Convert.ToInt32(sueldo).ToString("C0"), fntCell));
                totalHaberes = totalHaberes + Convert.ToInt32(sueldo);
                descuentos.AddCell(new Phrase(Convert.ToString(factorAFP) + "%", fntCell));
                descuentos.AddCell(new Phrase("AFP " + afpNombre, fntCell));
                descuentos.AddCell(new Phrase((Convert.ToInt32(sueldo) * (factorAFP / 100)).ToString("C0"), fntCell));
                descuentos.AddCell(new Phrase("7%", fntCell));
                descuentos.AddCell(new Phrase("Fonasa", fntCell));
                descuentos.AddCell(new Phrase((Convert.ToInt32(sueldo) * 0.07).ToString("C0"), fntCell));
                totalDescuentos = (int)((Convert.ToInt32(sueldo) * 0.1) + (Convert.ToInt32(sueldo) * 0.07));
                if (horas > 0)
                {
                    haberes.AddCell(new Phrase(horitas.ToString(), fntCell));
                    haberes.AddCell(new Phrase("Horas extras", fntCell));
                    haberes.AddCell(new Phrase(horas.ToString("C0"), fntCell));
                    totalHaberes = totalHaberes + horas;
                    countH += 1;
                }
                if (bono>0)
                {
                    haberes.AddCell(new Phrase(" ", fntCell));
                    haberes.AddCell(new Phrase("Bono Camiseta", fntCell));
                    haberes.AddCell(new Phrase(bono.ToString("C0"), fntCell));
                    totalHaberes = totalHaberes + horas;
                    countH += 1;
                }
                foreach (var hyd in hyDs)
                {
                    if (hyd.Tipo == "Haber")
                    {
                        haberes.AddCell(new Phrase(" ", fntCell));
                        haberes.AddCell(new Phrase(hyd.Nombre, fntCell));
                        haberes.AddCell(new Phrase(Convert.ToInt32(hyd.Monto).ToString("C0"), fntCell));
                        totalHaberes = totalHaberes + Convert.ToInt32(hyd.Monto);
                        countH += 1;
                    }
                    else
                    {
                        descuentos.AddCell(new Phrase(" ", fntCell));
                        descuentos.AddCell(new Phrase(hyd.Nombre, fntCell));
                        descuentos.AddCell(new Phrase(Convert.ToInt32(hyd.Monto).ToString("C0"), fntCell));
                        totalDescuentos = totalDescuentos + Convert.ToInt32(hyd.Monto);
                        countD += 1;
                    }
                }
                while (countH < 20)
                {
                    haberes.AddCell(new Phrase(" ", fntCell));
                    haberes.AddCell(new Phrase(" ", fntCell));
                    haberes.AddCell(new Phrase(" ", fntCell));
                    countH += 1;
                }
                while (countD < 20)
                {
                    descuentos.AddCell(new Phrase(" ", fntCell));
                    descuentos.AddCell(new Phrase(" ", fntCell));
                    descuentos.AddCell(new Phrase(" ", fntCell));
                    countD += 1;
                }
                tablaHaberes.AddCell(new PdfPCell(haberes) { Border = Rectangle.NO_BORDER });
                tablaDescuentos.AddCell(new PdfPCell(descuentos) { Border = Rectangle.NO_BORDER });
                PdfPTable tablaTotalHaberes = new PdfPTable(2);
                tablaTotalHaberes.SetWidths(new int[] { 2, 1 });
                tablaTotalHaberes.AddCell(new PdfPCell(new Phrase("Total haberes", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                tablaTotalHaberes.AddCell(new PdfPCell(new Phrase(totalHaberes.ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                tablaHaberes.AddCell(new PdfPCell(tablaTotalHaberes) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                PdfPTable tablaTotalDescuentos = new PdfPTable(2);
                tablaTotalDescuentos.SetWidths(new int[] { 2, 1 });
                tablaTotalDescuentos.AddCell(new PdfPCell(new Phrase("Total descuentos", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                tablaTotalDescuentos.AddCell(new PdfPCell(new Phrase(totalDescuentos.ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                tablaDescuentos.AddCell(new PdfPCell(tablaTotalDescuentos) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                PdfPTable hydTabla = new PdfPTable(3);


                hydTabla.SetWidths(new int[] { 10, 1, 11 });
                hydTabla.WidthPercentage = 100f;
                PdfPCell haber = new PdfPCell(tablaHaberes);
                haber.Border = Rectangle.NO_BORDER;
                PdfPCell descuento = new PdfPCell(tablaDescuentos);
                descuento.Border = Rectangle.NO_BORDER;
                hydTabla.AddCell(haber);
                PdfPTable tablaRelleno = new PdfPTable(1);
                PdfPCell relleno = new PdfPCell(new Phrase(" "));
                relleno.Border = Rectangle.NO_BORDER;
                hydTabla.AddCell(relleno);
                hydTabla.AddCell(descuento);
                tablaRelleno.AddCell(relleno);
                hydTabla.AddCell(new PdfPCell(tablaRelleno) { Border = Rectangle.NO_BORDER });
                hydTabla.AddCell(new PdfPCell(tablaRelleno) { Border = Rectangle.NO_BORDER });
                hydTabla.AddCell(new PdfPCell(tablaRelleno) { Border = Rectangle.NO_BORDER });
                PdfPTable tablaSII = new PdfPTable(2);
                tablaSII.SetWidths(new int[] { 2, 1 });
                tablaSII.AddCell(new PdfPCell(new Phrase("Imponible", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                tablaSII.AddCell(new PdfPCell(new Phrase((sueldo + horas).ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                tablaSII.AddCell(new PdfPCell(new Phrase("Tributable", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                tablaSII.AddCell(new PdfPCell(new Phrase(Convert.ToInt32(((sueldo + horas) - totalDescuentos)).ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                //tablaDescuentos.AddCell(new PdfPCell(tablaTotalDescuentos) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                PdfPTable tablaRRHH = new PdfPTable(3);
                tablaRRHH.AddCell(new PdfPCell(new Phrase("Mutual", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                tablaRRHH.AddCell(new PdfPCell(new Phrase("Censatia", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                tablaRRHH.AddCell(new PdfPCell(new Phrase("S.I.S.", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                tablaRRHH.AddCell(new PdfPCell(new Phrase((sueldo * 0.0095).ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                if (trabajador.FechaTermino != null)
                {
                    tablaRRHH.AddCell(new PdfPCell(new Phrase((sueldo * 0.024).ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                }
                else
                {
                    tablaRRHH.AddCell(new PdfPCell(new Phrase((sueldo * 0.03).ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                }
                tablaRRHH.AddCell(new PdfPCell(new Phrase((sueldo * 0.0153).ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                PdfPTable tablaContenedora = new PdfPTable(1);
                tablaContenedora.AddCell(new PdfPCell(tablaSII) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                tablaContenedora.AddCell(new PdfPCell(tablaRRHH) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                hydTabla.AddCell(new PdfPCell(tablaContenedora) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                hydTabla.AddCell(relleno);

                PdfPTable tablaLiquidaciones = new PdfPTable(2);
                tablaLiquidaciones.SetWidths(new int[] { 2, 1 });
                tablaLiquidaciones.AddCell(new PdfPCell(new Phrase("Total haberes", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                tablaLiquidaciones.AddCell(new PdfPCell(new Phrase(totalHaberes.ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                tablaLiquidaciones.AddCell(new PdfPCell(new Phrase("Descuentos legales", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                tablaLiquidaciones.AddCell(new PdfPCell(new Phrase(totalDescuentos.ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                tablaLiquidaciones.AddCell(new PdfPCell(new Phrase("Descuentos varios", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                tablaLiquidaciones.AddCell(new PdfPCell(new Phrase("$0", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                tablaLiquidaciones.AddCell(new PdfPCell(new Phrase("Liquido a pagar", fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                tablaLiquidaciones.AddCell(new PdfPCell(new Phrase((totalHaberes - totalDescuentos).ToString("C0"), fntCell)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT });
                hydTabla.AddCell(new PdfPCell(tablaLiquidaciones) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, Border = Rectangle.NO_BORDER });



                document.Add(hydTabla);
                document.Add(p);
                PdfPTable declaracion = new PdfPTable(1);
                declaracion.WidthPercentage = 100f;
                string texto = "Declaro que he recibido de " + empresa.Nombre + " " + empresa.Rut +
                    ", a mi entera satisfacción, el monto líquido a pago indicado en " +
                    "la presente liquidación y no tengo cargos ni cobros posteriores " +
                    "que hacer, asimismo acepto y reconozco la forma en como se determinó y las deducciones efectuadas.\n\n\n\n" +
                    "                                                               ______________________\n\n" +
                    "                                                                      Recibí conforme";
                declaracion.AddCell(new Phrase(texto, fntCell));
                document.Add(declaracion);
                document.NewPage();
            }
            document.Close();
            writer.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + "Liquidacion " + dateTime.ToString(" MMMM yyyy ") + empresa.Nombre + ".pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(document);
            Response.End();
        }
        private string GetDias(Trabajador trabajador, DateTime dateTime)
        {
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
        private int GetHoras(Trabajador trabajador, DateTime dateTime)
        {
            int horas = 0;
            int mes = dateTime.Month;
            var registros = db.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid && x.Fecha.Month == mes).Select(x => new { Fecha = x.Fecha }).ToList();
            registros = registros.OrderBy(o => o.Fecha).ToList();
            DateTime fecha = DateTime.Now;
            DateTime entrada = DateTime.Now;
            DateTime salida = DateTime.Now;
            int i = 0;
            while (i < registros.Count)
            {
                entrada = registros[i].Fecha;
                while (i < registros.Count && entrada.Day == registros[i].Fecha.Day)
                {
                    salida = registros[i].Fecha;
                    i++;
                }
                if(entrada.Day == salida.Day)
                {
                    TimeSpan resultado = salida.Subtract(entrada);
                    if (resultado.TotalHours > 8)
                    {
                        horas += Convert.ToInt32(resultado.TotalHours) - 8;
                    }
                }
            }
            return horas;
        }

        private string obtenerFecha(DateTime? datetime, string formato)
        {
            if (datetime != null)
            {
                DateTime date = Convert.ToDateTime(datetime);
                return date.ToString(formato);
            }
            return "";
        }

        private void ExcelPrevired(DateTime dateTime, string[] ids, Empresas empresa)
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
                int i = 1;
                foreach (var idd in ids)
                {
                    try
                    {
                        int id = Convert.ToInt32(idd);
                        Trabajador trabajador = db.Trabajador.First(x => x.Id == id);
                        string rut = trabajador.Rut.Replace(".", "").Replace("-", "");
                        string rutEmpresa = "";
                        try
                        {
                            rutEmpresa = empresa.Rut.Replace(".", "").Replace("-", "");
                        }
                        catch { }
                        string rutSubsidio = "";
                        try
                        {
                            rutSubsidio = obtenerValor(trabajador.Subsidio).Replace(".", "").Replace("-", "");
                        }
                        catch { }
                        string rutAV = "";
                        try
                        {

                            rutAV = trabajador.RutAV.Replace(".", "").Replace("-", "");
                        }
                        catch
                        {

                        }
                        sl.SetCellValue(i, 1, rut.Substring(0, rut.Length - 2));
                        sl.SetCellValue(i, 2, rut.Substring(rut.Length - 1, 1));
                        sl.SetCellValue(i, 3, trabajador.ApellidoPaterno);
                        sl.SetCellValue(i, 4, trabajador.ApellidoMaterno);
                        sl.SetCellValue(i, 5, trabajador.Nombre);
                        sl.SetCellValue(i, 6, trabajador.Sexo.Substring(0, 1));
                        sl.SetCellValue(i, 7, obtenerValor(trabajador.Nacionalidad));
                        sl.SetCellValue(i, 8, obtenerValor(trabajador.TipoPago));
                        string fechita = obtenerFecha(trabajador.FechaIngreso, "MMyyyy");
                        sl.SetCellValue(i, 9, obtenerFecha(trabajador.FechaIngreso, "MMyyyy"));
                        try
                        {
                            sl.SetCellValue(i, 10, obtenerFecha(trabajador.FechaTermino, "MMyyyy"));
                        }
                        catch { }
                        sl.SetCellValue(i, 11, obtenerValor(trabajador.RegimenPrevicional));
                        sl.SetCellValue(i, 12, obtenerValor(trabajador.Jubiulado));
                        sl.SetCellValue(i, 13, GetDias(trabajador, dateTime));
                        sl.SetCellValue(i, 14, obtenerValor(trabajador.TipoLineaAFP));
                        sl.SetCellValue(i, 15, obtenerValor(trabajador.CodigoMovimientoDePersonal));
                        sl.SetCellValue(i, 16, obtenerFecha(trabajador.InicioMovimientoPersonal, "dd-MM-yyyy"));
                        sl.SetCellValue(i, 17, obtenerFecha(trabajador.FinMovimientoPersonal, "dd-MM-yyyy"));
                        sl.SetCellValue(i, 18, obtenerValor(trabajador.TrampoAsignacionFamiliar));
                        sl.SetCellValue(i, 19, trabajador.CargasSimples.ToString());
                        sl.SetCellValue(i, 20, trabajador.CargasMaternales.ToString());
                        sl.SetCellValue(i, 21, trabajador.CargasInvalidas.ToString());
                        sl.SetCellValue(i, 22, trabajador.AsignacionFamiliar);
                        sl.SetCellValue(i, 23, trabajador.AsignacionFamiliarRetroactiva);
                        sl.SetCellValue(i, 24, trabajador.ReintegroCargasFamiliares);
                        sl.SetCellValue(i, 25, obtenerValor(trabajador.SolicitudTrabajadorJoven));
                        sl.SetCellValue(i, 26, obtenerValor(trabajador.AFP));
                        sl.SetCellValue(i, 27, trabajador.RentaImponibleAFP);
                        sl.SetCellValue(i, 28, trabajador.CotizacionObligatoriaAFP);
                        sl.SetCellValue(i, 29, trabajador.SIS);
                        sl.SetCellValue(i, 30, trabajador.AhorroVoluntarioAFP);
                        sl.SetCellValue(i, 31, trabajador.RentaImpSustitutiva);
                        sl.SetCellValue(i, 32, trabajador.TasaPactadaSus);
                        sl.SetCellValue(i, 33, trabajador.AporteIndemSus);
                        sl.SetCellValue(i, 34, trabajador.NPeriodosSus);
                        try { sl.SetCellValue(i, 35, obtenerFecha(trabajador.PeriodosDesdeSus, "dd-mm-yyyy")); }
                        catch { }
                        try { sl.SetCellValue(i, 36, obtenerFecha(trabajador.PerodoHastaSus, "dd-mm-yyyy")); }
                        catch { }

                        sl.SetCellValue(i, 37, trabajador.PuestoTrabajoPesado);
                        sl.SetCellValue(i, 38, trabajador.PorcentajeTrabajoPesado);
                        sl.SetCellValue(i, 39, trabajador.MontoTrabajoPesado);
                        sl.SetCellValue(i, 40, obtenerValor(trabajador.CodigoAPVI));
                        sl.SetCellValue(i, 41, trabajador.NumeroAPVI);
                        sl.SetCellValue(i, 42, obtenerValor(trabajador.FormaPagoAPVI));
                        sl.SetCellValue(i, 43, trabajador.CotizacionAPVI);
                        sl.SetCellValue(i, 44, trabajador.NumeroAPVI);
                        sl.SetCellValue(i, 45, obtenerValor(trabajador.CodigoAPVC));
                        sl.SetCellValue(i, 46, trabajador.NumeroAPVC);
                        sl.SetCellValue(i, 47, obtenerValor(trabajador.FormaPagoAPVC));
                        sl.SetCellValue(i, 48, trabajador.CotizacionTrabajadorAPVC);
                        sl.SetCellValue(i, 49, trabajador.CotizacionEmpleadorAPVC);
                        try
                        {
                            sl.SetCellValue(i, 50, rutAV.Substring(0, rutAV.Length - 2));
                            sl.SetCellValue(i, 51, rutAV.Substring(rutAV.Length - 1, 1));
                        }
                        catch { }
                        sl.SetCellValue(i, 52, trabajador.ApellidoPaternoAV);
                        sl.SetCellValue(i, 53, trabajador.ApellidoMaternoAV);
                        sl.SetCellValue(i, 54, trabajador.NombresAV);
                        sl.SetCellValue(i, 55, obtenerValor(trabajador.CodigoMovimientoDePersonal));
                        try { sl.SetCellValue(i, 56, obtenerFecha(trabajador.DesdeAV, "dd-MM-yyyy")); }
                        catch { }
                        try { sl.SetCellValue(i, 57, obtenerFecha(trabajador.HastaAV, "dd-MM-yyyy")); }
                        catch { }
                        sl.SetCellValue(i, 58, obtenerValor(trabajador.AFPAV));
                        sl.SetCellValue(i, 59, trabajador.MontoAV);
                        sl.SetCellValue(i, 60, trabajador.MontoCapitalizacionVoluntaria);
                        sl.SetCellValue(i, 61, trabajador.PeriodosAV);
                        sl.SetCellValue(i, 62, obtenerValor(trabajador.CodigoExcaja));
                        sl.SetCellValue(i, 63, trabajador.TasaExcaja);
                        sl.SetCellValue(i, 64, trabajador.RentaImponibleIPS);
                        sl.SetCellValue(i, 65, trabajador.CotizacionObligatoriaIPS);
                        sl.SetCellValue(i, 66, trabajador.RentaImponibleDesahucio);
                        sl.SetCellValue(i, 67, obtenerValor(trabajador.CodigoExcajaDesahucio));
                        sl.SetCellValue(i, 68, trabajador.TasaCotizacionDesahucio);
                        sl.SetCellValue(i, 69, trabajador.CotizaciónDesahucio);
                        sl.SetCellValue(i, 70, trabajador.CotizacionFonasa);
                        sl.SetCellValue(i, 71, trabajador.CotizacionISL);
                        sl.SetCellValue(i, 72, trabajador.BonificacionLey);
                        sl.SetCellValue(i, 73, trabajador.DescuentoPorCargasFamiliaresISL);
                        sl.SetCellValue(i, 74, trabajador.BonosGobierno);
                        sl.SetCellValue(i, 75, obtenerValor(trabajador.CodigoIntitucionSalud));
                        sl.SetCellValue(i, 76, trabajador.NumeroFun);
                        sl.SetCellValue(i, 77, trabajador.RentaImponibleIsapre);
                        sl.SetCellValue(i, 78, obtenerValor(trabajador.MonedaDelPlanPactadoIsapre));
                        sl.SetCellValue(i, 79, trabajador.CotizacionPactada);
                        sl.SetCellValue(i, 80, trabajador.CotizacionIsapre);
                        sl.SetCellValue(i, 81, trabajador.CotizacionIsapreAV);
                        sl.SetCellValue(i, 82, trabajador.MontoGES);
                        sl.SetCellValue(i, 83, obtenerValor(trabajador.CodigoCCAF));
                        sl.SetCellValue(i, 84, trabajador.RentaImponibleCCAF);
                        sl.SetCellValue(i, 85, trabajador.CreditosPersonalesCCAF);
                        sl.SetCellValue(i, 86, trabajador.DescuentoDentalCCAF);
                        sl.SetCellValue(i, 87, trabajador.DescuentosPorLeasing);
                        sl.SetCellValue(i, 88, trabajador.DescuentoPorSeguroDeVidaCCAF);
                        sl.SetCellValue(i, 89, trabajador.OtrosDescuentosCCAF);
                        sl.SetCellValue(i, 90, trabajador.CotizacionCCAFNoAfiniladosIsapres);
                        sl.SetCellValue(i, 91, trabajador.DescuentoCargasFamiliaresCCAF);
                        sl.SetCellValue(i, 92, trabajador.OtrosDescuentosCCAF1);
                        sl.SetCellValue(i, 93, trabajador.OtrosDescuentosCCAF2);
                        sl.SetCellValue(i, 94, trabajador.BonosGobiernoCCAF);
                        sl.SetCellValue(i, 95, trabajador.CodigoSucursalCCAF);
                        sl.SetCellValue(i, 96,  obtenerValor(trabajador.CodigoMutualidad));
                        sl.SetCellValue(i, 97, trabajador.RentaImponibleCCAF);
                        sl.SetCellValue(i, 98,  trabajador.CotizacionAccidenteMutual);
                        sl.SetCellValue(i, 99,  trabajador.SucursalParaPagoMutual);
                        sl.SetCellValue(i, 100, trabajador.RentaImponibleSeguroCensantia);
                        sl.SetCellValue(i, 101, trabajador.AporteTrabajadorSeguroCensatia);
                        sl.SetCellValue(i, 102, trabajador.AporteEmpleadorSeguroCesantia);
                        try
                        { 
                        sl.SetCellValue(i, 103, rutSubsidio.Substring(0, rutSubsidio.Length-2));
                        sl.SetCellValue(i, 104, rutSubsidio.Substring(rutSubsidio.Length - 1, 1));
                        }
                        catch { }
                        sl.SetCellValue(i, 105, trabajador.DatosEmpresa);
                        i++;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ocurrio una Excepción: " + ex.Message);
                    }
                }
                //Guardar como, y aqui ponemos la ruta de nuestro archivo
                sl.SaveAs("C:\\Data\\ExcelPrevired.xlsx");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocurrio una Excepción: " + ex.Message);
            }

            //doc.SaveAs("C:\\Data\\WorksheetOperations.xlsx");
            FileStream sourceFile = new FileStream("C:\\Data\\ExcelPrevired.xlsx", FileMode.Open);
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
            Response.AddHeader("content-disposition", "attachment;filename=" + "Excel Previred " + empresa.Nombre +" "+ dateTime.ToString("mm-yyyy") + ".xlsx");
            Response.BinaryWrite(getContent);
            Response.Flush();
            Response.End();
        }
        [HttpPost]
        public ActionResult Liquidacion(FormCollection collection)
        {
            
            string nombre = collection["id"];
            Empresas empresa = db.Empresas.First(x => x.Nombre == nombre);
            string periodo = collection["Periodo"];
            DateTime dateTime = DateTime.Now;
            dateTime =  dateTime.AddMonths(-1);
            if(!String.IsNullOrEmpty(periodo))
            {
                dateTime = Convert.ToDateTime(periodo);
            }
                    
            string[] ids = { "" };
            if (!String.IsNullOrEmpty(collection["centros"]))
            {
                ids = collection["centros"].Split(new char[] { ',' });
            }
            if(collection["Tipo"] == "Liquidacion")
            {
                LiquidacionPDF(dateTime, ids, empresa);
            }
            else
            {
                ExcelPrevired(dateTime, ids, empresa);
            }
            return RedirectToAction("Index");
        }

        public string obtenerValor(string palabra)
        {
            try
            {
                return palabra.Substring(0, palabra.IndexOf("."));
            }
            catch
            { }
            return ""; 
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
        // GET: Remuneraciones
        public ActionResult Index()
        {
            return View();
        }

        // GET: Remuneraciones/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Remuneraciones/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Remuneraciones/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Remuneraciones/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Remuneraciones/Edit/5
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

        // GET: Remuneraciones/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Remuneraciones/Delete/5
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
