# Criterios de aceptación: historial de auditoría

Escritos antes de delegar la construcción al agente. Cada criterio se comprueba siguiendo sus pasos, sin preguntarme nada.

**Usuarios para comprobar:** un Administrador activo (A1), un Estándar que pide permisos y sube documentos (R) y otro Estándar (C). Todos con sesión iniciada.

**Endpoint que debe existir:**

| Endpoint | Quién | Para qué |
|---|---|---|
| `GET /api/auditoria?usuarioId=&entidad=&desde=AAAA-MM-DD&hasta=AAAA-MM-DD` | Administrador | Consultar el historial; todos los filtros son opcionales y se pueden combinar |

**Campos de cada registro:** `id`, `usuarioId` (quién actuó), `accion`, `entidad`, `entidadId`, `fechaUtc`, `valorAnterior` y `valorNuevo`. Ninguno puede venir vacío.

**Entidades:** `SolicitudPermiso`, `Documento` y `Usuario`.

---

## Eventos auditados

**A-01 (RF-AUD-02) Aprobar una solicitud queda registrado.**
Dado una solicitud pendiente de R, cuando A1 la aprueba, entonces hay un registro con `usuarioId` de A1, `accion` `AprobarSolicitud`, `entidad` `SolicitudPermiso`, `entidadId` de la solicitud, `valorAnterior` `Pendiente` y `valorNuevo` con el estado en que quedó.

**A-02 (RF-AUD-02) Rechazar una solicitud queda registrado con el motivo.**
Dado una solicitud pendiente de R, cuando A1 la rechaza con el motivo "Falta la licencia", entonces hay un registro con `accion` `RechazarSolicitud`, `valorAnterior` `Pendiente` y `valorNuevo` `Rechazada: Falta la licencia`.

**A-03 (RF-AUD-03) Subir un documento queda registrado.**
Dado R con sesión, cuando sube `licencia.pdf`, entonces hay un registro con `usuarioId` de R, `accion` `SubirDocumento`, `entidad` `Documento`, `entidadId` del documento, `valorAnterior` `(no existía)` y `valorNuevo` `licencia.pdf`.

**A-04 (RF-AUD-03) Eliminar un documento queda registrado.**
Dado el documento `licencia.pdf` de R, cuando R (o A1) lo elimina, entonces hay un registro con `usuarioId` de quien lo eliminó, `accion` `EliminarDocumento`, `valorAnterior` `licencia.pdf` y `valorNuevo` `(eliminado)`.

**A-05 (RF-AUD-04) Cambiar el rol queda registrado con el antes y el después.**
Dado R con rol Estándar, cuando A1 le cambia el rol a Administrador, entonces hay un registro con `usuarioId` de A1, `accion` `CambiarRol`, `entidad` `Usuario`, `entidadId` de R, `valorAnterior` `Estandar` y `valorNuevo` `Administrador`.

**A-06 (RF-AUD-04) Desactivar y reactivar quedan registrados.**
Dado R activo, cuando A1 lo desactiva y después lo reactiva, entonces hay dos registros de A1 sobre R: `DesactivarUsuario` (de `Activo` a `Desactivado`) y `ReactivarUsuario` (de `Desactivado` a `Activo`).

**A-07 (RF-AUD-05) Restablecimiento iniciado por el propio usuario.**
Dado que R pidió un código con "Recuperar contraseña", cuando restablece su contraseña con ese código, entonces hay un registro con `usuarioId` de R, `accion` `RestablecerContrasena`, `entidad` `Usuario`, `entidadId` de R, `valorAnterior` `Contraseña anterior` y `valorNuevo` `Contraseña restablecida (iniciado por el propio usuario)`.

**A-08 (RF-AUD-05) Restablecimiento forzado por un Administrador.**
Dado R activo, cuando A1 llama `POST /api/usuarios/{id}/forzar-restablecimiento` y después R restablece con el código que le llegó, entonces hay dos registros: uno de A1 con `accion` `ForzarRestablecimiento` (de `Contraseña vigente` a `Código de restablecimiento enviado`) y otro de R con `accion` `RestablecerContrasena` y `valorNuevo` `Contraseña restablecida (iniciado por un Administrador)`.

**A-09 (RF-AUD-05) Nunca se guarda la contraseña ni el código.**
Dado los registros de A-07 y A-08, cuando se busca en todo el historial la contraseña nueva, su hash o el código de 6 números, entonces no aparecen ni una vez.

**A-10 (RF-AUD-01, RD-11) Todos los campos van llenos y la fecha en UTC.**
Dado cualquier registro de A-01 a A-08, entonces los ocho campos tienen valor y `fechaUtc` es la hora de `IReloj.AhoraUtc` del momento de la operación.

**A-11 No queda un cambio sin su registro.**
Dado cualquier operación de A-01 a A-08, cuando se guarda, entonces el registro de auditoría se guarda en el mismo `SaveChanges` que el cambio: o se guardan los dos, o ninguno.

## Inmutabilidad

**A-12 (RF-AUD-06) El historial no se edita ni se borra.**
Dado el código completo, cuando se revisa, entonces no hay ningún endpoint `PUT`, `PATCH` ni `DELETE` sobre `/api/auditoria`, el servicio de auditoría no tiene métodos para editar o borrar, y si algún código intenta modificar o borrar un registro con `SaveChanges`, el `CoreDbContext` lo rechaza.

## Consulta

**A-13 (RF-AUD-07) Filtro por usuario.**
Dado registros de A1 y de R, cuando A1 consulta con `usuarioId` de R, entonces solo salen registros de R, del más nuevo al más viejo.

**A-14 (RF-AUD-07) Filtro por entidad.**
Dado registros de las tres entidades, cuando A1 consulta con `entidad=Documento`, entonces solo salen registros de documentos.

**A-15 (RF-AUD-07, RD-11) Filtro por rango de fechas con el último día completo.**
Dado un registro creado el 20 de noviembre a las 23:30 UTC, cuando A1 consulta con `desde=2026-11-20&hasta=2026-11-20`, entonces ese registro sale; y con `desde=2026-11-21&hasta=2026-11-21` no sale.

**A-16 (RF-AUD-07, RD-06) Solo el Administrador consulta.**
Dado R con sesión, cuando llama `GET /api/auditoria`, entonces responde 403. Sin token, responde 401.

**A-17 (RD-07) Un filtro inválido se rechaza sin error.**
Dado A1, cuando consulta con `usuarioId=abc`, con `entidad=Envio` (no existe), con `desde=2026-13-40` o con `desde` posterior a `hasta`, entonces responde 400 con un mensaje claro, no un 500.

## Lo que no cambia

**A-18 (RD-09)** Después de `docker compose down` (sin `-v`) y `docker compose up`, el historial sigue ahí.
**A-19 (RD-03)** Nada en `src/Despacho.Core` usa `Despacho.Negocio`.
**A-20** `dotnet test` pasa completo, incluidas las 110 pruebas que ya existían.
