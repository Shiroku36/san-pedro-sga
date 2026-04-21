using ControlPersonalAppWeb;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Font = iTextSharp.text.Font;

namespace ControlPersonalAppWeb.Controllers
{
    public class CasinoController : Controller
    {
        private SgajcpEntities db = new SgajcpEntities();
        Dictionary<string, List<string>> nombresServicios = new Dictionary<string, List<string>>
{
    { "Molina", new List<string> {
        "Cena", "Desayuno SIMPLE", "Desayuno Doble", "Almuerzo", "Once", "Comida", "Colación a Terreno"
    }},
    { "Isla de Maipo", new List<string> {
        "Cena", "Desayuno", "Almuerzo", "Colación", "Comida"
    }}
};


        // GET: Casino
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FormCollection collection)
        {
            DateTime fechaActual = Convert.ToDateTime(collection["Fecha"]);
            ViewBag.Fecha = fechaActual.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
            var registrosHoy = db.Casino
                .Where(x => x.Fecha.HasValue &&
                            x.Fecha.Value.Day == fechaActual.Day &&
                            x.Fecha.Value.Month == fechaActual.Month &&
                            x.Fecha.Value.Year == fechaActual.Year)
                .OrderByDescending(x => x.Fecha)
                .ToList();

            var personas = db.Personas
                        .DistinctBy(p => p.Rut)
                        .ToDictionary(p => p.Rut, p => p.Nombre);

            var registrosConNombre = registrosHoy
                .Select(c => new CasinoConNombreViewModel
                {
                    Id = c.Id,
                    Rut = c.Rut,
                    Fecha = c.Fecha,
                    Comida = c.Comida,
                    Foto = c.Foto,
                    Ubicacion = c.Ubicacion,
                    Nombre = personas.ContainsKey(c.Rut.ToLower()) ? personas[c.Rut.ToLower()] : "(Desconocido)"
                })
                .ToList();



            return View(registrosConNombre);
        }
        public ActionResult Index()
        {
            DateTime fechaActual = DateTime.Now;
            ViewBag.Fecha = fechaActual.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));

            var registrosHoy = db.Casino
                .Where(x => x.Fecha.HasValue &&
                            x.Fecha.Value.Day == fechaActual.Day &&
                            x.Fecha.Value.Month == fechaActual.Month &&
                            x.Fecha.Value.Year == fechaActual.Year)
                .OrderByDescending(x => x.Fecha)
                .ToList();
            var personas = db.Personas
                        .DistinctBy(p => p.Rut)
                        .ToDictionary(p => p.Rut, p => p.Nombre);


            var registrosConNombre = registrosHoy
                .Select(c => new CasinoConNombreViewModel
                {
                    Id = c.Id,
                    Rut = c.Rut,
                    Fecha = c.Fecha,
                    Comida = c.Comida,
                    Foto = c.Foto,
                    Ubicacion = c.Ubicacion,
                    Nombre = personas.ContainsKey(c.Rut.ToLower()) ? personas[c.Rut.ToLower()] : "(Desconocido)"
                })
                .ToList();
            return View(registrosConNombre);
        }
        public ActionResult Informe(FormCollection collection)
        {
            DateTime fechaInicio = Convert.ToDateTime(collection["Inicio"]);
            DateTime fechaFin = Convert.ToDateTime(collection["Fin"]);
            fechaFin = fechaFin.AddDays(1);
            // Servicios por ubicación
            Dictionary<string, List<string>> nombresServicios = new Dictionary<string, List<string>>
    {
        { "Molina", new List<string> {
            "Cena", "Desayuno SIMPLE", "Desayuno Doble", "Almuerzo", "Once", "Comida", "Colación a Terreno"
        }},
        { "Isla de Maipo", new List<string> {
            "Cena", "Desayuno", "Almuerzo", "Colación", "Comida"
        }}
    };

            // JOIN Casino + Personas por RUT
            var datosFiltrados = (from c in db.Casino
                                  join p in db.Personas on c.Rut equals p.Rut
                                  where DbFunctions.TruncateTime(c.Fecha) >= fechaInicio.Date &&
                                        DbFunctions.TruncateTime(c.Fecha) <= fechaFin.Date
                                  select new
                                  {
                                      c.Fecha,
                                      c.Comida,
                                      c.Ubicacion,
                                      c.Rut,
                                      p.Nombre,
                                      p.GlosaPuesto,
                                      p.CentroCosto1,
                                      p.CentroCosto2,
                                      p.GlosaCosto,
                                      p.Instalacion
                                  }).ToList();

            // ✅ EXPORTACIÓN A PDF
            if (collection["formato"] == "pdf")
            {
                Document document = new Document(PageSize.A4.Rotate());
                MemoryStream memoryStream = new MemoryStream();
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                Font tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLUE);
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                Font dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                Font totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, Font.ITALIC);

                document.Add(new Paragraph("Informe de Servicios", tituloFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 15f });
                document.Add(new Paragraph($"Desde: {fechaInicio:dd/MM/yyyy} - Hasta: {fechaFin.AddDays(-1):dd/MM/yyyy}", dataFont) { SpacingAfter = 10f });

                var ubicaciones = datosFiltrados.Select(x => x.Ubicacion).Distinct().ToList();

                foreach (var ubicacion in ubicaciones)
                {
                    var servicios = nombresServicios.ContainsKey(ubicacion) ? nombresServicios[ubicacion] : new List<string>();
                    var datosUbicacion = datosFiltrados.Where(x => x.Ubicacion == ubicacion).ToList();

                    // Agrupar por fecha y contar por comida
                    var agrupado = datosUbicacion
                        .GroupBy(x => x.Fecha.Value.Date)
                        .OrderBy(g => g.Key)
                        .ToList();

                    // Encabezado ubicación
                    document.Add(new Paragraph("Ubicación: " + ubicacion, headerFont) { SpacingBefore = 10f , SpacingAfter = 10f});

                    PdfPTable table = new PdfPTable(servicios.Count + 1); // +1 para columna de fecha
                    table.WidthPercentage = 100;

                    // Cabeceras
                    AddCell(table, "Fecha", headerFont, BaseColor.LIGHT_GRAY);
                    foreach (var servicio in servicios)
                        AddCell(table, servicio, headerFont, BaseColor.LIGHT_GRAY);

                    // Totales
                    Dictionary<string, int> totales = servicios.ToDictionary(s => s, s => 0);

                    // Filas por fecha
                    foreach (var grupo in agrupado)
                    {
                        AddCell(table, grupo.Key.ToString("dd-MM-yyyy"), dataFont);

                        var conteo = grupo
                            .GroupBy(x => x.Comida)
                            .ToDictionary(g => g.Key, g => g.Count());

                        foreach (var servicio in servicios)
                        {
                            int cantidad = conteo.ContainsKey(servicio) ? conteo[servicio] : 0;
                            totales[servicio] += cantidad;
                            AddCell(table, cantidad.ToString(), dataFont);
                        }
                    }

                    // Fila Total
                    AddCell(table, "Total", totalFont, BaseColor.LIGHT_GRAY);
                    foreach (var servicio in servicios)
                        AddCell(table, totales[servicio].ToString(), totalFont, BaseColor.LIGHT_GRAY);

                    document.Add(table);
                }

                document.Close();
                return File(memoryStream.ToArray(), "application/pdf", $"Informe_Servicios_{fechaInicio:dd-MM-yyyy}_a_{fechaFin.AddDays(-1):dd-MM-yyyy}.pdf");
            }


            // ✅ EXPORTACIÓN A EXCEL
            else
            {
                SLDocument sl = new SLDocument();
                sl.RenameWorksheet(SLDocument.DefaultFirstSheetName, "Informe Servicios");

                SLStyle headerStyle = sl.CreateStyle();
                headerStyle.Font.Bold = true;
                headerStyle.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.LightGray, System.Drawing.Color.LightGray);
                headerStyle.Alignment.Horizontal = HorizontalAlignmentValues.Center;

                SLStyle titleStyle = sl.CreateStyle();
                titleStyle.Font.Bold = true;
                titleStyle.Font.FontSize = 18;
                titleStyle.Font.FontColor = System.Drawing.Color.Blue;
                titleStyle.Alignment.Horizontal = HorizontalAlignmentValues.Center;

                int fila = 1;
                sl.SetCellValue(fila, 1, "Informe de Servicios");
                sl.MergeWorksheetCells(fila, 1, fila, 11);
                sl.SetCellStyle(fila++, 1, titleStyle);

                sl.SetCellValue(fila++, 1, $"Desde: {fechaInicio:dd/MM/yyyy} - Hasta: {fechaFin:dd/MM/yyyy}");

                string[] headers = { "Centro Costo 1", "Centro Costo 2", "Glosa Centro de Costo", "Ubicación",
                             "Instalación", "Rut", "Nombre completo", "Glosa Puesto", "Fecha", "Hora", "Servicio", "Ubicación pedido" };

                for (int i = 0; i < headers.Length; i++)
                {
                    sl.SetCellValue(fila, i + 1, headers[i]);
                }

                sl.SetCellStyle(fila, 1, fila, headers.Length, headerStyle);
                fila++;

                foreach (var d in datosFiltrados)
                {
                    sl.SetCellValue(fila, 1, d.CentroCosto1);
                    sl.SetCellValue(fila, 2, d.CentroCosto2);
                    sl.SetCellValue(fila, 3, d.GlosaCosto);
                    sl.SetCellValue(fila, 4, d.Ubicacion);
                    sl.SetCellValue(fila, 5, d.Instalacion);
                    sl.SetCellValue(fila, 6, d.Rut);
                    sl.SetCellValue(fila, 7, d.Nombre);
                    sl.SetCellValue(fila, 8, d.GlosaPuesto); 
                    sl.SetCellValue(fila, 9, d.Fecha?.ToString("dd/MM/yyyy"));
                    sl.SetCellValue(fila, 10, d.Fecha?.ToString("HH:mm"));
                    if (d.Ubicacion == "Molina")
                    {
                        var lista = nombresServicios["Molina"];
                        int index = lista.IndexOf(d.Comida);
                        if (index != -1)
                        {
                            sl.SetCellValue(fila, 11, $"{index + 1}.- {d.Comida}");
                        }
                        else
                        {
                            sl.SetCellValue(fila, 11, $"{0}.- {d.Comida}");
                        }
                    }
                    else
                    {
                        var lista = nombresServicios["Isla de Maipo"];
                        int index = lista.IndexOf(d.Comida);
                        if (index != -1)
                        {
                            sl.SetCellValue(fila, 11, $"{index + 1} {d.Comida}");
                        }
                        else
                        {
                            sl.SetCellValue(fila, 11, $"{0} {d.Comida}");
                        }
                    }
                    sl.SetCellValue(fila, 12, d.Ubicacion);
                    fila++;
                }

                MemoryStream ms = new MemoryStream();
                sl.SaveAs(ms);
                ms.Position = 0;

                return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Informe_Servicios_{fechaInicio:dd-MM-yyyy}_a_{fechaFin:dd-MM-yyyy}.xlsx");
            }
        }

        // Método auxiliar para agregar celdas
        private void AddCell(PdfPTable table, string text, Font font, BaseColor background = null)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            if (background != null)
                cell.BackgroundColor = background;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 5;
            table.AddCell(cell);
        }

        // GET: Casino/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Casino casino = db.Casino.Find(id);
            if (casino == null)
            {
                return HttpNotFound();
            }
            return View(casino);
        }

        // GET: Casino/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Casino/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Rut,Fecha,Comida,Foto,Ubicacion")] Casino casino)
        {
            if (ModelState.IsValid)
            {
                db.Casino.Add(casino);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(casino);
        }

        // GET: Casino/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Casino casino = db.Casino.Find(id);
            if (casino == null)
            {
                return HttpNotFound();
            }
            return View(casino);
        }

        // POST: Casino/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Rut,Fecha,Comida,Foto,Ubicacion")] Casino casino)
        {
            if (ModelState.IsValid)
            {
                db.Entry(casino).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(casino);
        }

        // GET: Casino/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Casino casino = db.Casino.Find(id);
            if (casino == null)
            {
                return HttpNotFound();
            }
            return View(casino);
        }

        // POST: Casino/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Casino casino = db.Casino.Find(id);
            db.Casino.Remove(casino);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
