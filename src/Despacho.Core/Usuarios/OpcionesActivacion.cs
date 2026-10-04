using System;

namespace Despacho.Core.Usuarios;

public sealed class OpcionesActivacion
{
    public string UrlBase { get; set; } = string.Empty;
    public TimeSpan Vigencia { get; set; } = TimeSpan.FromHours(24);
}