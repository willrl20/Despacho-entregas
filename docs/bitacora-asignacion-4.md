# Bitácora de la Asignación 4

Wilmel Rodriguez Lebron (2025-1963)

Construí el historial de auditoría del Core con el agente (OpenCode con Claude Sonnet). Antes de delegar escribí los criterios en `docs/criterios-auditoria.md` (A-01 a A-20) y los subí en su propio commit (`dca3865`), para que se vea en el historial que fueron primero.

## Tareas que le delegué

| Tarea | Qué le pedí | Commit |
|---|---|---|
| 1. Registro de eventos e inmutabilidad | Crear `RegistroAuditoria`, anotar los nueve eventos en el mismo guardado que el cambio, que `CoreDbContext` rechace modificar o borrar registros, la migración y una prueba por evento que revise todos los campos | `612b4b5` |
| 2. Consulta | `GET /api/auditoria` solo para el Administrador, con filtros por usuario, entidad y fechas, rechazo controlado de filtros inválidos, sus pruebas y la sección Auditoría del README | `eab1adc` |
| 3. Corrección | Que una petición que no cambia nada no deje registro (A-21) | `ec82ba3` |

## Qué me devolvió

En la tarea 1: la carpeta `src/Despacho.Core/Auditoria` (entidad, constantes de acciones, entidades y textos, y la extensión `RegistrarAuditoria`), los cambios en los cuatro servicios del Core, la migración `20261128045836_AuditoriaRegistros` (tabla nueva y la columna `ForzadoPorAdministradorId` en `CodigosRecuperacion`) y 13 pruebas. `dotnet test`: 123 de 123.

En la tarea 2: `ServicioAuditoria` con un solo método de consulta, `AuditoriaEndpoints` con un solo `MapGet` y `RequiereRol(Rol.Administrador)`, 10 pruebas y la sección del README. `dotnet test`: 133 de 133.

## Errores del agente

### Error 1: registraba cambios que no ocurrieron

**Cómo lo detecté.** Seguí el README en Docker con mis cuentas reales. Sin querer llamé dos veces a reactivar al mismo usuario, y al consultar el historial apareció esto:

```
{"id":11,"usuarioId":2003,"accion":"ReactivarUsuario","entidad":"Usuario","entidadId":1002,
 "valorAnterior":"Activo","valorNuevo":"Activo"}
```

El usuario ya estaba activo, no cambió nada, y el historial dice que el Administrador lo reactivó. Quien revise el historial cree que hubo dos reactivaciones. Y como el historial no se puede borrar, ese registro falso se queda para siempre.

**Por qué pasó.** Criterio ausente. A-06 decía "de `Activo` a `Desactivado`", pero no dije qué hacer si el usuario ya estaba así. El agente calculaba el valor anterior leyendo el estado actual y registraba siempre, sin comprobar si había cambio.

**Cómo lo corregí.** Primero le pedí solo las pruebas, y fallaron las tres, lo que mostró que el error estaba en tres métodos y no solo en el que encontré:

```
AuditoriaSinCambioTests.CambiarRolAsync_ElMismoRolQueYaTiene_NoCreaRegistroDeAuditoria [FAIL]
AuditoriaSinCambioTests.ReactivarAsync_UsuarioQueYaEstaActivo_NoCreaRegistroDeAuditoria [FAIL]
AuditoriaSinCambioTests.DesactivarAsync_UsuarioQueYaEstaDesactivado_NoCreaRegistroDeAuditoria [FAIL]
  Assert.Empty() Failure: Collection was not empty
Resumen de pruebas: total: 136; con errores: 3; correcto: 133
```

Después le pedí la corrección: si no hay cambio, la operación responde éxito pero no registra nada. Revisé el diff: `CambiarRolAsync` y `ReactivarAsync` salen antes si no hay cambio, y `DesactivarAsync` solo registra si el usuario estaba activo, pero sigue cerrando sus sesiones como antes. Los valores anterior y nuevo ahora son fijos, así que ya no puede salir `Activo → Activo`. Revisé los otros eventos: subir y eliminar siempre cambian algo, aprobar y rechazar solo se permiten desde `Pendiente`, y restablecer siempre cambia la contraseña. Después de corregir: 136 de 136. En Docker reactivé otra vez al mismo usuario y el total de registros siguió en 12.

Agregué el criterio A-21 en `docs/criterios-auditoria.md`. Commit: `ec82ba3 Corrige la auditoría: una petición que no cambia nada no deja registro (A-21)`.

### Error 2: intentó salir del repositorio

Durante la tarea 1, OpenCode pidió permiso para "Access external directory C:\" con el patrón `C:\*`. Mi `AGENTS.md` dice que trabaje solo dentro del repositorio. Lo detecté por el aviso de permiso, elegí **Reject** y le recordé la regla. Siguió la tarea sin salir del proyecto.

### Error 3: no siguió una instrucción al pie de la letra

Le pedí: "No uses un SaveChanges aparte para la auditoría". Al revisar el diff de `CoreDbContext` vi que usa dos: primero guarda la operación y después los registros. Lo hizo porque, al subir un documento nuevo, su `Id` no existe hasta que se guarda, y el registro lo necesita en `entidadId`. Los dos guardados van dentro de una transacción: si el segundo falla, se deshace el primero.

La causa fue una instrucción incompleta mía: no pensé en los registros de entidades que todavía no tienen `Id`. Lo acepté después de comprobar dos cosas: que mi `Program.cs` no usa reintentos automáticos de SQL Server (que no permiten transacciones propias) y que en Docker el registro de subir documento quedó con el `entidadId` real (1002) y no con 0.

