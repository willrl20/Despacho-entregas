using Despacho.Core.Comun;
using Despacho.Core.Notificaciones;
using Despacho.Core.Permisos;
using Despacho.Negocio.Datos;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Envios;

public sealed class ServicioEnvios : IServicioEnvios
{
    private readonly NegocioDbContext _db;
    private readonly IServicioPermisos _permisos;
    private readonly IReloj _reloj;

    public const string MensajeDatosInvalidos = "Los datos del envío no son válidos.";
    public const string MensajeMuyLejos = "La distancia no puede pasar de 30 km.";
    public const string MensajeSinPermiso = "Necesitas el permiso Repartidor aprobado.";
    public const string MensajeNoExiste = "El envío no existe.";
    public const string MensajePropio = "No puedes aceptar tu propio envío.";
    public const string MensajeNoDisponible = "El envío ya no está disponible.";
    public const string MensajeNoEsTuEnvio = "No eres el repartidor de este envío.";
    public const string MensajeTransicionInvalida = "El envío no puede cambiar a ese estado.";
    public const string MensajeNoPuedeCancelar = "Solo el cliente del envío o un Administrador pueden cancelarlo.";
    public const string MensajeSinRuta = "No puedes ver la ruta de este envío.";

    private sealed record Validado(ModalidadEnvio Modalidad, TamanoPaquete Tamano, double DistanciaKm, decimal Costo);

    public const string TituloEnvioAceptado = "Tu envío fue aceptado";
    public const string TituloEnvioEntregado = "Tu envío fue entregado";

    private readonly IServicioNotificaciones _notificaciones;

    public ServicioEnvios(NegocioDbContext db, IServicioPermisos permisos, IReloj reloj, IServicioNotificaciones notificaciones)
    {
        _db = db;
        _permisos = permisos;
        _reloj = reloj;
        _notificaciones = notificaciones;
    }

    private static Resultado<Validado> Validar(DatosEnvio? datos)
    {
        if (datos == null)
            return Resultado<Validado>.Falla(MensajeDatosInvalidos);

        if (!Enum.TryParse<ModalidadEnvio>(datos.Modalidad, true, out var modalidad) || !Enum.IsDefined(modalidad))
            return Resultado<Validado>.Falla(MensajeDatosInvalidos);

        if (!Enum.TryParse<TamanoPaquete>(datos.Tamano, true, out var tamano) || !Enum.IsDefined(tamano))
            return Resultado<Validado>.Falla(MensajeDatosInvalidos);

        if (!LatitudValida(datos.OrigenLatitud) || !LatitudValida(datos.DestinoLatitud)
            || !LongitudValida(datos.OrigenLongitud) || !LongitudValida(datos.DestinoLongitud))
            return Resultado<Validado>.Falla(MensajeDatosInvalidos);

        if (string.IsNullOrWhiteSpace(datos.ContactoNombre) || datos.ContactoNombre.Length > 100)
            return Resultado<Validado>.Falla(MensajeDatosInvalidos);

        if (string.IsNullOrWhiteSpace(datos.ContactoTelefono) || datos.ContactoTelefono.Length > 20)
            return Resultado<Validado>.Falla(MensajeDatosInvalidos);

        if (string.IsNullOrWhiteSpace(datos.DescripcionPaquete) || datos.DescripcionPaquete.Length > 200)
            return Resultado<Validado>.Falla(MensajeDatosInvalidos);

        if (datos.OrigenReferencia != null && datos.OrigenReferencia.Length > 200)
            return Resultado<Validado>.Falla(MensajeDatosInvalidos);

        if (datos.DestinoReferencia != null && datos.DestinoReferencia.Length > 200)
            return Resultado<Validado>.Falla(MensajeDatosInvalidos);

        var distancia = TarifaEnvio.CalcularDistanciaKm(datos.OrigenLatitud, datos.OrigenLongitud, datos.DestinoLatitud, datos.DestinoLongitud);
        if (distancia < 0.05)
            return Resultado<Validado>.Falla(MensajeDatosInvalidos);

        if (distancia > TarifaEnvio.DistanciaMaximaKm)
            return Resultado<Validado>.Falla(MensajeMuyLejos);

        return Resultado<Validado>.Ok(new Validado(modalidad, tamano, distancia, TarifaEnvio.CalcularCosto(distancia, tamano)));
    }

