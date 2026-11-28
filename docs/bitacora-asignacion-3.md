# Bitácora de la Asignación 3

Wilmel Rodriguez Lebron (2025-1963)

Audité lo que el agente (OpenCode con Claude Sonnet) construyó en la Práctica 5: notificaciones, cola de correos y reportes. Comparé contra mis criterios (`docs/criterios-notificaciones-reportes.md`), los requisitos RF-NOT y RF-REP, y los casos que mis criterios no cubrían: dos procesos al mismo tiempo, fechas que no coinciden con la creación y una dependencia que falla.

Para cada hallazgo seguí el mismo orden con el agente: primero le pedí **solo la prueba** y comprobé que fallaba; después le pedí la corrección, revisé el código y comprobé que pasaba; y al final hice un commit por hallazgo.

| # | Hallazgo | Parte | Gravedad | Commit |
|---|---|---|---|---|
| 1 | La cola envía dos veces el mismo correo si corren dos procesadores a la vez | Cola de correos | Media | `bf0447c` |
| 2 | El reporte de repartidores cuenta por fecha de creación y no de entrega | Reportes | Media | `e8d1508` |
| 3 | Si falla la notificación, la operación responde error aunque ya se guardó | Notificaciones | Media | `88763ab` |

Punto de partida: rama `asignacion-3` desde `main` (tag `practica-5`), con `dotnet test` en 106 de 106.

---

## Hallazgo 1: la cola envía dos veces el mismo correo

**Gravedad:** media. **Requisito:** RF-NOT-12. **Criterio:** C-19.

**Qué se pidió.** En la Práctica 5 el criterio C-19 decía: *"Dado un correo ya Enviado, cuando el procesador corre dos veces seguidas, entonces ese correo no se vuelve a enviar y su número de intentos no cambia."*

**Qué entregó el agente.** Cumplía C-19 cuando el procesador corre dos veces **una detrás de otra**, pero no cuando corren **al mismo tiempo**. El procesador leía todos los correos `Pendiente`, los enviaba y recién después guardaba el estado. Si otro procesador leía la cola en ese momento, veía el mismo correo como `Pendiente` y lo enviaba otra vez. Esto pasa de verdad en el proyecto: el README indica correr el procesador a mano con `docker compose exec enviador ...` mientras el servicio `enviador` ya lo corre solo cada 30 segundos.

Lo provoqué con una prueba de dos procesadores sobre la misma base, donde el segundo corre mientras el primero está enviando. Salida de `dotnet test` antes de corregir:

```
Despacho.Core.Tests.ColaCorreosConcurrenciaTests.ProcesarPendientesAsync_DosProcesadoresALaVez_EnvianCadaCorreoUnaSolaVez [FAIL]
  Assert.Equal() Failure: Values differ
  Expected: 1
  Actual:   2
Resumen de pruebas: total: 107; con errores: 1; correcto: 106
```

**Por qué pasó.** Criterio ausente. C-19 solo hablaba de dos vueltas **seguidas**. No escribí ningún criterio para dos procesadores **al mismo tiempo**, aunque mi propio README lo hacía posible.

**Corrección.** La hizo el agente. Primero le pedí solo la prueba (la de arriba). Después le di esta instrucción:

> Antes de enviar un correo, el procesador lo reserva y guarda esa reserva en la base. Agrega a CorreoEnCola un campo ReservadoHastaUtc (fecha UTC que puede ser nula) y márcalo como token de concurrencia en la configuración de EF. El procesador solo toma correos Pendiente que no estén reservados o cuya reserva ya venció. La reserva dura 5 minutos; usa una constante con nombre. Al reservar, si otro procesador ya lo reservó primero (conflicto de concurrencia al guardar), ese correo se salta sin enviarlo. Al terminar el envío, salga bien o mal, se limpia la reserva. La hora sale siempre de IReloj. Crea una migración nueva para el campo. No cambies los estados Pendiente, Enviado y Fallido ni el máximo de 3 intentos.

