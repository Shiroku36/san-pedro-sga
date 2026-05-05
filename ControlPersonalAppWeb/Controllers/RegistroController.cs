using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Data.Entity.Validation;
using System.IO;
using ControlPersonalAppWeb.Models;
using System.Web.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Image = iTextSharp.text.Image;
using Font = iTextSharp.text.Font;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.Xml.Linq;
using Microsoft.Ajax.Utilities;

namespace ControlPersonalAppWeb.Controllers
{
    public class RegistroController : Controller
    {
        SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta => Utils.SessionManager.CuentaAutenticada();
        public ActionResult BusquedaRegistro()
        {
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            ViewBag.Campos = db.Campos.Select(x => x.Nombre).ToList();
            return View(registros);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BusquedaRegistro(FormCollection collection)
        {
            string Numero = collection["Uid"];
            string Campo = collection["Campo"];
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            if (!String.IsNullOrEmpty(Numero))
            {
                var pulsera = db.Pulseras.FirstOrDefault(x => x.Numero == Numero);
                if (pulsera != null)
                {
                    string Uid = pulsera.Uid;
                    registros = db.RegistroTrabajador.Where(x => x.Uid == Uid).ToList();
                    foreach (var registro in registros)
                    {
                        registro.NombreTrabajador = Numero;
                    }
                }
            }
            if (!String.IsNullOrEmpty(Campo))
            {
                registros = db.RegistroTrabajador.Where(x => x.Campo == Campo).ToList();
                foreach (var registro in registros)
                {
                    registro.NombreTrabajador = Campo;
                }
            }
            ViewBag.Campos = db.Campos.Select(x => x.Nombre).ToList();
            return View(registros.OrderByDescending(x => x.Fecha));
        }

        public ActionResult Registros()
        {
            SgajcpEntities database = new SgajcpEntities();
            //string uid = "p2";
            string empresa = "Invina";
            List<Trabajador> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).ToList();
            ViewBag.numero = "";
            foreach (var trabajador in trabajadores)
            {
            }
            // foreach (var registro in registros)
            //{

            //}
            //List<Trabajador> trabajadores = database.Trabajador.ToList();
            //foreach(var trabajador in trabajadores)
            //{
            //}
            //trabajador.
            //database.SaveChanges();
            return View(new List<RegistroTrabajador>());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registros(FormCollection collection)
        {

            string numero = collection["numero"];
            Pulseras pulsera = db.Pulseras.FirstOrDefault(p => p.Numero == numero);
            List<RegistroTrabajador> registros = pulsera != null
                ? db.RegistroTrabajador.Where(rt => rt.Uid == pulsera.Uid).ToList()
                : new List<RegistroTrabajador>();
            ViewBag.numero = numero;
            /*
            string[] ids = collection["ID"].Split(new char[] { ',' });
            SgajcpEntities databases = new SgajcpEntities();
            foreach (string id in ids)
            {
                var employee = databases.RegistroTrabajador.Find(int.Parse(id));
                employee.Foto = getImage();
                databases.SaveChanges();
            }*/

            return View(registros);
        }

        public ActionResult Index(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Trabajador trabajador = database.Trabajador.FirstOrDefault(x => x.Id == id);
            if (trabajador == null)
                return HttpNotFound();
            ViewBag.idTrabajador = id;
            Utils.SessionManager.trabajadorID = id;
            ViewBag.id = id;
            ViewBag.trabajador = trabajador;
            Utils.SessionManager.log("Registros índice trabajador: " + trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno);
            return View(database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid).ToList().OrderByDescending(x => x.Fecha));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FormCollection collection)
        {
            string[] ids = collection["ID"].Split(new char[] { ',' });
            SgajcpEntities databases = new SgajcpEntities();
            foreach (string id in ids)
            {
                var employee = databases.RegistroTrabajador.Find(int.Parse(id));
                employee.Uid = "p2";
                databases.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        // GET: Registro/Details/ 
        public ActionResult Details(int id, int idTrabajador)
        {
            SgajcpEntities database = new SgajcpEntities();
            RegistroTrabajador registroTrabajador = database.RegistroTrabajador.FirstOrDefault(x => x.Id == id);
            if (registroTrabajador == null)
                return HttpNotFound();
            ViewBag.idTrabajador = idTrabajador;
            Trabajador trabajador = db.Trabajador.FirstOrDefault(x => x.Id == idTrabajador);
            if (trabajador == null)
                return HttpNotFound();
            Utils.SessionManager.log("Registros detalle trabajador: " + trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno);
            return View(registroTrabajador);
        }

        // GET: Registro/Create
        public ActionResult Create(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Trabajador trabajador = database.Trabajador.FirstOrDefault(x => x.Id == id);
            if (trabajador == null)
                return HttpNotFound();
            ViewBag.Campos = getCampos("VSP");
            ViewBag.Uid = trabajador.Uid;
            ViewBag.Id = id;
            ViewBag.WorkerId = id;
            ViewBag.WorkerName = trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno;
            return View();
        }
        public string[] getCampos(string empresa)
        {
            var campos = db.Campos.Select(x => new { x.Nombre, x.Empresa }).Where(x => x.Empresa == empresa).ToList();
            int count = 0;
            string[] nombres = new string[campos.Count];
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
        // POST: Registro/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, FormCollection collection, HttpPostedFileBase file)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                RegistroTrabajador registro = new RegistroTrabajador
                {
                    Fecha = Convert.ToDateTime(collection["Fecha"]),
                    Campo = collection["Campo"],
                    Uid = collection["Uid"],
                    NombreTrabajador = collection["NombreTrabajador"],
                    IdTrabajador = 1,
                    Empresa = Utils.SessionManager.CuentaAutenticada().Empresa
                };
                HttpPostedFileBase postedFile = Request.Files["Foto"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    registro.Foto = getImageFromPostfile(postedFile, 850);
                }
                else
                {
                    registro.Foto = getImage();
                }
                database.RegistroTrabajador.Add(registro);
                Utils.SessionManager.log("Registros detalle trabajador: " + registro.NombreTrabajador);
                database.SaveChanges();
                Trabajador trabajador = db.Trabajador.First(x => x.Id == id);
                return RedirectToAction("Index", new { id = id });
            }
            catch (DbEntityValidationException e)
            {
                ViewBag.Campos = getCampos("VSP");
                ViewBag.Uid = collection["Uid"];
                ViewBag.Id = id;
                ViewBag.WorkerId = id;
                ViewBag.WorkerName = collection["NombreTrabajador"];
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
        public static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            return (System.Drawing.Image)(new Bitmap(imgToResize, size));
        }
        // GET: Registro/Edit/5
        public ActionResult Edit(int id, int idTrabajador)
        {
            SgajcpEntities database = new SgajcpEntities();
            RegistroTrabajador registroTrabajador = database.RegistroTrabajador.FirstOrDefault(x => x.Id == id);
            if (registroTrabajador == null)
                return HttpNotFound();
            ViewBag.idTrabajador = idTrabajador;
            Trabajador trabajador = database.Trabajador.FirstOrDefault(x => x.Id == idTrabajador);
            if (trabajador == null)
                return HttpNotFound();
            ViewBag.Campos = getCampos(trabajador.Empresa);
            ViewBag.WorkerName = trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno;
            return View(registroTrabajador);
        }

        // POST: Registro/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, FormCollection collection, HttpPostedFileBase file)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                RegistroTrabajador registroTrabajador = database.RegistroTrabajador.First(x => x.Id == id);
                registroTrabajador.Uid = collection["Uid"];
                registroTrabajador.Fecha = Convert.ToDateTime(collection["Fecha"]);
                registroTrabajador.Campo = collection["Campo"];
                registroTrabajador.NombreTrabajador = collection["NombreTrabajador"];
                registroTrabajador.IdTrabajador = 1;

                HttpPostedFileBase postedFile = Request.Files["Foto"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    registroTrabajador.Foto = getImageFromPostfile(postedFile, 850);
                }
                database.SaveChanges();
                int idT = Convert.ToInt32(collection["idTrabajador"]);
                Trabajador trabajador = db.Trabajador.First(x => x.Id == idT);
                Utils.SessionManager.log("Registros detalle trabajador: " + trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno);
                return RedirectToAction("Index", new { id = collection["idTrabajador"] });
            }
            catch
            {
                int idT = Convert.ToInt32(collection["idTrabajador"]);
                Trabajador trabajador = db.Trabajador.First(x => x.Id == idT);
                ViewBag.idTrabajador = idT;
                ViewBag.Campos = getCampos(trabajador.Empresa);
                ViewBag.WorkerName = trabajador.Nombre + " " + trabajador.ApellidoPaterno + " " + trabajador.ApellidoMaterno;
                return View(db.RegistroTrabajador.First(x => x.Id == id));
            }
        }

