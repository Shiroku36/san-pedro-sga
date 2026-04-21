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
    public class TrabajadoresController : Controller
    {
        private SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();

        // GET: Trabajadores
        public ActionResult Index()
        {
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            List<TrabajadorIndex> trabajadores = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno, Rut = x.Rut, Uid = x.Uid, Numero = x.Numero, Empresa = x.Empresa }).ToList();
            return View(trabajadores);
        }

        // GET: Trabajadores/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trabajador trabajador = db.Trabajador.Find(id);
            if (trabajador == null)
            {
                return HttpNotFound();
            }
            return View(trabajador);
        }

        // GET: Trabajadores/Create
        public ActionResult Create()
        {
            string empresaNomhbre = Utils.SessionManager.CuentaAutenticada().Empresa;
            ViewBag.Empresa = empresaNomhbre;
            var empresa = db.Empresas.First(x => x.Nombre == empresaNomhbre);
            //ViewBag.Cargos = db.Cargo.Where(x => x.IdEmpresa == empresa.Id).ToList();
            return View();
        }

        // POST: Trabajadores/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombre,ApellidoPaterno,ApellidoMaterno,Rut,TipoCuenta,Banco,NumeroCuenta,Contratado,CodPersonal,EstadoCivil,Nacionalidad,FechaNacimiento,Direccion,Sexo,Ciudad,Telefono,Email,CargasFamiliares,TrampoAsignacionFamiliar,CargasSimples,CargasMaternales,CargasInvalidas,RegimenPrevicional,AsignacionFamiliar,AsignacionFamiliarRetroactiva,ReintegroCargasFamiliares,SolicitudTrabajadorJoven,AFP,TipoLineaAFP,RentaImponibleAFP,CotizacionObligatoriaAFP,SIS,AhorroVoluntarioAFP,RentaImpSustitutiva,TasaPactadaSus,AporteIndemSus,NPeriodosSus,PeriodosDesdeSus,PuestoTrabajoPesado,PerodoHastaSus,PorcentajeTrabajoPesado,MontoTrabajoPesado,CodigoMovimientoDePersonal,InicioMovimientoPersonal,FinMovimientoPersonal,TrampoAFP,Jubiulado,SeguroCesantia,Mutual,Salud,NombreEmer,VinculoEmer,DireccionEmer,TelefonoEmer,Cargo,NombreJefe,Campo,Empleador,Contrato,Jornada,FechaIngreso,FechaTermino,SueldoBase,Gratificacion,SueldoBruto,TipoPago,FactorHE,Colacion,Movilizacion,DiasVacacionesAño,JefeDirecto,Gerente,FotoCarnet,Foto,Uid,Empresa,APVI,CodigoAPVI,NumeroAPVI,FormaPagoAPVI,CotizacionAPVI,CotizaciónDepositosConvenidos,APVC,CodigoAPVC,NumeroAPVC,FormaPagoAPVC,CotizacionTrabajadorAPVC,CotizacionEmpleadorAPVC,AfiliadoVoluntario,RutAV,ApellidoPaternoAV,ApellidoMaternoAV,NombresAV,CodigoMovimientoPersonalAV,DesdeAV,HastaAV,AFPAV,MontoCapitalizacionVoluntaria,MontoAV,PeriodosAV,CodigoExcaja,TasaExcaja,RentaImponibleIPS,CotizacionObligatoriaIPS,RentaImponibleDesahucio,CodigoExcajaDesahucio,TasaCotizacionDesahucio,CotizaciónDesahucio,CotizacionFonasa,CotizacionISL,BonificacionLey,DescuentoPorCargasFamiliaresISL,BonosGobierno,CodigoIntitucionSalud,NumeroFun,RentaImponibleIsapre,MonedaDelPlanPactadoIsapre,CotizacionPactada,CotizacionIsapre,CotizacionIsapreAV,MontoGES,CodigoCCAF,RentaImponibleCCAF,CreditosPersonalesCCAF,DescuentoDentalCCAF,DescuentosPorLeasing,DescuentoPorSeguroDeVidaCCAF,OtrosDescuentosCCAF,CotizacionCCAFNoAfiniladosIsapres,DescuentoCargasFamiliaresCCAF,OtrosDescuentosCCAF1,OtrosDescuentosCCAF2,BonosGobiernoCCAF,CodigoSucursalCCAF,CodigoMutualidad,RentaImponibleMutual,CotizacionAccidenteMutual,SucursalParaPagoMutual,RentaImponibleSeguroCensantia,AporteTrabajadorSeguroCensatia,AporteEmpleadorSeguroCesantia,Subsidio,DatosEmpresa")] Trabajador trabajador)
        {
            if (ModelState.IsValid)
            {
                db.Trabajador.Add(trabajador);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(trabajador);
        }

        // GET: Trabajadores/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trabajador trabajador = db.Trabajador.Find(id);
            if (trabajador == null)
            {
                return HttpNotFound();
            }
            string empresaNomhbre = Utils.SessionManager.CuentaAutenticada().Empresa;
            ViewBag.Empresa = empresaNomhbre;
            var empresa = db.Empresas.First(x => x.Nombre == empresaNomhbre);
            //ViewBag.Cargos = db.Cargo.Where(x => x.IdEmpresa == empresa.Id).Select(x => x.Nombre);

            if (trabajador.Sexo == null)
                trabajador.Sexo = "";

            return View(trabajador);
        }

        // POST: Trabajadores/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,ApellidoPaterno,ApellidoMaterno,Rut,TipoCuenta,Banco,NumeroCuenta,Contratado,CodPersonal,EstadoCivil,Nacionalidad,FechaNacimiento,Direccion,Sexo,Ciudad,Telefono,Email,CargasFamiliares,TrampoAsignacionFamiliar,CargasSimples,CargasMaternales,CargasInvalidas,RegimenPrevicional,AsignacionFamiliar,AsignacionFamiliarRetroactiva,ReintegroCargasFamiliares,SolicitudTrabajadorJoven,AFP,TipoLineaAFP,RentaImponibleAFP,CotizacionObligatoriaAFP,SIS,AhorroVoluntarioAFP,RentaImpSustitutiva,TasaPactadaSus,AporteIndemSus,NPeriodosSus,PeriodosDesdeSus,PuestoTrabajoPesado,PerodoHastaSus,PorcentajeTrabajoPesado,MontoTrabajoPesado,CodigoMovimientoDePersonal,InicioMovimientoPersonal,FinMovimientoPersonal,TrampoAFP,Jubiulado,SeguroCesantia,Mutual,Salud,NombreEmer,VinculoEmer,DireccionEmer,TelefonoEmer,Cargo,NombreJefe,Campo,Empleador,Contrato,Jornada,FechaIngreso,FechaTermino,SueldoBase,Gratificacion,SueldoBruto,TipoPago,FactorHE,Colacion,Movilizacion,DiasVacacionesAño,JefeDirecto,Gerente,FotoCarnet,Foto,Uid,Empresa,APVI,CodigoAPVI,NumeroAPVI,FormaPagoAPVI,CotizacionAPVI,CotizaciónDepositosConvenidos,APVC,CodigoAPVC,NumeroAPVC,FormaPagoAPVC,CotizacionTrabajadorAPVC,CotizacionEmpleadorAPVC,AfiliadoVoluntario,RutAV,ApellidoPaternoAV,ApellidoMaternoAV,NombresAV,CodigoMovimientoPersonalAV,DesdeAV,HastaAV,AFPAV,MontoCapitalizacionVoluntaria,MontoAV,PeriodosAV,CodigoExcaja,TasaExcaja,RentaImponibleIPS,CotizacionObligatoriaIPS,RentaImponibleDesahucio,CodigoExcajaDesahucio,TasaCotizacionDesahucio,CotizaciónDesahucio,CotizacionFonasa,CotizacionISL,BonificacionLey,DescuentoPorCargasFamiliaresISL,BonosGobierno,CodigoIntitucionSalud,NumeroFun,RentaImponibleIsapre,MonedaDelPlanPactadoIsapre,CotizacionPactada,CotizacionIsapre,CotizacionIsapreAV,MontoGES,CodigoCCAF,RentaImponibleCCAF,CreditosPersonalesCCAF,DescuentoDentalCCAF,DescuentosPorLeasing,DescuentoPorSeguroDeVidaCCAF,OtrosDescuentosCCAF,CotizacionCCAFNoAfiniladosIsapres,DescuentoCargasFamiliaresCCAF,OtrosDescuentosCCAF1,OtrosDescuentosCCAF2,BonosGobiernoCCAF,CodigoSucursalCCAF,CodigoMutualidad,RentaImponibleMutual,CotizacionAccidenteMutual,SucursalParaPagoMutual,RentaImponibleSeguroCensantia,AporteTrabajadorSeguroCensatia,AporteEmpleadorSeguroCesantia,Subsidio,DatosEmpresa")] Trabajador trabajador)
        {
            if (ModelState.IsValid)
            {
                db.Entry(trabajador).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(trabajador);
        }

        // GET: Trabajadores/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trabajador trabajador = db.Trabajador.Find(id);
            if (trabajador == null)
            {
                return HttpNotFound();
            }
            return View(trabajador);
        }

        // POST: Trabajadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Trabajador trabajador = db.Trabajador.Find(id);
            db.Trabajador.Remove(trabajador);
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
