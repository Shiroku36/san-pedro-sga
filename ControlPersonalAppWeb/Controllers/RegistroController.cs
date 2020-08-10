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

namespace ControlPersonalAppWeb.Controllers
{
    public class RegistroController : Controller
    {
        DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        public ActionResult Busqueda()
        {
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            return View(registros);
        }
        [HttpPost]
        public ActionResult Busqueda(FormCollection collection)
        {
            string Uid = collection["Uid"];
            List<RegistroTrabajador> registros = db.RegistroTrabajador.Where(x => x.Uid == Uid).ToList();
            if(!String.IsNullOrEmpty(collection["Nueva"]))
            { 
                if(collection["Nueva"]=="Borrar")
                {
                    foreach (var registro in registros)
                    {
                        db.RegistroTrabajador.Remove(registro);
                    }
                }
                else
                { 
                    foreach( var registro in registros)
                    {
                        registro.Uid = collection["Nueva"];
                    }
                }
                db.SaveChanges();
            }
            return View(registros);
        }

        public ActionResult Registros()
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            //string uid = "p2";
            string empresa = "Invina";
            List<Trabajador> trabajadores = database.Trabajador.Where(x => x.Empresa == empresa).ToList();
            foreach(var trabajador in trabajadores)
            {
                if(!String.IsNullOrEmpty(trabajador.Jornada))
                {
                    if(trabajador.Jornada[0]=='A')
                    {
                        trabajador.Entrada = "08:00";
                        trabajador.Salida = "12:00";
                        trabajador.EntradaA = "12:45";
                        trabajador.SalidaA = "17:45";
                    }
                    if(trabajador.Jornada[0]=='B')
                    {
                        trabajador.Entrada = "08:00";
                        trabajador.Salida = "12:00";
                        trabajador.EntradaA = "12:00";
                        trabajador.SalidaA = "15:30";
                    }
                    if(trabajador.Jornada[0]=='C')
                    {
                        trabajador.Entrada = "07:00";
                        trabajador.Salida = "11:00";
                        trabajador.EntradaA = "11:30";
                        trabajador.SalidaA = "15:30";
                    }
                    if(trabajador.Jornada[0]=='D')
                    {
                        trabajador.Entrada = "08:00";
                        trabajador.Salida = "12:00";
                        trabajador.EntradaA = "13:00";
                        trabajador.SalidaA = "18:00";
                    }
                }
            }
           // foreach (var registro in registros)
            //{

            //}
            //List<Trabajador> trabajadores = database.Trabajador.ToList();
            //foreach(var trabajador in trabajadores)
            //{
            //}
            //trabajador.
            database.SaveChanges();
            return View();
        }
        [HttpPost]
        public ActionResult Registros(FormCollection collection)
        {
            string[] ids = collection["ID"].Split(new char[] { ',' });
            DBManejoPersonalEntities databases = new DBManejoPersonalEntities();
            foreach (string id in ids)
            {
                var employee = databases.RegistroTrabajador.Find(int.Parse(id));
                employee.Foto = getImage();
                databases.SaveChanges();
            }
            return RedirectToAction("Registros");
        }