Revisé el resultado antes de aceptarlo: `ReservarAsync` recarga el correo, lo reserva y, si hay conflicto, lo salta; `MarcarEnviado` y `RegistrarFallo` limpian `ReservadoHastaUtc`; la migración `20261124174022_ReservaCorreos` solo agrega la columna. Después de corregir: `total: 107; con errores: 0; correcto: 107`.

Commit: `bf0447c Hallazgo 1: el procesador no envía dos veces el mismo correo (RF-NOT-12)`.

**Qué cambias.** Agregué el criterio **C-32** (dos procesadores al mismo tiempo no duplican) y una regla en `AGENTS.md`: todo proceso que pueda correr en dos copias a la vez reserva cada registro antes de trabajarlo y lleva una prueba con dos contextos sobre la misma base.

---

## Hallazgo 2: el reporte de repartidores cuenta por fecha de creación

**Gravedad:** media. **Requisitos:** RF-REP-02 y RF-REP-05. **Criterio:** C-22.

**Qué se pidió.** C-22 decía: *"Pregunta que responde: ¿cuántos envíos entregó cada repartidor, cuántos kilómetros recorrió y cuánto generó? Dado dos repartidores con 3 y 2 envíos entregados en el rango..."*

**Qué entregó el agente.** `ServicioReporteRepartidores` filtraba por `CreadoEnUtc`. El envío no guardaba cuándo se entregó, así que el agente usó la única fecha que había. Un envío creado el 1 de noviembre y entregado el 10 no aparecía al consultar el día 10, y sí aparecía al consultar el día 1, aunque ese día no se entregó nada.

Lo provoqué con los servicios reales y un reloj de prueba: el cliente crea el envío el 1, el repartidor lo acepta y lo entrega el 10, y el Administrador consulta el día 10. Salida antes de corregir:

```
Despacho.Negocio.Tests.ReporteRepartidoresFechaEntregaTests.ObtenerAsync_EnvioCreadoAntesDelRangoYEntregadoDentro_CuentaPorLaFechaDeEntrega [FAIL]
  Assert.Single() Failure: The collection was empty
Resumen de pruebas: total: 108; con errores: 1; correcto: 107
```

**Por qué pasó.** Criterio ambiguo. Escribí "entregados en el rango", pero no dije qué fecha se compara con el rango, y en mi ejemplo todos los envíos se creaban y entregaban el mismo día, así que no se notaba la diferencia. Tampoco revisé que el envío no tenía fecha de entrega.

**Corrección.** La hizo el agente con esta instrucción:

> Agrega a Envio una propiedad EntregadoEnUtc (fecha UTC que puede ser nula). En ServicioEnvios, cuando un envío pasa a Entregado, guarda EntregadoEnUtc con la hora de IReloj, en el mismo SaveChanges que el cambio de estado. En ServicioReporteRepartidores, filtra por EntregadoEnUtc en lugar de CreadoEnUtc. Crea una migración nueva y, después de agregar la columna, rellena EntregadoEnUtc con CreadoEnUtc para los envíos que ya están Entregado, para que no desaparezcan del reporte. Revisa si hay otro lugar que use CreadoEnUtc cuando debería usar la fecha de entrega.

Revisé: la fecha se guarda con `_reloj.AhoraUtc` antes del `SaveChangesAsync`; la migración `20261124175219_FechaEntregaEnvios` agrega la columna y después hace `UPDATE Envios SET EntregadoEnUtc = CreadoEnUtc WHERE Estado = 'Entregado'` (el estado se guarda como texto). Busqué los otros usos de `CreadoEnUtc`: los demás solo ordenan listas por fecha de creación, que es lo correcto. Después de corregir: `total: 108; con errores: 0; correcto: 108`.

Commit: `e8d1508 Hallazgo 2: el reporte de repartidores cuenta por fecha de entrega (RF-REP-05)`.

**Qué cambias.** Aclaré C-22 (el rango se aplica a `EntregadoEnUtc`), agregué el criterio **C-33** con un envío creado y entregado en días distintos, y una regla en `AGENTS.md`: un reporte filtra por la fecha del hecho que cuenta y, si esa fecha no existe, se agrega en vez de usar otra.

