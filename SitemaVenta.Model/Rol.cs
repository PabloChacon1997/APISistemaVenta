using System;
using System.Collections.Generic;

namespace SistemaVenta.Model;

public partial class Rol
{
    public int IdRol { get; set; }

    public string? Nombre { get; set; }

    public DateTime? FechaRegistro { get; set; }
    public virtual ICollection<Menurol> MenuRols { get; } = new List<Menurol>();
    public virtual ICollection<Usuario> Usuarios { get; } = new List<Usuario>();
}
