using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb.Models;

namespace ControlPersonalAppWeb.Controllers
{/*
    public class HyDsController : Controller
    {
        private SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: HyDs
        public ActionResult Index(int? id)
        {
            ViewBag.Remuneracion = "HyDs";
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            ViewBag.personas = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Rut = x.Rut, Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno }).ToList();
            if (id != null && id != 0)
            {
                int idd = (int)id;
                ViewBag.trabajador = db.Trabajador.First(x => x.Id == idd);
                return View(db.HyD.Where(x => x.IdTrabajador == idd).ToList());
            }
            ViewBag.trabajador = new Trabajador { Nombre = "", Rut = "", Id = 0 };
            List<HyD> hyd = new List<HyD>();
            return View(hyd);
        }

        // GET: HyDs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HyD hyD = db.HyD.Find(id);
            if (hyD == null)
            {
                return HttpNotFound();
            }
            return View(hyD);
        }

        // GET: HyDs/Create
        public ActionResult Create(int id)
        {
            string[] hyd = { "0,5 Dia Trabajado","Aguinaldo", "Ajuste", "Asig. Familiar Retroactiva", "Asignacion Maternal",
                "Bono", "Bono 1 dia", "Bono Domingos", "Bono Participacion", "Bono Productividad", "Bono Recepcionista", "Bono Sabado",
                "Bono Sala Cuna", "Colación", "Comisión", "Dia Festivo", "Feriado Anual Pendiente", "Feriado Proporcional", "Gratificacion",
                "Indemnizacion", "Movilización", "Sueldo Part Time", "Trato", "Ahorro CCAF", "Ahorro CCAF para Vivienda", "Anticipo",
                "Credito Social CCAF", "Descuento 0.5 Ausencia", "Descuento Telefono", "Firma", "Mercaderias", "Prestamo", "Seg Vida CCAF",
                "Seguro Hogar", "Seguro Vida Caja"};
            ViewBag.hyd = hyd;
            ViewBag.id = id;
            return View();
        }

        // POST: HyDs/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombre,Moneda,Monto,Periodo,Tipo,IdTrabajador")] HyD hyD)
        {
            if (ModelState.IsValid)
            {
                db.HyD.Add(hyD);
                db.SaveChanges();
                return RedirectToAction("Index", new { id = hyD.IdTrabajador });
            }

            return View(hyD);
        }

        // GET: HyDs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HyD hyD = db.HyD.Find(id);
            if (hyD == null)
            {
                return HttpNotFound();
            }
            return View(hyD);
        }

        // POST: HyDs/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,Moneda,Monto,Periodo,Tipo,IdTrabajador")] HyD hyD)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hyD).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(hyD);
        }

        // GET: HyDs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HyD hyD = db.HyD.Find(id);
            if (hyD == null)
            {
                return HttpNotFound();
            }
            return View(hyD);
        }

        // POST: HyDs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            HyD hyD = db.HyD.Find(id);
            db.HyD.Remove(hyD);
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
    }*/
}
