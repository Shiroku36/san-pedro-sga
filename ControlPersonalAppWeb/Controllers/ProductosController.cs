using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb;
using ControlPersonalAppWeb.Utils;

namespace ControlPersonalAppWeb.Controllers
{
    public class ProductosController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();
        string path = "C:\\Data\\Fichas\\";
        // GET: Productos
        public ActionResult Index()
        {
            Utils.SessionManager.log("Index productos");
            return View(db.Producto.Where(x => x.EmpresaId == cuenta.EmpresaId).OrderBy(x => x.Tipo).ToList());
        }

        // GET: Productos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto producto = db.Producto.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            Utils.SessionManager.log("Detalle producto: " + producto.Nombre);
            return View(producto);
        }

        // GET: Productos/Create
        public ActionResult Create()
        {
            cargarDatos();
            return View();
        }
        public ActionResult Ficha(int Id)
        {
            if(System.IO.File.Exists(path+Id+".pdf"))
            {
                Producto producto = db.Producto.First(x => x.Id == Id);
                FileStream sourceFile = new FileStream(path+Id+".pdf", FileMode.Open);
                float FileSize;
                FileSize = sourceFile.Length;
                byte[] getContent = new byte[(int)FileSize];
                sourceFile.Read(getContent, 0, (int)sourceFile.Length);
                sourceFile.Close();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Length", getContent.Length.ToString());
                Response.AddHeader("content-disposition", "attachment;filename=" + "Ficha técnica "+producto.Nombre+".pdf");
                Response.BinaryWrite(getContent);
                Response.Flush();
                Response.End();

            }
            return RedirectToAction("Index");
        }
        // POST: Productos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection, HttpPostedFileBase file, [Bind(Include = "Id,Nombre,Unidad,Tipo,StockMin,Descripcion")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                HttpPostedFileBase postedFile = Request.Files["Foto"];
                HttpPostedFileBase pdf = Request.Files["Ficha"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    producto.Foto = getImageFromPostfile(postedFile, 850);
                    producto.Miniatura = getImageFromPostfile(postedFile, 200);
                }
                producto.Empresa = cuenta.Empresa;
                producto.EmpresaId = cuenta.EmpresaId;
                db.Producto.Add(producto);
                db.SaveChanges();
                if (pdf != null && pdf.ContentLength > 0)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    pdf.SaveAs(path + Convert.ToString(producto.Id) + ".PDF");
                }
                Utils.SessionManager.log("Agregar producto: " + producto.Nombre);
                return RedirectToAction("Index");
            }

            return View(producto);
        }
        public byte[] getImageFromPostfile(HttpPostedFileBase postedFile, int tamaño)
        {
            MemoryStream target = new MemoryStream();
            System.Drawing.Image image = System.Drawing.Image.FromStream(postedFile.InputStream, true, true);
            SizeF dimensiones = image.PhysicalDimension;
            for (int i = 100; i > 0; i--)
            {
                float dimension = (dimensiones.Width * i) / 100;
                if (dimension<=tamaño)
                {
                    int ancho = (int) dimensiones.Width * i / 100;
                    int alto = (int) dimensiones.Height * i / 100;
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
        // GET: Productos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto producto = db.Producto.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            if(producto.Foto!=null)
            {
                SessionManager.FotoPruducto = producto.Foto;
                SessionManager.MiniaturaPruducto = producto.Miniatura;
            }
            cargarDatos();
            return View(producto);
        }

        // POST: Productos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection collection, HttpPostedFileBase file, [Bind(Include = "Id,Nombre,Unidad,Tipo,StockMin,Descripcion")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                HttpPostedFileBase postedFile = Request.Files["Foto"];
                HttpPostedFileBase pdf = Request.Files["Ficha"];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    producto.Foto = getImageFromPostfile(postedFile, 850);
                    producto.Miniatura = getImageFromPostfile(postedFile, 200);
                }
                if (producto.Foto == null && Utils.SessionManager.FotoPruducto != null)
                {
                    producto.Foto = Utils.SessionManager.FotoPruducto;
                    producto.Miniatura = Utils.SessionManager.MiniaturaPruducto;
                    Utils.SessionManager.FotoPruducto = null;
                    Utils.SessionManager.MiniaturaPruducto = null;
                }
                if (pdf != null && pdf.ContentLength > 0)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    pdf.SaveAs(path + Convert.ToString(producto.Id) + ".PDF");
                }
                producto.Empresa = cuenta.Empresa;
                producto.EmpresaId = cuenta.EmpresaId;
                db.Entry(producto).State = EntityState.Modified;
                db.SaveChanges();
                Utils.SessionManager.log("Editar producto: " + producto.Nombre);
                return RedirectToAction("Index");
            }
            return View(producto);
        }

        // GET: Productos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto producto = db.Producto.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            Utils.SessionManager.log("Eliminar producto: " + producto.Nombre);
            db.Producto.Remove(producto);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Producto producto = db.Producto.Find(id);
            Utils.SessionManager.log("Eliminar producto: " + producto.Nombre);
            db.Producto.Remove(producto);
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
        public void cargarDatos()
        {
            try
            {
                string datos = db.Datos.First(x => x.EmpresaId == cuenta.EmpresaId && x.Valor == "Tipos").Campo;
                while (datos[0] == ',')
                {
                    datos = datos.Substring(1, datos.Length - 1);
                }
                ViewBag.tipos = datos.Split(new char[] { ',' }).ToList();
                datos = db.Datos.First(x => x.EmpresaId == cuenta.EmpresaId && x.Valor == "Unidades").Campo;
                while (datos[0] == ',')
                {
                    datos = datos.Substring(1, datos.Length - 1);
                }
                ViewBag.unidades = datos.Split(new char[] { ',' }).ToList();
            }
            catch
            {
                ViewBag.tipos = new string[] { "" };
                ViewBag.unidades = new string[] { "" };
            }
        }
    }
}
