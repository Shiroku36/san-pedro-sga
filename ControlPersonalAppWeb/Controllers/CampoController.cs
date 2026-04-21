using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControlPersonalAppWeb.Controllers
{
    
    public class CampoController : Controller
    {
        SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();
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

        // GET: Campo
        public ActionResult Index()
        {

            //Eliminar duplicados
            /*
            List<Trabajador> trabajadores = db.Trabajador.ToList();

            List<Trabajador> trabajadoresSD = trabajadores
            .GroupBy(p => p.Rut)
            .Select(g => g.OrderBy(p => p.Id).First())
            .ToList();
            var ids = trabajadoresSD.Select(x => x.Id).ToList();
            var duplicados = db.Trabajador.Where(x => !ids.Contains(x.Id)).ToList();
            db.Trabajador.RemoveRange(duplicados);
            db.SaveChanges();*/

            /*Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();
            string empresa = cuenta.Empresa;
            string[] nombres;
            List<Campos> campos = new List<Campos>();
            if (empresa == "JCP")
            {
                nombres = GetNombreCampos("");
            }
            else
            {
                nombres = GetNombreCampos(empresa);
            }
            foreach (var campo in nombres)
            {
                if (cuenta.Permisos.Contains(campo))
                {
                    campos.AddRange(database.Campos.Where(x => x.Nombre == campo).ToList());
                }
            }
            return View(campos);
            */
            Utils.SessionManager.log("Predio índice");
            return View(db.Campos.OrderBy(x => x.Nombre).ToList());
        }

        // GET: Campo/Details/5
        public ActionResult Details(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Campos campo = database.Campos.First(x => x.Id == id);
            Utils.SessionManager.log("Predio detalle: " + campo.Nombre);
            return View(campo);
        } 

        // GET: Campo/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Campo/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                Campos campo = new Campos
                {
                    Nombre = collection["Nombre"],
                    Empresa = collection["Empresa"],
                    Lugar = collection["Lugar"],
                    Coordenadas = collection["Coordenadas"],
                    Encargado = collection["Encargado"],
                    Correo = collection["Correo"],
                    Telefono = collection["Telefono"],
                    Asistente = collection["Asistente"],
                    TelefonoAsistente = collection["TelefonoAsistente"]
                };
                List<Cuentas> cuentas = db.Cuentas.ToList();
                foreach (var acount in cuentas)
                {
                    if(acount.Permisos!=null)
                    {
                        List<string> permisos = acount.Permisos.Split(',').ToList();
                        permisos.Add(campo.Nombre);
                        acount.Permisos = String.Join(",", permisos);
                    }
                    else
                    {
                        acount.Permisos = campo.Nombre;
                    }
                }
                database.Campos.Add(campo);
                Utils.SessionManager.log("Predio crear: " + campo.Nombre);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Campo/Edit/5
        public ActionResult Edit(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Campos campo = database.Campos.First(x => x.Id == id);
            return View(campo);
        }

        // POST: Campo/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                SgajcpEntities database = new SgajcpEntities();
                string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
                Campos campo = database.Campos.First(x => x.Id == id);
                campo.Nombre = collection["Nombre"];
                campo.Empresa = collection["Empresa"];
                campo.Lugar = collection["Lugar"];
                campo.Coordenadas = collection["Coordenadas"];
                campo.Encargado = collection["Encargado"];
                campo.Correo = collection["Correo"];
                campo.Telefono = collection["Telefono"];
                campo.Asistente = collection["Asistente"];
                campo.TelefonoAsistente = collection["TelefonoAsistente"];
                campo.Empresa = empresa;
                Utils.SessionManager.log("Predio editar: " + campo.Nombre);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Campo/Delete/5
        public ActionResult Delete(int id)
        {
            SgajcpEntities database = new SgajcpEntities();
            Campos Campo = database.Campos.First(x => x.Id == id);
            database.Campos.Remove(Campo);
            Utils.SessionManager.log("Predio eliminar: " + Campo.Nombre);
            database.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Campo/Delete/5
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
