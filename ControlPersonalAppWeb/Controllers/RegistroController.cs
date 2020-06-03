using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Data.Entity.Validation;
using System.IO;
using ControlPersonalAppWeb.Models;

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
            string empresa = trabajador.Empresa;
            var campos = database.Campos.Select(x => new { x.Nombre, x.Empresa }).Where(x => x.Empresa == empresa).ToList();
            int count = 0;
            string[] nombres = new string[campos.Count];
            foreach (var campo in campos)
            {
                nombres[count] = campo.Nombre;
                count++;
            }
            ViewBag.Campos = nombres;
            ViewBag.Uid = trabajador.Uid;
            ViewBag.Id = id;
            return View();
        }
        public byte[] getImage()
        {
            Image img = Image.FromFile(Server.MapPath("~/App_Data/Foto.png"));
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
        // POST: Registro/Create
        [HttpPost]
        public ActionResult Create(int id, FormCollection collection)
        {
            try
            {
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                RegistroTrabajador registro = new RegistroTrabajador
                {
                    Fecha = Convert.ToDateTime(collection["Fecha"]),
                    Campo = collection["Campo"],
                    Uid = collection["Uid"],
                    Foto = getImage(),
                    Empresa = Utils.SessionManager.CuentaAutenticada().Empresa
                };
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

        // GET: Registro/Edit/5
        public ActionResult Edit(int id, int idTrabajador)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            RegistroTrabajador registroTrabajador = database.RegistroTrabajador.First(x => x.Id == id);
            ViewBag.idTrabajador = idTrabajador;
            return View(registroTrabajador);
        }

        // POST: Registro/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                DBManejoPersonalEntities database = new DBManejoPersonalEntities();
                RegistroTrabajador registroTrabajador = database.RegistroTrabajador.First(x => x.Id == id);
                registroTrabajador.Uid = collection["Uid"];
                registroTrabajador.Fecha = Convert.ToDateTime(collection["Fecha"]);
                registroTrabajador.Campo = collection["Campo"];
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
            if (empresa == "JCP")
            {
                trabajadores = database.Trabajador
                                           .Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, Uid = x.Uid, Contratado = x.Campo }).ToList();
            }
            else
            {
                trabajadores = database.Trabajador
                                        .Where(x => x.Empresa == empresa)
                                        .Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, Uid = x.Uid, Contratado = x.Campo }).ToList();
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
                        registro.NombreTrabajador = trabajador.Nombre;
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
            var campo = collection["Campo"];
            List<TrabajadorIndex> trabajadores = database.Trabajador
                                            .Where(x => x.Empresa == empresa)
                                            .Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, Uid = x.Uid, Contratado = x.Campo }).ToList();
            string date = DateTime.Now.ToShortDateString();
            DateTime dateTime = Convert.ToDateTime(date);
            int i = 0;
            while (i < trabajadores.Count)
            {
                var trabajador = trabajadores[i];
                if (campo == "Todos")
                {
                    var registry = database.RegistroTrabajador.Where(x => x.Fecha >= dateTime && x.Uid == trabajador.Uid).ToList();
                    if(registry.Count>0)
                    {
                        foreach (RegistroTrabajador registro in registry)
                        {
                            registro.IdTrabajador = trabajador.Id;
                            registro.NombreTrabajador = trabajador.Nombre;
                        }
                        registros.AddRange(registry);
                        i++;
                    }
                    else
                    {
                        trabajadores.Remove(trabajador);
                    }
                }
                else
                {
                    var registry = database.RegistroTrabajador.Where(x => x.Fecha >= dateTime && x.Uid == trabajador.Uid && x.Campo == campo).ToList();
                    if (registry.Count > 0)
                    {
                        foreach (RegistroTrabajador registro in registry)
                        {
                            registro.IdTrabajador = trabajador.Id;
                            registro.NombreTrabajador = trabajador.Nombre;
                        }
                        registros.AddRange(registry);
                        i++;
                    }
                    else
                    {
                        trabajadores.Remove(trabajador);
                    }
                }
            }
            ViewBag.trabajadores = trabajadores;
            ViewBag.campos = GetCampos();
            return View(registros.OrderByDescending(x => x.Fecha));
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