        // GET: Registro/Delete/5
        public ActionResult Delete(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            RegistroTrabajador registroTrabajador = database.RegistroTrabajador.FirstOrDefault(x => x.Id == id);
            if (registroTrabajador == null)
                return HttpNotFound();
            return View(registroTrabajador);
        }

        // POST: Registro/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            RegistroTrabajador registroTrabajador = database.RegistroTrabajador.FirstOrDefault(x => x.Id == id);
            if (registroTrabajador == null)
                return HttpNotFound();
            database.RegistroTrabajador.Remove(registroTrabajador);
            database.SaveChanges();
            return RedirectToAction("Index", new { id = Utils.SessionManager.trabajadorID });
        }
        public List<RegistroTrabajador> GetRegistrosHoy(DateTime fechaActual)
        {
            // Obtener la empresa autenticada
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;

            // Obtener los campos permitidos para la cuenta
            var camposPermitidos = db.Campos
                .Where(x => cuenta.Permisos.Contains(x.Nombre))
                .Select(x => x.Nombre)
                .ToList();

            if (!camposPermitidos.Any())
                return new List<RegistroTrabajador>();

            // Obtener todos los trabajadores de la empresa
            var trabajadores = db.Trabajador
                .Where(x => x.Uid != null)
                .Select(x => new TrabajadorIndex
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    ApellidoMaterno = x.ApellidoMaterno,
                    ApellidoPaterno = x.ApellidoPaterno,
                    Uid = x.Uid,
                    Empresa = x.Empresa
                })
                .ToList();