    private static bool LatitudValida(double valor) => valor >= -90 && valor <= 90;

    private static bool LongitudValida(double valor) => valor >= -180 && valor <= 180;

    public Resultado<Cotizacion> Cotizar(DatosEnvio? datos)
    {
        var validado = Validar(datos);
        if (!validado.Exito)
            return Resultado<Cotizacion>.Falla(validado.Error!);

        return Resultado<Cotizacion>.Ok(new Cotizacion(validado.Valor!.DistanciaKm, validado.Valor.Costo));
    }

    public async Task<Resultado<int>> CrearAsync(int clienteId, DatosEnvio? datos)
    {
        var validado = Validar(datos);
        if (!validado.Exito)
            return Resultado<int>.Falla(validado.Error!);

        var v = validado.Valor!;
        var d = datos!;

        var envio = new Envio
        {
            ClienteId = clienteId,
            Modalidad = v.Modalidad,
            OrigenLatitud = d.OrigenLatitud,
            OrigenLongitud = d.OrigenLongitud,
            OrigenReferencia = d.OrigenReferencia?.Trim(),
            DestinoLatitud = d.DestinoLatitud,
            DestinoLongitud = d.DestinoLongitud,
            DestinoReferencia = d.DestinoReferencia?.Trim(),
            ContactoNombre = d.ContactoNombre!.Trim(),
            ContactoTelefono = d.ContactoTelefono!.Trim(),
            DescripcionPaquete = d.DescripcionPaquete!.Trim(),
            Tamano = v.Tamano,
            DistanciaKm = v.DistanciaKm,
            Costo = v.Costo,
            CreadoEnUtc = _reloj.AhoraUtc
        };

        _db.Envios.Add(envio);
        await _db.SaveChangesAsync();

        return Resultado<int>.Ok(envio.Id);
    }

