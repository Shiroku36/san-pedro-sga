using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
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

        // GET: Personas
        public ActionResult Index()
        {
            List<Personas> personas = db.Personas
                        .OrderBy(x => x.Rut).ToList();
            
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
        public ActionResult Cargar(FormCollection collection)
        {
            HttpPostedFileBase hpf = Request.Files["csv"];
            string mensaje = "";
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
                        mensaje += "\nError general: " + ex.Message;
                    }
                }

                try
                {
                    db.SaveChanges();
                    ViewBag.mensaje = string.IsNullOrEmpty(mensaje) ? "Carga exitosa." : mensaje;
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        mensaje += $"Entidad: {eve.Entry.Entity.GetType().Name}, Estado: {eve.Entry.State}\n";
                        foreach (var ve in eve.ValidationErrors)
                        {
                            mensaje += $"- Propiedad: {ve.PropertyName}, Error: {ve.ErrorMessage}\n";
                        }
                    }

                    ViewBag.mensaje = mensaje;
                }

                csvreader.Close();
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
            Personas personas = db.Personas.Find(id);
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
            Personas personas = db.Personas.Find(id);
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
                db.Entry(personas).State = EntityState.Modified;
                db.SaveChanges();
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
            Personas personas = db.Personas.Find(id);
            if (personas == null)
            {
                return HttpNotFound();
            }
            db.Personas.Remove(personas);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Personas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Personas personas = db.Personas.Find(id);
            db.Personas.Remove(personas);
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
