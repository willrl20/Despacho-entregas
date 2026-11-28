# Criterios de aceptación: notificaciones, cola de correos y reportes

Escritos antes de delegar la construcción al agente. Cada criterio se comprueba siguiendo sus pasos, sin preguntarme nada.

**Usuarios para comprobar:** un Administrador activo (A1), un segundo Administrador activo (A2), un Administrador desactivado (A3), un Estándar que será repartidor (R) y un Estándar cliente (C). Todos con sesión iniciada.

**Endpoints que deben existir:**

| Endpoint | Quién | Para qué |
|---|---|---|
| `GET /api/notificaciones` | Con sesión | Mis notificaciones, de la más nueva a la más vieja, cada una con `leida` |
| `GET /api/notificaciones?soloNoLeidas=true` | Con sesión | Solo las no leídas |
| `POST /api/notificaciones/{id}/leida` | Con sesión | Marcar una notificación propia como leída |
| `PUT /api/notificaciones/preferencias` con `{"correo":false}` | Con sesión | Apagar o encender el canal de correo |
| `GET /api/correos?estado=Fallido` | Administrador | Consultar la cola (el filtro de estado es opcional) |
| `GET /api/reportes/solicitudes?desde=AAAA-MM-DD&hasta=AAAA-MM-DD` | Con sesión | Reporte del Core |
| `GET /api/reportes/repartidores?desde=AAAA-MM-DD&hasta=AAAA-MM-DD` | Con sesión | Reporte del negocio |

---

## Notificaciones

**C-01 (RF-NOT-01) Crear una solicitud avisa a los Administradores activos.**
Dado A1, A2 (activos) y A3 (desactivado), cuando R crea una solicitud del permiso Repartidor, entonces A1 y A2 tienen cada uno una notificación nueva sin leer que dice quién pidió qué permiso, y ni A3, ni R, ni C tienen notificación por ese evento.

**C-02 (RF-NOT-02) Aprobar avisa al solicitante.**
Dado una solicitud pendiente de R, cuando A1 la aprueba, entonces R tiene una notificación nueva sin leer que dice que fue **aprobada**, y ningún otro usuario recibe notificación por ese evento.

**C-03 (RF-NOT-02) Rechazar avisa al solicitante con el motivo.**
Dado una solicitud pendiente de R, cuando A1 la rechaza con el motivo "Falta la licencia", entonces R tiene una notificación nueva sin leer que dice que fue **rechazada** e incluye el texto "Falta la licencia".

**C-04 (RF-NOT-03) Aceptar un envío avisa al cliente.**
Dado un envío pendiente creado por C y R con el permiso Repartidor aprobado, cuando R acepta el envío, entonces C tiene una notificación nueva sin leer que dice que su envío (con su número) fue aceptado, y R no recibe notificación por ese evento.

**C-05 (RF-NOT-03) Entregar un envío avisa al cliente.**
Dado un envío de C en ruta, cuando su repartidor confirma la entrega, entonces C tiene una notificación nueva sin leer que dice que su envío fue entregado.

**C-06 (RF-NOT-04) Cada usuario ve solo sus notificaciones.**
Dado que A1 y R tienen notificaciones, cuando R consulta `GET /api/notificaciones`, entonces la respuesta contiene solo notificaciones de R, y ninguna de A1.

**C-07 (RF-NOT-04) Se distinguen leídas y no leídas.**
Dado que R tiene dos notificaciones y marcó una como leída, cuando consulta `GET /api/notificaciones`, entonces ve las dos con `leida` en `true` y `false`; y con `?soloNoLeidas=true` ve solo la no leída.

**C-08 (RF-NOT-05) Marcar como leída una propia.**
Dado una notificación sin leer de R, cuando R llama `POST /api/notificaciones/{id}/leida`, entonces responde 200 y la notificación aparece con `leida: true`.

**C-09 (RF-NOT-05, RD-06) No se puede marcar la de otro.**
Dado una notificación de A1, cuando R llama `POST /api/notificaciones/{id}/leida` con el id de esa notificación, entonces responde 404 (sin decir que existe) y la notificación de A1 sigue sin leer.