        public ActionResult Index(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Trabajador trabajador = database.Trabajador.First(x => x.Id == id);
            ViewBag.idTrabajador = id;
            Utils.SessionManager.trabajadorID = id;
            ViewBag.id = id;
            ViewBag.trabajador = trabajador;
            Utils.SessionManager.log("Registros visitados: " + trabajador.Nombre);
            return View(database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid).ToList().OrderByDescending(x => x.Fecha));
        }
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            /*string[] ids = collection["ID"].Split(new char[] { ',' });
            DBManejoPersonalEntities databases = new DBManejoPersonalEntities();
            foreach (string id in ids)
            {
                var employee = databases.RegistroTrabajador.Find(int.Parse(id));
                employee.Uid = "p2";
                databases.SaveChanges();
            }*/
            return RedirectToAction("Index");
        }
        // GET: Registro/Details/ 
        public ActionResult Details(int id, int idTrabajador)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            RegistroTrabajador registroTrabajador = database.RegistroTrabajador.First(x => x.Id == id);
            ViewBag.idTrabajador = idTrabajador;
            Trabajador trabajador = db.Trabajador.First(x => x.Id== idTrabajador);
            Utils.SessionManager.log("Detalle registro: " + trabajador.Nombre + " "+ trabajador.ApellidoMaterno + " " + trabajador.ApellidoMaterno);
            return View(registroTrabajador);
        }

        // GET: Registro/Create
        public ActionResult Create(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Trabajador trabajador = database.Trabajador.First(x => x.Id == id);
            ViewBag.Campos = getCampos(trabajador.Empresa);
            ViewBag.Uid = trabajador.Uid;
            ViewBag.Id = id;
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
        public ActionResult Create(int id, FormCollection collection, HttpPostedFileBase file)
        {
            try
            {
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
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
                database.SaveChanges();
                Trabajador trabajador = db.Trabajador.First(x => x.Id == id);
                Utils.SessionManager.log("Detalle registro: " + trabajador.Nombre + " " + trabajador.ApellidoMaterno + " " + trabajador.ApellidoMaterno);
                return RedirectToAction("Index", new { id = id});
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
        public static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            return (System.Drawing.Image)(new Bitmap(imgToResize, size));
        }
        // GET: Registro/Edit/5
        public ActionResult Edit(int id, int idTrabajador)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            RegistroTrabajador registroTrabajador = database.RegistroTrabajador.First(x => x.Id == id);
            ViewBag.idTrabajador = idTrabajador;
            Trabajador trabajador = database.Trabajador.First(x => x.Id == idTrabajador);
            ViewBag.Campos = getCampos(trabajador.Empresa);
            return View(registroTrabajador);
        }

        // POST: Registro/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection, HttpPostedFileBase file)
        {
            try
            {
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
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
                Utils.SessionManager.log("Editar registro: " + trabajador.Nombre + " " + trabajador.ApellidoMaterno + " " + trabajador.ApellidoMaterno);
                return RedirectToAction("Index", new { id = collection["idTrabajador"] });
            }
            catch
            {
                return View();
            }
        }

        // GET: Registro/Delete/5
        public ActionResult Delete(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            RegistroTrabajador registroTrabajador = database.RegistroTrabajador.First(x => x.Id == id);
            Utils.SessionManager.log("Eliminar registro : " + registroTrabajador.Fecha +" "+registroTrabajador.Uid);
            database.RegistroTrabajador.Remove(registroTrabajador);
            database.SaveChanges();
            return RedirectToAction("Index", new { id = Utils.SessionManager.trabajadorID});
        }

        // POST: Registro/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult Hoy()
        {
            List<RegistroTrabajador> registros = new List<RegistroTrabajador>();
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            List<TrabajadorIndex> trabajadores = new List<TrabajadorIndex>();
            DateTime dia = DateTime.Now;
            if (empresa == "JCP")
            {
                trabajadores = database.Trabajador
                                           .Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoMaterno = x.ApellidoMaterno, ApellidoPaterno = x.ApellidoPaterno, Uid = x.Uid, Contratado = x.Campo }).ToList();
            }
            else
            {
                trabajadores = database.Trabajador
                                        .Where(x => x.Empresa == empresa)
                                        .Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoMaterno = x.ApellidoMaterno, ApellidoPaterno = x.ApellidoPaterno, Uid = x.Uid, Contratado = x.Campo }).ToList();
            }
            string date = DateTime.Now.ToShortDateString();
            DateTime dateTime = Convert.ToDateTime(date);
            int i = 0;
            while (i<trabajadores.Count)
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
                    trabajadores.Remove(trabajadores[i]);
                }
            }
            ViewBag.trabajadores = trabajadores;
            ViewBag.campos = GetCampos();
            Utils.SessionManager.log("Registros hoy");
            return View(registros.OrderByDescending(x => x.Fecha));
        }
        [HttpPost]
        public ActionResult Hoy(FormCollection collection)
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
            }
            return RedirectToAction("Index", "Informes");
        }
        public string[] GetCampos()
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
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
    }
}
