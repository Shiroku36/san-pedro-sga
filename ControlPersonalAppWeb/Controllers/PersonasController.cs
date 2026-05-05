using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb;
using OfficeOpenXml;
using System.IO;
using SpreadsheetLight;
using System.Data.Entity.Validation;

namespace ControlPersonalAppWeb.Controllers
{
    public class PersonasController : Controller
    {
        private SgajcpEntities db = new SgajcpEntities();
        private const int PersonasPageSize = 100;

        private Personas ObtenerPersonaPorId(int id)
        {
            const string sql = @"
SELECT TOP 1 Id, CentroCosto1, CentroCosto2, GlosaCosto, Ubicacion, Instalacion, Rut, Nombre, GlosaPuesto
FROM dbo.Personas WITH (NOLOCK)
WHERE Id = @id";

            return db.Database.SqlQuery<Personas>(sql, new SqlParameter("@id", id)).FirstOrDefault();
        }

        // GET: Personas
        public ActionResult Index(string q = null, int page = 1)
        {
            var searchQuery = (q ?? string.Empty).Trim();
            var paginaActual = Math.Max(page, 1);
            var offset = (paginaActual - 1) * PersonasPageSize;
            var likeSearch = "%" + searchQuery + "%";

            const string sql = @"
SELECT Id, CentroCosto1, CentroCosto2, GlosaCosto, Ubicacion, Instalacion, Rut, Nombre, GlosaPuesto
FROM dbo.Personas WITH (NOLOCK)
WHERE (@searchQuery = '' 
    OR ISNULL(Rut, '') LIKE @likeSearch
    OR ISNULL(Nombre, '') LIKE @likeSearch
    OR ISNULL(CentroCosto1, '') LIKE @likeSearch
    OR ISNULL(CentroCosto2, '') LIKE @likeSearch
    OR ISNULL(GlosaCosto, '') LIKE @likeSearch
    OR ISNULL(Ubicacion, '') LIKE @likeSearch
    OR ISNULL(GlosaPuesto, '') LIKE @likeSearch)
ORDER BY Id
OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

            var personas = db.Database.SqlQuery<Personas>(
                sql,
                new SqlParameter("@searchQuery", searchQuery),
                new SqlParameter("@likeSearch", likeSearch),
                new SqlParameter("@offset", offset),
                new SqlParameter("@fetch", PersonasPageSize + 1))
                .ToList();

            var tienePaginaSiguiente = personas.Count > PersonasPageSize;
            if (tienePaginaSiguiente)
            {
                personas = personas.Take(PersonasPageSize).ToList();
            }

            ViewBag.SearchQuery = searchQuery;
            ViewBag.PaginaActual = paginaActual;
            ViewBag.TienePaginaAnterior = paginaActual > 1;
            ViewBag.TienePaginaSiguiente = tienePaginaSiguiente;
            ViewBag.PageSize = PersonasPageSize;

            return View(personas);
        }
        public string formatearRut(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut))
                return "";

            // Limpiar el RUT
            rut = rut.Replace(".", "").Replace("-", "").Replace(" ", "").Replace("\t", "").ToLower();

            if (rut.Length <= 1)
                return rut;

            // Separar número y dígito verificador
            string numero = rut.Substring(0, rut.Length - 1);
            string dv = rut.Substring(rut.Length - 1);