---

## Hallazgo 3: si falla la notificación, la operación responde error

**Gravedad:** media. **Requisitos:** RF-NOT-08 y RD-07. **Criterios:** C-04, C-05 y C-14.

**Qué se pidió.** C-04 y C-05 pedían que aceptar y entregar un envío avisara al cliente, y C-01 a C-03 lo mismo con las solicitudes de permiso. C-14 pedía que la operación no dependiera del servidor de correo.

**Qué entregó el agente.** Las notificaciones se crean en un paso aparte, después de guardar la operación. Si ese segundo paso falla, la excepción sube y la petición termina en error, aunque el envío ya quedó aceptado o la solicitud ya quedó aprobada. El repartidor ve un error, vuelve a intentar y le dicen que el envío ya no está disponible. Además, el `Resultado` que devuelve `NotificarAsync` se ignoraba.

Lo provoqué con un doble de `IServicioNotificaciones` que lanza "La base no responde", en dos pruebas: aceptar un envío y aprobar una solicitud. Salida antes de corregir:

```
Despacho.Core.Tests.NotificacionFallidaPermisosTests.AprobarAsync_FallaLaNotificacion_LaSolicitudSeApruebaIgual [FAIL]
  Assert.Null() Failure: Value is not null
  Actual: System.InvalidOperationException: La base no responde.
     at Despacho.Core.Permisos.ServicioPermisos.AprobarAsync(...) in ...\ServicioPermisos.cs:line 109
Resumen de pruebas: total: 110; con errores: 2; correcto: 108
```

**Por qué pasó.** Criterio ausente. C-14 solo cubría el servidor de correo, y no escribí ningún criterio para cuando falla la notificación interna. En la revisión de la Práctica 5 vi que notificaba después de guardar y lo di por bueno, sin preguntarme qué pasaba si ese paso fallaba.

**Corrección.** La hizo el agente con esta instrucción:

> En ServicioEnvios (aceptar y entregar) y en ServicioPermisos (crear, aprobar y rechazar), si la notificación falla, la operación igual responde éxito, porque ya quedó guardada. La falla no se oculta en silencio: se registra como advertencia con ILogger, con el id del usuario y el tipo de evento, sin datos sensibles. Agrega el ILogger como último parámetro del constructor con valor por defecto null. Si el resultado de NotificarAsync o NotificarAdministradoresAsync es una falla, regístralo también. Revisa si queda algún otro lugar donde se notifique después de guardar.

El agente puso la regla en un solo lugar, `AvisoSeguro.EjecutarAsync` en el Core, y lo usaron los cinco puntos donde se notifica (aceptar, entregar, crear, aprobar y rechazar). Revisé que registra solo el evento, el id del usuario y el tipo de error, y que el Core sigue sin usar el negocio. Después de corregir: `total: 110; con errores: 0; correcto: 110`.

Commit: `88763ab Hallazgo 3: si falla la notificación, la operación responde éxito (RF-NOT-08)`.

**Qué cambias.** Agregué el criterio **C-34** y una regla en `AGENTS.md`: las notificaciones se crean con `AvisoSeguro.EjecutarAsync` y nunca se ignora el `Resultado` de un servicio. También agregué que toda funcionalidad nueva lleve pruebas de dos peticiones al mismo tiempo y de una dependencia que falla.

---

## Comprobación final

- `dotnet test`: 110 de 110.
- `docker compose up --build`: la Api aplicó las dos migraciones nuevas al arrancar (`ALTER TABLE [Envios] ADD [EntregadoEnUtc]` y el `UPDATE` de los envíos entregados) y quedó escuchando en el puerto 8080. El `enviador` siguió corriendo cada 30 segundos sin errores.

## Lo que encontré fuera del alcance

Mientras auditaba vi que dos repartidores también podrían aceptar el mismo envío si lo hacen al mismo tiempo. Es de envíos (Práctica 4), no de lo que el agente hizo en la Práctica 5, así que no lo incluyo aquí. Lo corrijo aparte con la misma técnica del Hallazgo 1.
