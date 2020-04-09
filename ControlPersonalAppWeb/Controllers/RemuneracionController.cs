using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControlPersonalAppWeb.Controllers
{
    public class RemuneracionController : Controller
    {
        DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        // GET: Remurenaciones
        public ActionResult Index(int id)
        {
            DBManejoPersonalEntities database = new DBManejoPersonalEntities();
            Trabajador trabajador = database.Trabajador.First(x => x.Id == id);
            int mes = DateTime.Now.Month - 1;
            var registros = database.RegistroTrabajador.Where(x => x.Uid == trabajador.Uid && x.Fecha.Month == mes).Select(x => new { Fecha = x.Fecha }).ToList();
            registros = registros.OrderByDescending(o => o.Fecha).ToList();
            int dias = 0;
            DateTime fecha = DateTime.Now;
            foreach (var registro in registros)
            {
                if(registro.Fecha.Day != fecha.Day)
                {
                    dias++;
                }
                fecha = registro.Fecha;
            }
            ViewBag.dias = dias;
            return View(database.Trabajador.First(x => x.Id == id));
        }

        // GET: Remurenaciones/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Remurenaciones/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Remurenaciones/Create
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

        // GET: Remurenaciones/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Remurenaciones/Edit/5
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

        // GET: Remurenaciones/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Remurenaciones/Delete/5
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