**C-10 (RF-NOT-06) Cada notificación llega por los dos canales.**
Dado que A1 tiene el correo activo, cuando ocurre cualquiera de los eventos de C-01 a C-05 que le corresponde, entonces A1 tiene la notificación interna y además hay un correo nuevo en la cola dirigido al correo de A1, con el mismo asunto.

**C-11 (RF-NOT-07) Apagar el correo no apaga la interna.**
Dado que R llamó `PUT /api/notificaciones/preferencias` con `{"correo":false}`, cuando A1 aprueba una solicitud de R, entonces R tiene la notificación interna y **no** se agregó ningún correo a la cola dirigido a R. Si después la vuelve a encender, el siguiente evento sí encola el correo.

**C-12 (RD-06) Todo requiere sesión.**
Dado una petición sin token o con un token cerrado, cuando se llama cualquier endpoint de `/api/notificaciones`, entonces responde 401.

**C-13 (RD-07) Un identificador inválido se rechaza sin error.**
Dado R con sesión, cuando llama `POST /api/notificaciones/999999/leida` (no existe), entonces responde 404 con un mensaje claro, no un 500.

## Cola de correos

**C-14 (RF-NOT-08) La operación no depende del servidor de correo.**
Dado que el servidor de correo no responde (por ejemplo, `SMTP_HOST=no-existe.invalid` en `.env`), cuando un usuario se registra o R crea una solicitud, entonces la operación responde con éxito y el correo queda en la cola en estado `Pendiente`.

**C-15 (RF-NOT-09) El envío lo hace el procesador aparte.**
Dado correos `Pendiente` en la cola, cuando corre el procesador (`Despacho.EnviadorCorreos`, el servicio `enviador` de Docker), entonces los envía sin que se vuelva a ejecutar la operación que los creó, y quedan en estado `Enviado` con su fecha de envío.

**C-16 (RF-NOT-10) Un correo que falla se reintenta un máximo de 3 veces.**
Dado que el servidor de correo no responde, cuando el procesador corre varias veces, entonces cada vuelta suma un intento al correo; al llegar a **3 intentos** el correo queda en estado `Fallido`, con el último error guardado, y las vueltas siguientes ya no lo intentan. El correo nunca se borra.

**C-17 (RF-NOT-11) Cada correo guarda su historia.**
Dado cualquier correo de la cola, cuando un Administrador consulta `GET /api/correos`, entonces cada correo muestra destinatario, asunto, estado (`Pendiente`, `Enviado` o `Fallido`), número de intentos, fecha de creación, fecha del último intento, fecha de envío y último error. El último error no muestra contraseñas ni trazas.

**C-18 (RF-NOT-11) Solo el Administrador consulta la cola.**
Dado R con sesión, cuando llama `GET /api/correos`, entonces responde 403.

**C-19 (RF-NOT-12) Correr el procesador dos veces no duplica.**
Dado un correo ya `Enviado`, cuando el procesador corre dos veces seguidas, entonces ese correo no se vuelve a enviar y su número de intentos no cambia.

**C-20 (RF-NOT-13) Las credenciales salen del entorno.**
Dado el repositorio completo, cuando se busca la contraseña SMTP del `.env` en todo el historial (`git log --all -p`), entonces aparece 0 veces; y el código lee `SMTP_HOST`, `SMTP_PUERTO`, `SMTP_USUARIO`, `SMTP_CONTRASENA` y `SMTP_REMITENTE` de variables de entorno.

## Reportes

**C-21 (RF-REP-01, RF-REP-03) Reporte del Core: solicitudes por estado.**
Dado 5 solicitudes en el rango (2 Pendiente, 2 Aplicada, 1 Rechazada), cuando A1 consulta `GET /api/reportes/solicitudes` con ese rango, entonces recibe **3 filas** (una por estado, con su cantidad: 2, 2 y 1), no las 5 solicitudes. Los estados sin solicitudes no aparecen.