## Decisiones de diseño

- **Dónde se escribe.** Cada servicio agrega el registro al mismo `CoreDbContext` antes de guardar, y `CoreDbContext` guarda el cambio y su registro en la misma transacción. No puede quedar un cambio sin su registro.
- **Valor anterior y nuevo.** Texto corto con solo lo que cambió: el estado (`Pendiente`, `Aplicada`), el rol, `Activo` o `Desactivado`, o el nombre del archivo. Para crear y borrar uso textos fijos, `(no existía)` y `(eliminado)`, para que ningún campo quede vacío. En contraseñas solo guardo textos fijos: nunca la contraseña, el hash ni el código.
- **RD-03.** Todo lo auditado vive en el Core, así que la auditoría está en `src/Despacho.Core/Auditoria` y no usa nada de `Despacho.Negocio`. Si el negocio quisiera auditar algo, usaría el Core, no al revés.
- **Inmutabilidad.** No hay `PUT`, `PATCH` ni `DELETE` sobre `/api/auditoria`, el servicio solo consulta, y `CoreDbContext` lanza una excepción si algo intenta modificar o borrar un registro.

## Comprobación en Docker

Seguí el README con dos cuentas reales: el Administrador `dashgoenvios@gmail.com` (usuario 2003) y mi cuenta Estándar (usuario 1002). Los nueve eventos dejaron su registro con los ocho campos. Algunos de ellos:

```
{"id":3,"usuarioId":2003,"accion":"RechazarSolicitud","entidad":"SolicitudPermiso","entidadId":1002,
 "valorAnterior":"Pendiente","valorNuevo":"Rechazada: Falta la licencia"}
{"id":5,"usuarioId":1002,"accion":"SubirDocumento","entidad":"Documento","entidadId":1002,
 "valorAnterior":"(no existía)","valorNuevo":"licencia.pdf"}
{"id":2,"usuarioId":1002,"accion":"RestablecerContrasena","entidad":"Usuario","entidadId":1002,
 "valorAnterior":"Contraseña anterior","valorNuevo":"Contraseña restablecida (iniciado por un Administrador)"}
{"id":12,"usuarioId":1002,"accion":"RestablecerContrasena","entidad":"Usuario","entidadId":1002,
 "valorAnterior":"Contraseña anterior","valorNuevo":"Contraseña restablecida (iniciado por el propio usuario)"}
```

El código de recuperación que usé no aparece en ningún registro. La consulta respondió:

| Petición | Respuesta |
|---|---|
| Con el token del Estándar | `403 {"error":"No tienes permiso para esta operación."}` |
| Sin token | `401 {"error":"Sesión no válida."}` |
| `usuarioId=abc` | `400 {"error":"El usuarioId debe ser un número entero positivo."}` |
| `entidad=Envio` | `400 {"error":"La entidad debe ser SolicitudPermiso, Documento o Usuario."}` |
| `desde=2026-13-40` | `400 {"error":"Las fechas deben existir y tener el formato AAAA-MM-DD."}` |
| `desde` posterior a `hasta` | `400 {"error":"La fecha desde no puede ser posterior a la fecha hasta."}` |
| `desde=2026-11-28&hasta=2026-11-28` | `200` con los 12 registros |
| `desde=2026-11-27&hasta=2026-11-27` | `200 []` |

Después de `docker compose down` (sin `-v`) y `docker compose up --build -d`, la consulta siguió devolviendo los 12 registros.

## Paralelo con el historial de git

Comparo el registro 7 de mi historial con el commit `ec82ba3` de mi repositorio.

| Commit de git | Registro de auditoría | En el commit | En el registro |
|---|---|---|---|
| Identificador (hash) | `id` | `ec82ba3`, calculado a partir del contenido; es el mismo en cualquier copia del repositorio | `7`, un número consecutivo que asigna la base de datos |
| Autor | `usuarioId` | Wilmel Rodriguez, tomado de mi `git config`; lo escribo yo y podría poner otro nombre | `2003`, tomado de la sesión del Administrador; quien actúa no lo puede elegir |
| Fecha | `fechaUtc` | La pone mi computadora, en mi hora local con la zona (-0400) | `2026-11-28T05:39:05`, la pone el servidor con `IReloj`, siempre en UTC |
| Mensaje | `accion`, `entidad` y `entidadId` | "Corrige la auditoría: una petición que no cambia nada no deja registro (A-21)", texto libre que explica el porqué | `CambiarRol` sobre `Usuario` 1002, de una lista cerrada; dice qué pasó, no por qué |
| Diff | `valorAnterior` y `valorNuevo` | Las líneas quitadas y agregadas en cada archivo | `Estandar` → `Administrador`, solo el dato que cambió |

**Lo que guarda el commit y el registro no:** el commit apunta a su commit padre, así que la historia forma una cadena; guarda una foto completa de todos los archivos y no solo lo que cambió; y su mensaje explica el porqué. Además, un commit local se puede reescribir (`commit --amend`, `rebase`) antes de subirlo.

**Lo que guarda el registro y el commit no:** sobre qué cosa se actuó (`entidad` y `entidadId`), un autor que viene de la sesión y no se puede inventar, y la hora siempre en UTC. Y desde la aplicación no se puede reescribir: ni el Administrador puede editarlo ni borrarlo. Por eso el registro falso del error 1 sigue ahí, y por eso hay que evitarlo antes de guardarlo.
