using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControlPersonalAppWeb.Models;

namespace ControlPersonalAppWeb.Controllers
{ /*
    public class HistorialController : Controller
    {
        private SgajcpEntities db = new SgajcpEntities();
        private Cuentas cuenta = Utils.SessionManager.CuentaAutenticada();
        // GET: Historial
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Trabajador(int id)
        {
            List<Historial> Historial = new List<Historial>();
            Cuentas trabajador = db.Cuentas.First(x => x.Id == id);
            ViewBag.Buscado = trabajador.Nombre + " " + trabajador.Apellido;
            List<Solicitud> solicitudes = db.Solicitud.Where(x => x.TrabajadorId == id).ToList();
            List<ControlInventario> gestiones = db.ControlInventario.Where(x => x.TrabajadorId == id).ToList();
            List<Ingreso> ingresos = db.Ingreso.Where(x => x.TrabajadorId == id).ToList();
            foreach (var solicitud in solicitudes)
            {
                List<Stock> productos = db.Stock.Where(x => x.SolicitudId == solicitud.Id).ToList();
                Historial.Add(new Historial
                {
                    Fecha = solicitud.Fecha,
                    Trabajador = trabajador.Nombre + " " + trabajador.Apellido,
                    TrabajadorId = id,
                    Productos = productos,
                    Ubicación = solicitud.Origen,
                    Destino = solicitud.Destino,
                    Tipo = "Solicitud",
                    TipoId = solicitud.Id
                }) ;
            }
            foreach (var gestion in gestiones)
            {
                List<Stock> productos = db.Stock.Where(x => x.ControlId == gestion.Id).ToList();
                Historial.Add(new Historial
                {
                    Fecha = gestion.Fecha,
                    Trabajador = trabajador.Nombre + " " + trabajador.Apellido,
                    TrabajadorId = id,
                    Productos = productos,
                    Ubicación = gestion.Ubicacion,
                    Destino = "",
                    Tipo = "Gestión",
                    TipoId = gestion.Id
                });
            }
            foreach (var ingreso in ingresos)
            {
                List<Stock> productos = db.Stock.Where(x => x.IngresoId == ingreso.Id).ToList();
                Historial.Add(new Historial
                {
                    Fecha = ingreso.Fecha,
                    Trabajador = trabajador.Nombre + " " + trabajador.Apellido,
                    TrabajadorId = id,
                    Productos = productos,
                    Ubicación = ingreso.Ubicacion,
                    Destino = "",
                    Tipo = "Ingreso",
                    TipoId = ingreso.Id
                });
            }
            Historial = Historial.OrderByDescending(x => x.Fecha).ToList();
            return View(Historial);
        }
        public ActionResult Producto(int id)
        {
            List<Historial> Historial = new List<Historial>();
            ViewBag.Buscado = db.Producto.First(x => x.Id == id).Nombre;
            List<Stock> productos = db.Stock.Where(x => x.ProductoId == id && x.Tipo == null).ToList();
            foreach (var producto in productos)
            {
                if (producto.SolicitudId != null)
                {
                    List<Stock> products = db.Stock.Where(x => x.SolicitudId == producto.SolicitudId).ToList();
                    int idd = (int)producto.SolicitudId;
                    Solicitud solicitud = db.Solicitud.First(x => x.Id == idd);
                    Historial.Add(new Historial
                    {
                        Fecha = solicitud.Fecha,
                        Trabajador = solicitud.Trabajador,
                        TrabajadorId = (int)solicitud.TrabajadorId,
                        Productos = products,
                        Ubicación = solicitud.Origen,
                        Destino = solicitud.Destino,
                        Tipo = "Solicitud",
                        TipoId = solicitud.Id
                    });
                }
                if (producto.ControlId != null)
                {
                    int idd = (int)producto.ControlId;
                    ControlInventario gestion = db.ControlInventario.First(x => x.Id == idd);
                    List<Stock> products = db.Stock.Where(x => x.ControlId == producto.ControlId).ToList();
                    Historial.Add(new Historial
                    {
                        Fecha = gestion.Fecha,
                        Trabajador = gestion.Trabajador,
                        TrabajadorId = (int)gestion.TrabajadorId,
                        Productos = products,
                        Ubicación = gestion.Ubicacion,
                        Destino = "",
                        Tipo = "Gestión",
                        TipoId = gestion.Id
                    });
                }
                if (producto.IngresoId != null)
                {
                    int idd = (int)producto.IngresoId;
                    Ingreso ingreso = db.Ingreso.First(x => x.Id == idd);
                    List<Stock> products = db.Stock.Where(x => x.IngresoId == producto.IngresoId).ToList();
                    Historial.Add(new Historial
                    {
                        Fecha = ingreso.Fecha,
                        Trabajador = ingreso.Trabajador,
                        TrabajadorId = (int)ingreso.TrabajadorId,
                        Productos = products,
                        Ubicación = ingreso.Ubicacion,
                        Destino = "",
                        Tipo = "Ingreso",
                        TipoId = ingreso.Id
                    });
                }
            }
            Historial = Historial.OrderByDescending(x => x.Fecha).ToList();
            return View(Historial);
        }
    }*/
}