**C-22 (RF-REP-02, RF-REP-03) Reporte del negocio: lo que hizo cada repartidor.**
Pregunta que responde: *¿cuántos envíos entregó cada repartidor, cuántos kilómetros recorrió y cuánto generó?* Dado dos repartidores con 3 y 2 envíos entregados en el rango, cuando A1 consulta `GET /api/reportes/repartidores`, entonces recibe **2 filas**, una por repartidor, con la cantidad de envíos entregados, la suma de `DistanciaKm` y la suma de `Costo`, ordenadas de mayor a menor por lo generado. Solo cuenta envíos en estado `Entregado`. El rango se aplica a la fecha de entrega (`EntregadoEnUtc`), no a la fecha de creación (ver C-33).

**C-23 (RF-REP-03) La agregación la hace la base de datos.**
Dado el código de los dos reportes, cuando se revisa, entonces la agrupación y los totales se calculan en la consulta (`GroupBy` con `Count`/`Sum` traducidos a SQL), no trayendo todos los registros a memoria para sumarlos.

**C-24 (RF-REP-04) Un Estándar solo ve lo suyo.**
Dado R y otro repartidor con envíos entregados, cuando R consulta `GET /api/reportes/repartidores`, entonces recibe solo su propia fila. Y cuando R consulta `GET /api/reportes/solicitudes`, solo se cuentan las solicitudes que él creó.

**C-25 (RF-REP-05) El rango de fechas cambia el resultado.**
Dado solicitudes y envíos creados en dos semanas distintas, cuando se consulta cada reporte con el rango de la primera semana y luego con el de la segunda, entonces los resultados son distintos y cada uno cuenta solo lo de su semana.

**C-26 (RF-REP-05, RD-11) El último día se incluye completo.**
Dado una solicitud creada el 20 de noviembre a las 23:30 UTC, cuando se consulta con `desde=2026-11-20&hasta=2026-11-20`, entonces esa solicitud se cuenta. Las fechas del filtro se interpretan en UTC: `desde` desde las 00:00 y `hasta` hasta las 23:59:59 de ese día.

**C-27 (RD-07) Fechas inválidas se rechazan.**
Dado cualquier reporte, cuando se pide con `desde` posterior a `hasta`, o con una fecha que no existe (`2026-13-40`), entonces responde 400 con un mensaje claro, no un 500.

**C-28 (RF-REP-05) Un rango sin datos devuelve vacío.**
Dado un rango donde no hay registros, cuando se consulta cualquier reporte, entonces responde 200 con una lista vacía, no un error.

## Lo que no cambia

**C-29** `dotnet test` pasa completo, incluidas las 64 pruebas que ya existían.
**C-30** Los seis frentes de la revisión acumulada siguen funcionando con `docker compose up --build`.
**C-31 (RD-03)** Nada en `src/Despacho.Core` usa `Despacho.Negocio`.

## Criterios agregados después de la auditoría (Asignación 3)

**C-32 (RF-NOT-12, Hallazgo 1) Dos procesadores al mismo tiempo no duplican.**
Dado un correo `Pendiente` en la cola, cuando dos procesadores corren a la vez (por ejemplo, el servicio `enviador` y el comando manual del README), entonces el correo se envía una sola vez y queda `Enviado` con 1 intento.

**C-33 (RF-REP-02, RF-REP-05, Hallazgo 2) El reporte de repartidores cuenta por la fecha de entrega.**
Dado un envío creado el 1 de noviembre y entregado el 10, cuando A1 consulta `GET /api/reportes/repartidores?desde=2026-11-10&hasta=2026-11-10`, entonces el envío aparece contado para su repartidor; y con `desde=2026-11-01&hasta=2026-11-01` no aparece.

**C-34 (RF-NOT-08, RD-07, Hallazgo 3) Si la notificación falla, la operación responde éxito.**
Dado que guardar la notificación falla, cuando R acepta o entrega un envío, o A1 crea, aprueba o rechaza una solicitud, entonces la operación responde éxito, queda guardada y la falla aparece como advertencia en el registro de la Api, sin datos sensibles.