            return numero + "-" + dv;
        }

        public ActionResult Cargar()
        {
            SgajcpEntities db = new SgajcpEntities();
            var cuentaActual = ControlPersonalAppWeb.Utils.SessionManager.CuentaAutenticada();
            string empresa = cuentaActual != null ? cuentaActual.Empresa : string.Empty;
            var empresas = db.Empresas.Select(x => x.Nombre).ToList();
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
            HttpPostedFileBase hpf = Request.Files["csv"];
            string mensaje = "";
            bool huboErrores = false;
            if (hpf != null && hpf.ContentLength > 0)
            {
                StreamReader csvreader = new StreamReader(hpf.InputStream);
                var line1 = csvreader.ReadLine(); // encabezado

                while (!csvreader.EndOfStream)
                {
                    try
                    {
                        var line = csvreader.ReadLine();
                        string[] values = line.Split(new[] { ',', ';', '\t' });

                        if (values.Length < 8) continue; // Validación mínima

                        string rut = values[5]?.Trim();

                        if (!string.IsNullOrEmpty(rut))
                        {
                            rut = formatearRut(rut); // Usa tu función si ya existe

                            var personaExistente = db.Personas.FirstOrDefault(p => p.Rut == rut);

                            if (personaExistente != null)
                            {
                                // Actualiza datos existentes
                                personaExistente.CentroCosto1 = values[0]?.Trim();
                                personaExistente.CentroCosto2 = values[1]?.Trim();
                                personaExistente.GlosaCosto = values[2]?.Trim();
                                personaExistente.Ubicacion = values[3]?.Trim();
                                personaExistente.Instalacion = values[4]?.Trim();
                                personaExistente.Nombre = values[6]?.Trim();
                                personaExistente.GlosaPuesto = values[7]?.Trim();
                            }
                            else
                            {
                                // Crea nuevo registro
                                var persona = new Personas
                                {
                                    CentroCosto1 = values[0]?.Trim(),
                                    CentroCosto2 = values[1]?.Trim(),
                                    GlosaCosto = values[2]?.Trim(),
                                    Ubicacion = values[3]?.Trim(),
                                    Instalacion = values[4]?.Trim(),
                                    Rut = rut,
                                    Nombre = values[6]?.Trim(),
                                    GlosaPuesto = values[7]?.Trim()
                                };

                                db.Personas.Add(persona);
                            }
                        }
                    }
                    catch (DbEntityValidationException e)
                    {
                        huboErrores = true;
                        mensaje += "\nError al guardar: " + e.Message;
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            mensaje += $"Entidad: {eve.Entry.Entity.GetType().Name}, Estado: {eve.Entry.State}\n";
                            foreach (var ve in eve.ValidationErrors)
                            {
                                mensaje += $"- Propiedad: {ve.PropertyName}, Error: {ve.ErrorMessage}\n";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        huboErrores = true;
                        mensaje += "\nError general: " + ex.Message;
                    }
                }

                try
                {
                    db.SaveChanges();
                    TempData["FeedbackType"] = huboErrores ? "info" : "success";
                    TempData["FeedbackMessage"] = string.IsNullOrEmpty(mensaje) ? "Personas cargadas correctamente." : mensaje;
                }
                catch (DbEntityValidationException e)
                {
                    huboErrores = true;
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        mensaje += $"Entidad: {eve.Entry.Entity.GetType().Name}, Estado: {eve.Entry.State}\n";
                        foreach (var ve in eve.ValidationErrors)
                        {
                            mensaje += $"- Propiedad: {ve.PropertyName}, Error: {ve.ErrorMessage}\n";
                        }
                    }

                    TempData["FeedbackType"] = "error";
                    TempData["FeedbackMessage"] = mensaje;
                }

                csvreader.Close();
            }
            else
            {
                TempData["FeedbackType"] = "error";
                TempData["FeedbackMessage"] = "Selecciona un archivo antes de continuar.";
            }

            Utils.SessionManager.log("Carga personas desde CSV");
            return RedirectToAction("Index");
        }



        // GET: Personas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Personas personas = ObtenerPersonaPorId(id.Value);
            if (personas == null)
            {
                return HttpNotFound();
            }
            return View(personas);
        }

        // GET: Personas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Personas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CentroCosto1,CentroCosto2,GlosaCosto,Ubicacion,Instalacion,Rut,Nombre,GlosaPuesto")] Personas personas)
        {
            if (ModelState.IsValid)
            {
                db.Personas.Add(personas);
                db.SaveChanges();
                TempData["FeedbackType"] = "success";
                TempData["FeedbackMessage"] = "Persona creada correctamente.";
                return RedirectToAction("Index");
            }

            return View(personas);
        }

        // GET: Personas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Personas personas = ObtenerPersonaPorId(id.Value);
            if (personas == null)
            {
                return HttpNotFound();
            }
            return View(personas);
        }

        // POST: Personas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CentroCosto1,CentroCosto2,GlosaCosto,Ubicacion,Instalacion,Rut,Nombre,GlosaPuesto")] Personas personas)
        {
            if (ModelState.IsValid)
            {
                const string sql = @"
UPDATE dbo.Personas
SET CentroCosto1 = @CentroCosto1,
    CentroCosto2 = @CentroCosto2,
    GlosaCosto = @GlosaCosto,
    Ubicacion = @Ubicacion,
    Instalacion = @Instalacion,
    Rut = @Rut,
    Nombre = @Nombre,
    GlosaPuesto = @GlosaPuesto
WHERE Id = @Id";

                db.Database.ExecuteSqlCommand(
                    sql,
                    new SqlParameter("@CentroCosto1", (object)personas.CentroCosto1 ?? DBNull.Value),
                    new SqlParameter("@CentroCosto2", (object)personas.CentroCosto2 ?? DBNull.Value),
                    new SqlParameter("@GlosaCosto", (object)personas.GlosaCosto ?? DBNull.Value),
                    new SqlParameter("@Ubicacion", (object)personas.Ubicacion ?? DBNull.Value),
                    new SqlParameter("@Instalacion", (object)personas.Instalacion ?? DBNull.Value),
                    new SqlParameter("@Rut", (object)personas.Rut ?? DBNull.Value),
                    new SqlParameter("@Nombre", (object)personas.Nombre ?? DBNull.Value),
                    new SqlParameter("@GlosaPuesto", (object)personas.GlosaPuesto ?? DBNull.Value),
                    new SqlParameter("@Id", personas.Id));
                TempData["FeedbackType"] = "success";
                TempData["FeedbackMessage"] = "Persona actualizada correctamente.";
                return RedirectToAction("Index");
            }
            return View(personas);
        }

        // GET: Personas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Personas personas = ObtenerPersonaPorId(id.Value);
            if (personas == null)
            {
                return HttpNotFound();
            }
            return View(personas);
        }

        // POST: Personas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Database.ExecuteSqlCommand("DELETE FROM dbo.Personas WHERE Id = @Id", new SqlParameter("@Id", id));
            TempData["FeedbackType"] = "success";
            TempData["FeedbackMessage"] = "Persona eliminada correctamente.";
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
