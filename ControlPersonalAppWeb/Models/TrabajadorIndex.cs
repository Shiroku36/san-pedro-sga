using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlPersonalAppWeb.Models
{
    public class TrabajadorIndex
    {
        public int Id { get; set; }
        public string Uid { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Rut { get; set; }
        public string Gerente { get; set; }
        public string Empresa { get; set; }
        public string Contratado { get; set; }
        public string Entrada { get; set; }
        public string EntradaA { get; set; }
        public string Salida { get; set; }
        public string SalidaA { get; set; }

    }
}