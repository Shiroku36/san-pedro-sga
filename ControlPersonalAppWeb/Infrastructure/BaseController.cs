using System;
using System.Web.Mvc;

namespace ControlPersonalAppWeb.Infrastructure
{
    public abstract class BaseController : Controller
    {
        private SgajcpEntities _db;

        protected SgajcpEntities Db
        {
            get
            {
                if (_db == null)
                    _db = new SgajcpEntities();
                return _db;
            }
        }

        protected Cuentas CuentaActual
        {
            get
            {
                return Utils.SessionManager.CuentaAutenticada();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _db != null)
            {
                _db.Dispose();
                _db = null;
            }
            base.Dispose(disposing);
        }
    }
}