    public async Task<IReadOnlyList<EnvioResumen>> ListarDeClienteAsync(int clienteId)
    {
        return await _db.Envios
            .Where(e => e.ClienteId == clienteId)
            .OrderByDescending(e => e.CreadoEnUtc)
            .Select(e => new EnvioResumen(
                e.Id,
                e.Modalidad.ToString(),
                e.Estado.ToString(),
                e.DescripcionPaquete,
                e.Tamano.ToString(),
                e.DistanciaKm,
                e.Costo,
                e.RepartidorId,
                e.CreadoEnUtc))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<EnvioResumen>> ListarTodosAsync()
    {
        return await _db.Envios
            .OrderByDescending(e => e.CreadoEnUtc)
            .Select(e => new EnvioResumen(
                e.Id,
                e.Modalidad.ToString(),
                e.Estado.ToString(),
                e.DescripcionPaquete,
                e.Tamano.ToString(),
                e.DistanciaKm,
                e.Costo,
                e.RepartidorId,
                e.CreadoEnUtc))
            .ToListAsync();
    }

    public async Task<Resultado> CancelarAsync(int usuarioId, bool esAdministrador, int envioId)
    {
        var envio = await _db.Envios.FindAsync(envioId);
        if (envio == null)
            return Resultado.Falla(MensajeNoExiste);

        if (!esAdministrador && envio.ClienteId != usuarioId)
            return Resultado.Falla(MensajeNoPuedeCancelar);

        if (!envio.CambiarEstado(EstadoEnvio.Cancelado))
            return Resultado.Falla(MensajeTransicionInvalida);

        await _db.SaveChangesAsync();

        return Resultado.Ok();
    }

    public async Task<Resultado<IReadOnlyList<EnvioDisponible>>> ListarDisponiblesAsync(int repartidorId)
    {
        if (!await _permisos.TienePermisoAsync(repartidorId, Permiso.Repartidor))
            return Resultado<IReadOnlyList<EnvioDisponible>>.Falla(MensajeSinPermiso);

        var envios = await _db.Envios
            .Where(e => e.Estado == EstadoEnvio.Pendiente)
            .OrderBy(e => e.CreadoEnUtc)
            .Select(e => new EnvioDisponible(
                e.Id,
                e.Tamano.ToString(),
                e.DistanciaKm,
                e.Costo,
                e.CreadoEnUtc))
            .ToListAsync();

        return Resultado<IReadOnlyList<EnvioDisponible>>.Ok(envios);
    }

    public async Task<Resultado> AceptarAsync(int repartidorId, int envioId)
    {
        if (!await _permisos.TienePermisoAsync(repartidorId, Permiso.Repartidor))
            return Resultado.Falla(MensajeSinPermiso);

        var envio = await _db.Envios.FindAsync(envioId);
        if (envio == null)
            return Resultado.Falla(MensajeNoExiste);

        if (envio.ClienteId == repartidorId)
            return Resultado.Falla(MensajePropio);

        if (!envio.CambiarEstado(EstadoEnvio.Asignado))
            return Resultado.Falla(MensajeNoDisponible);

        envio.RepartidorId = repartidorId;
        await _db.SaveChangesAsync();

        await _notificaciones.NotificarAsync(
            envio.ClienteId,
            TituloEnvioAceptado,
            $"Tu envío #{envio.Id} fue aceptado por un repartidor.");

        return Resultado.Ok();
    }

    public async Task<Resultado<RutaEnvio>> VerRutaAsync(int repartidorId, int envioId)
    {
        var envio = await _db.Envios.FindAsync(envioId);
        if (envio == null)
            return Resultado<RutaEnvio>.Falla(MensajeNoExiste);

        if (envio.RepartidorId != repartidorId || (envio.Estado != EstadoEnvio.Asignado && envio.Estado != EstadoEnvio.EnRuta))
            return Resultado<RutaEnvio>.Falla(MensajeSinRuta);

        return Resultado<RutaEnvio>.Ok(new RutaEnvio(
            envio.Id,
            envio.Modalidad.ToString(),
            envio.Estado.ToString(),
            envio.OrigenLatitud,
            envio.OrigenLongitud,
            envio.OrigenReferencia,
            envio.DestinoLatitud,
            envio.DestinoLongitud,
            envio.DestinoReferencia,
            envio.ContactoNombre,
            envio.ContactoTelefono,
            envio.DescripcionPaquete,
            envio.Tamano.ToString()));
    }

    public Task<Resultado> MarcarRecogidoAsync(int repartidorId, int envioId) =>
        CambiarComoRepartidorAsync(repartidorId, envioId, EstadoEnvio.EnRuta);

    public Task<Resultado> ConfirmarEntregaAsync(int repartidorId, int envioId) =>
        CambiarComoRepartidorAsync(repartidorId, envioId, EstadoEnvio.Entregado);

    private async Task<Resultado> CambiarComoRepartidorAsync(int repartidorId, int envioId, EstadoEnvio nuevo)
    {
        var envio = await _db.Envios.FindAsync(envioId);
        if (envio == null)
            return Resultado.Falla(MensajeNoExiste);

        if (envio.RepartidorId != repartidorId)
            return Resultado.Falla(MensajeNoEsTuEnvio);

        if (!envio.CambiarEstado(nuevo))
            return Resultado.Falla(MensajeTransicionInvalida);

        if (nuevo == EstadoEnvio.Entregado)
            envio.EntregadoEnUtc = _reloj.AhoraUtc;

        await _db.SaveChangesAsync();

        if (nuevo == EstadoEnvio.Entregado)
        {
            await _notificaciones.NotificarAsync(
                envio.ClienteId,
                TituloEnvioEntregado,
                $"Tu envío #{envio.Id} fue entregado.");
        }

        return Resultado.Ok();
    }
}