            // Verificar que la lista no esté vacía
            if (!trabajadores.Any())
            {
                throw new InvalidOperationException("La lista de trabajadores está vacía.");
            }


            // Obtener todos los registros de los trabajadores para la fecha actual y campos permitidos
            var registros = db.RegistroTrabajador
                .Where(x => x.Fecha.Year == fechaActual.Year &&
                            x.Fecha.Month == fechaActual.Month &&
                            x.Fecha.Day == fechaActual.Day &&
                            camposPermitidos.Contains(x.Campo))
                .ToList();

            // Asignar información adicional a los registros
            List<RegistroTrabajador> registrosCompletos = new List<RegistroTrabajador>();

            foreach (var registro in registros)
            {
                Trabajador trabajador = new Trabajador();
                foreach(var worker in trabajadores)
                {
                    if ( registro.Uid.ToLower() == worker.Uid.ToLower())
                    {
                        trabajador = new Trabajador
                        {
                            ApellidoMaterno = worker.ApellidoMaterno,
                            ApellidoPaterno = worker.ApellidoPaterno,
                            Empresa = worker.Empresa,
                            Id = worker.Id,
                            Nombre = worker.Nombre
                        };
                        break;
                    }
                }    
                if(trabajador.Empresa == null || trabajador.Nombre == null)
                {
                    registrosCompletos.Add(new RegistroTrabajador
                    {
                        Id = registro.Id,
                        Fecha = registro.Fecha,
                        Uid = registro.Uid,
                        Campo = registro.Campo,
                        IdTrabajador = 0, // Valor por defecto
                        NombreTrabajador = "Desconocido",
                        Empresa = "Desconocida",
                        Foto = registro.Foto
                        
                    });
                }
                else
                {
                    // Si se encuentra el trabajador, crear un nuevo RegistroTrabajador con la información combinada
                    registrosCompletos.Add(new RegistroTrabajador
                    {
                        Id = registro.Id,
                        Fecha = registro.Fecha,
                        Uid = registro.Uid,
                        Campo = registro.Campo,
                        IdTrabajador = trabajador.Id,
                        NombreTrabajador = $"{trabajador.Nombre} {trabajador.ApellidoPaterno} {trabajador.ApellidoMaterno}",
                        Empresa = trabajador.Empresa,
                        Foto = registro.Foto
                    });
                }
            }

            return registrosCompletos;
        }
        public ActionResult Hoy()
        {
            DateTime dateTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            ViewBag.Fecha = dateTime.ToString("dd-MM-yyyy");
            Utils.SessionManager.log("Registros hoy");
            return View(GetRegistrosHoy(dateTime).OrderByDescending(x => x.Fecha));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Hoy(FormCollection collection)
        {
            DateTime dateTime = Convert.ToDateTime(collection["Fecha"]);
            ViewBag.Fecha = dateTime.ToString("dd-MM-yyyy");
            return View(GetRegistrosHoy(dateTime).OrderByDescending(x => x.Fecha));


            /*
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
                    if(registry.Count>0)
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
                    string strHeader = "Registros del día "+ DateTime.Now.ToLongDateString();
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
                    //Add line break
                    document.Add(new Chunk("\n", fntHead));

                    List<string> titulosRegistrosxdia = new List<string> {"Fecha", "Nombre", "Tipo", "Hora", "Campo" };
                    PdfPTable tablaRegistrosxdia = new PdfPTable(titulosRegistrosxdia.Count) { WidthPercentage = 100f };
                    //tablaHorasTrabajadas.SetWidths(new int[] { 3, 1 });
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
                        if(celda.Fecha.Hour < 12)
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
                    Response.AddHeader("content-disposition", "attachment;filename=" + strHeader +".pdf");
                    Response.Write(document);
                    Response.End();
                }
            }*/
        }
        public string[] GetCampos()
        {
            SgajcpEntities database = new SgajcpEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            var campos = database.Campos.Select(x => new { x.Nombre, x.Empresa }).Where(x => x.Empresa == empresa).ToList();
            int count = 1;
            string[] nombres = new string[campos.Count + 1];
            nombres[0] = "Todos";
            foreach (var campito in campos)
            {
                nombres[count] = campito.Nombre;
                count++;
            }
            return nombres;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
