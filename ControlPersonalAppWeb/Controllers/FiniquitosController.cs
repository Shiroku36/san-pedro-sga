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
{
    public class FiniquitosController : Controller
    {
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: Finiquitos
        public ActionResult Index(int? id)
        {
            ViewBag.Remuneracion = "Finiquitos";

            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            ViewBag.personas = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Rut = x.Rut, Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno }).ToList();
            if (id != null && id != 0)
            {
                int idd = (int)id;
                ViewBag.trabajador = db.Trabajador.First(x => x.Id == idd);
                return View(db.Finiquito.ToList());
            }
            ViewBag.trabajador = new Trabajador { Nombre = "", Rut = "", Id = 0 };
            List<Finiquito> finiquitos = new List<Finiquito>();
            return View(finiquitos);
        }

        // GET: Finiquitos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Finiquito finiquito = db.Finiquito.Find(id);
            if (finiquito == null)
            {
                return HttpNotFound();
            }
            return View(finiquito);
        }

        // GET: Finiquitos/Create
        public ActionResult Create(int id)
        {
            Trabajador trabajador = db.Trabajador.First(x => x.Id == id);
            Vacaciones vacaciones = db.Vacaciones.First(x => x.IdTrabajador == id);
            Finiquito finiquito = new Finiquito()
            {
                AvisoPrevio = trabajador.SueldoBruto,
                Indemnización = trabajador.SueldoBruto,
                Vacaciones = (trabajador.SueldoBruto / 30) * vacaciones.Saldo,
                FactorVacaciones = vacaciones.Saldo,
                FactorAvisoPrevio = 1,
                FactorIndemnizacion = 0,
                SueldoMensual = trabajador.SueldoBruto               
            };
            return View(finiquito);
        }

        // POST: Finiquitos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Causa,Texto,FechaDocumento,FechaFin,DiasDeVacaciones,Dias,SueldoMensual,AvisoPrevio,FactorAvisoPrevio,Indemnización,FactorIndemnizacion,Vacaciones,FactorVacaciones,Total,IdTrabajador")] Finiquito finiquito)
        {
            if (ModelState.IsValid)
            {
                db.Finiquito.Add(finiquito);
                db.SaveChanges();
                return RedirectToAction("Index", new { id = finiquito.IdTrabajador });
            }

            return View(finiquito);
        }

        // GET: Finiquitos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Finiquito finiquito = db.Finiquito.Find(id);
            if (finiquito == null)
            {
                return HttpNotFound();
            }
            return View(finiquito);
        }

        // POST: Finiquitos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Causa,Texto,FechaDocumento,FechaFin,DiasDeVacaciones,Dias,SueldoMensual,AvisoPrevio,FactorAvisoPrevio,Indemnización,FactorIndemnizacion,Vacaciones,FactorVacaciones,Total,IdTrabajador")] Finiquito finiquito)
        {
            if (ModelState.IsValid)
            {
                db.Entry(finiquito).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(finiquito);
        }

        // GET: Finiquitos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Finiquito finiquito = db.Finiquito.Find(id);
            if (finiquito == null)
            {
                return HttpNotFound();
            }
            return View(finiquito);
        }

        // POST: Finiquitos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Finiquito finiquito = db.Finiquito.Find(id);
            db.Finiquito.Remove(finiquito);
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
