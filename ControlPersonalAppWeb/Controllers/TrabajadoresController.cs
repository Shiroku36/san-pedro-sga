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
        private DBManejoPersonalEntities db = new DBManejoPersonalEntities();

        // GET: Trabajadores
        public ActionResult Index()
        {
            string empresa = Utils.SessionManager.CuentaAutenticada().Empresa;
            List<TrabajadorIndex> trabajadores = db.Trabajador.Where(x => x.Empresa == empresa).Select(x => new TrabajadorIndex { Id = x.Id, Nombre = x.Nombre, ApellidoPaterno = x.ApellidoPaterno, ApellidoMaterno = x.ApellidoMaterno, Rut = x.Rut, Uid = x.Uid, Gerente = x.Gerente, Empresa = x.Empresa, Contratado = x.Campo }).ToList();
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
            ViewBag.Cargos = db.Cargo.Where(x => x.IdEmpresa == empresa.Id).ToList();
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
            ViewBag.Cargos = db.Cargo.Where(x => x.IdEmpresa == empresa.Id).Select(x => x.Nombre);

            if (trabajador.Nacionalidad == null)
                trabajador.Nacionalidad = "";
            if (trabajador.Sexo == null)
                trabajador.Sexo = "";
            if (trabajador.TrampoAsignacionFamiliar == null)
                trabajador.TrampoAsignacionFamiliar = "";
            if (trabajador.CargasSimples == null)
                trabajador.CargasSimples = 0;
            if (trabajador.CargasMaternales == null)
                trabajador.CargasMaternales = 0;
            if (trabajador.CargasInvalidas == null)
                trabajador.CargasInvalidas = 0;
            if (trabajador.RegimenPrevicional == null)
                trabajador.RegimenPrevicional = "";
            if (trabajador.AsignacionFamiliar == null)
                trabajador.AsignacionFamiliar = "";
            if (trabajador.AsignacionFamiliarRetroactiva == null)
                trabajador.AsignacionFamiliarRetroactiva = "";
            if (trabajador.SolicitudTrabajadorJoven == null)
                trabajador.SolicitudTrabajadorJoven = "";
            if (trabajador.AFP == null)
                trabajador.AFP = "";
            if (trabajador.TrampoAFP == null)
                trabajador.TrampoAFP = "";
            if (trabajador.Jubiulado == null)
                trabajador.Jubiulado = "";
            if (trabajador.TipoLineaAFP == null)
                trabajador.TipoLineaAFP = "";
            if (trabajador.CodigoIntitucionSalud == null)
                trabajador.CodigoIntitucionSalud = "";
            if (trabajador.Contrato == null)
                trabajador.Contrato = "";
            if (trabajador.TipoPago == null)
                trabajador.TipoPago = "";
            if (trabajador.Colacion == null)
                trabajador.Colacion = 0;
            if (trabajador.Movilizacion == null)
                trabajador.Movilizacion = 0;
            if (trabajador.DiasVacacionesAño == null)
                trabajador.DiasVacacionesAño = 0;
            if (trabajador.Cargo == null)
                trabajador.Cargo = "";
            if (trabajador.CodigoMovimientoDePersonal == null)
                trabajador.CodigoMovimientoDePersonal = "";
            if (trabajador.InicioMovimientoPersonal == null)
                trabajador.InicioMovimientoPersonal = new DateTime();
            if (trabajador.FinMovimientoPersonal == null)
                trabajador.FinMovimientoPersonal = new DateTime();
            if (trabajador.RentaImponibleAFP == null)
                trabajador.RentaImponibleAFP = "";
            if (trabajador.CotizacionObligatoriaAFP == null)
                trabajador.CotizacionObligatoriaAFP = "";
            if (trabajador.SIS == null)
                trabajador.SIS = "";
            if (trabajador.AhorroVoluntarioAFP == null)
                trabajador.AhorroVoluntarioAFP = "";
            if (trabajador.SueldoBase == null)
                trabajador.SueldoBase = 0;
            if (trabajador.Gratificacion == null)
                trabajador.Gratificacion = 0;
            if (trabajador.SueldoBruto == null)
                trabajador.SueldoBruto = 0;
            if (trabajador.RentaImpSustitutiva == null)
                trabajador.RentaImpSustitutiva = "";
            if (trabajador.TasaPactadaSus == null)
                trabajador.TasaPactadaSus = "";
            if (trabajador.AporteIndemSus == null)
                trabajador.AporteIndemSus = "";
            if (trabajador.NPeriodosSus == null)
                trabajador.NPeriodosSus = "";
            if (trabajador.PeriodosDesdeSus == null)
                trabajador.PeriodosDesdeSus = new DateTime();
            if (trabajador.PerodoHastaSus == null)
                trabajador.PerodoHastaSus = new DateTime();
            if (trabajador.PuestoTrabajoPesado == null)
                trabajador.PuestoTrabajoPesado = "";
            if (trabajador.PorcentajeTrabajoPesado == null)
                trabajador.PuestoTrabajoPesado = "";
            if (trabajador.APVI == null)
                trabajador.APVI = "";
            if (trabajador.CodigoAPVI == null)
                trabajador.CodigoAPVI = "";
            if (trabajador.NumeroAPVI == null)
                trabajador.NumeroAPVI = "";
            if (trabajador.FormaPagoAPVI == null)
                trabajador.FormaPagoAPVI = "";
            if (trabajador.CotizacionAPVI == null)
                trabajador.CotizacionAPVI = "";
            if (trabajador.CotizaciónDepositosConvenidos == null)
                trabajador.CotizaciónDepositosConvenidos = "";
            if (trabajador.APVC == null)
                trabajador.APVC = "";
            if (trabajador.CodigoAPVC == null)
                trabajador.CodigoAPVC = "";
            if (trabajador.NumeroAPVC == null)
                trabajador.NumeroAPVC = "";
            if (trabajador.FormaPagoAPVC == null)
                trabajador.FormaPagoAPVC = "";
            if (trabajador.CotizacionTrabajadorAPVC == null)
                trabajador.CotizacionTrabajadorAPVC = "";
            if (trabajador.CotizacionEmpleadorAPVC == null)
                trabajador.CotizacionEmpleadorAPVC = "";
            if (trabajador.AfiliadoVoluntario == null)
                trabajador.AfiliadoVoluntario = "";
            if (trabajador.RutAV == null)
                trabajador.RutAV = "";
            if (trabajador.NombresAV == null)
                trabajador.NombresAV = "";
            if (trabajador.ApellidoPaternoAV == null)
                trabajador.ApellidoPaternoAV = "";
            
            if (trabajador.ApellidoMaternoAV == null)
                trabajador.ApellidoMaternoAV = "";
            if (trabajador.CodigoMovimientoPersonalAV == null)
                trabajador.CodigoMovimientoPersonalAV = "";
            if (trabajador.DesdeAV == null)
                trabajador.DesdeAV = new DateTime();
            if (trabajador.HastaAV == null)
                trabajador.HastaAV = new DateTime();
            if (trabajador.AFPAV == null)
                trabajador.AFPAV = "";
            if (trabajador.MontoCapitalizacionVoluntaria == null)
                trabajador.MontoCapitalizacionVoluntaria = "";
            if (trabajador.MontoAV == null)
                trabajador.MontoAV = "";
            if (trabajador.PeriodosAV == null)
                trabajador.PeriodosAV = "";
            if (trabajador.CodigoExcaja == null)
                trabajador.CodigoExcaja = "";
            if (trabajador.TasaExcaja == null)
                trabajador.TasaExcaja = "";
            if (trabajador.RentaImponibleIPS == null)
                trabajador.RentaImponibleIPS = "";
            if (trabajador.CotizacionObligatoriaIPS == null)
                trabajador.CotizacionObligatoriaIPS = "";
            if (trabajador.RentaImponibleDesahucio == null)
                trabajador.RentaImponibleDesahucio = "";
            if (trabajador.CodigoExcajaDesahucio == null)
                trabajador.CodigoExcajaDesahucio = "";
            if (trabajador.TasaCotizacionDesahucio == null)
                trabajador.TasaCotizacionDesahucio = "";
            if (trabajador.CotizacionFonasa == null)
                trabajador.CotizacionFonasa = "";
            if (trabajador.CotizacionISL == null)
                trabajador.CotizacionISL = "";
            if (trabajador.BonificacionLey == null)
                trabajador.BonificacionLey = "";
            if (trabajador.DescuentoPorCargasFamiliaresISL == null)
                trabajador.DescuentoPorCargasFamiliaresISL = "";
            if (trabajador.BonosGobierno == null)
                trabajador.BonosGobierno = "";
            if (trabajador.CodigoIntitucionSalud == null)
                trabajador.CodigoIntitucionSalud = "";
            if (trabajador.NumeroFun == null)
                trabajador.NumeroFun = "";
            if (trabajador.RentaImponibleIsapre == null)
                trabajador.RentaImponibleIsapre = "";
            if (trabajador.MonedaDelPlanPactadoIsapre == null)
                trabajador.MonedaDelPlanPactadoIsapre = "";
            if (trabajador.CotizacionPactada == null)
                trabajador.CotizacionPactada = "";
            if (trabajador.CotizacionIsapre == null)
                trabajador.CotizacionIsapre = "";
            if (trabajador.MontoGES == null)
                trabajador.MontoGES = "";
            if (trabajador.CodigoCCAF == null)
                trabajador.CodigoCCAF = "";
            if (trabajador.RentaImponibleCCAF == null)
                trabajador.RentaImponibleCCAF = "";
            if (trabajador.CreditosPersonalesCCAF == null)
                trabajador.CreditosPersonalesCCAF = "";
            if (trabajador.DescuentoDentalCCAF == null)
                trabajador.DescuentoDentalCCAF = "";
            if (trabajador.DescuentosPorLeasing == null)
                trabajador.DescuentosPorLeasing = "";
            if (trabajador.DescuentoPorSeguroDeVidaCCAF == null)
                trabajador.DescuentoPorSeguroDeVidaCCAF = "";
            if (trabajador.DescuentoCargasFamiliaresCCAF == null)
                trabajador.DescuentoCargasFamiliaresCCAF = "";
            if (trabajador.OtrosDescuentosCCAF == null)
                trabajador.OtrosDescuentosCCAF = "";
            if (trabajador.CotizacionCCAFNoAfiniladosIsapres == null)
                trabajador.CotizacionCCAFNoAfiniladosIsapres = "";
            if (trabajador.OtrosDescuentosCCAF1 == null)
                trabajador.OtrosDescuentosCCAF1 = "";
            if (trabajador.OtrosDescuentosCCAF2 == null)
                trabajador.OtrosDescuentosCCAF2 = "";
            if (trabajador.BonosGobiernoCCAF == null)
                trabajador.BonosGobiernoCCAF = "";
            if (trabajador.CodigoSucursalCCAF == null)
                trabajador.CodigoSucursalCCAF = "";
            if (trabajador.CodigoMutualidad == null)
                trabajador.CodigoMutualidad = "";
            if (trabajador.RentaImponibleMutual == null)
                trabajador.RentaImponibleMutual = "";
            if (trabajador.CotizacionAccidenteMutual == null)
                trabajador.CotizacionAccidenteMutual = "";
            if (trabajador.SucursalParaPagoMutual == null)
                trabajador.SucursalParaPagoMutual = "";
            if (trabajador.OtrosDescuentosCCAF2 == null)
                trabajador.OtrosDescuentosCCAF2 = "";
            if (trabajador.SeguroCesantia == null)
                trabajador.SeguroCesantia = "";
            if (trabajador.RentaImponibleSeguroCensantia == null)
                trabajador.RentaImponibleSeguroCensantia = "";
            if (trabajador.AporteTrabajadorSeguroCensatia == null)
                trabajador.AporteTrabajadorSeguroCensatia = "";
            if (trabajador.AporteEmpleadorSeguroCesantia == null)
                trabajador.AporteEmpleadorSeguroCesantia = "";
            if (trabajador.Subsidio == null)
                trabajador.Subsidio = "";
            if (trabajador.DatosEmpresa == null)
                trabajador.DatosEmpresa = "";

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
