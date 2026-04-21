using System;
public class CasinoConNombreViewModel
{
    public int Id { get; set; }
    public string Rut { get; set; }
    public DateTime? Fecha { get; set; }
    public string Comida { get; set; }
    public byte[] Foto { get; set; }
    public string Ubicacion { get; set; }
    public string Nombre { get; set; } // <- agregado desde Personas
}