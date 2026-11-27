Instrucciones para el agente
Qué es este proyecto

Despacho de Entregas: una aplicación para enviar paquetes de un punto a otro. Un cliente crea un envío (elige si envía o recibe el paquete), un repartidor con permiso aprobado lo acepta, lo recoge y lo entrega.

El proyecto tiene dos partes:

Core (src/Despacho.Core): usuarios, sesiones, permisos, documentos y correos.
Negocio (src/Despacho.Negocio): los envíos, su máquina de estados y el precio.

La Api (src/Despacho.Api) solo recibe las peticiones y llama a los servicios. El programa src/Despacho.EnviadorCorreos envía los correos de la cola.

Lenguaje y herramientas
C# con .NET 8 y ASP.NET Core (Minimal APIs).
Entity Framework Core 8.0.11 con SQL Server. Hay dos contextos: CoreDbContext y NegocioDbContext.
Pruebas con xUnit y la base de datos en memoria de EF Core (Microsoft.EntityFrameworkCore.InMemory 8.0.11).
Docker y Docker Compose.
Cómo correrlo
Copy-Item .env.example .env
docker compose up --build

La Api responde en http://localhost:8080 (Swagger en http://localhost:8080/swagger).

Cómo probarlo
dotnet test

Desde la raíz del repositorio. Corre las pruebas del Core y del negocio. Todas deben pasar antes de dar un trabajo por terminado.

Convenciones de código
Todo el código, los nombres y los mensajes van en español, sin tildes en los nombres de clases, métodos y variables (por ejemplo Notificacion, ServicioNotificaciones).
Namespaces con punto y coma (namespace Despacho.Core.Notificaciones;), uno por carpeta.
La lógica va en los servicios, nunca en los endpoints (RD-02). Los endpoints solo leen la petición, llaman al servicio y traducen el resultado.
Cada servicio tiene su interfaz (IServicioX) y se registra en src/Despacho.Api/Program.cs.
Los servicios devuelven Resultado o Resultado<T> (Despacho.Core.Comun) y no lanzan excepciones por errores esperados (RD-07).
Los mensajes de error van en constantes públicas del servicio y nunca muestran trazas, rutas ni consultas (RD-08).
Las fechas se guardan siempre en UTC con IReloj.AhoraUtc (RD-11). Nunca uses DateTime.Now.
Cada endpoint declara su autorización con RequiereRol(...) o RequiereSesion() de src/Despacho.Api/Seguridad/FiltroRol.cs (RD-06). El usuario se lee de http.Items[FiltroRol.ClaveUsuario] como UsuarioActual.
Las pruebas tienen las fases // Arrange, // Act y // Assert, verifican un solo comportamiento, crean su propia base en memoria con Guid.NewGuid().ToString() y se llaman Metodo_Situacion_ResultadoEsperado. Encima de cada prueba va un comentario con el requisito (por ejemplo // RF-NOT-01).
Sin comentarios en el código de producción.
Los cambios de base de datos se hacen con migraciones de EF Core en Datos/Migraciones del proyecto que corresponde.
Restricciones
Nunca leas, muestres ni modifiques el archivo .env. Si necesitas saber qué variables existen, mira .env.example.
Nunca escribas credenciales, contraseñas, claves de API ni tokens en el código, en docker-compose.yml, en el Dockerfile ni en ningún archivo versionado. Todo secreto se lee de variables de entorno (RD-10).
El Core no puede depender del negocio (RD-03): nada en src/Despacho.Core puede usar Despacho.Negocio. El negocio sí puede usar el Core.
No modifiques la máquina de estados del envío (MaquinaEstadosEnvio) ni la de las solicitudes (TransicionesSolicitud).
No modifiques ni borres pruebas que ya existen para que pasen. Si una prueba existente falla, avisa.
No toques las migraciones que ya existen; crea migraciones nuevas.
Trabaja solo dentro de este repositorio. No accedas a carpetas fuera de él.
No hagas commit, push, tag ni cambies de rama. No instales paquetes ni ejecutes comandos salvo que se te pida.
Al terminar, lista los archivos que creaste, modificaste y eliminaste.
Reglas agregadas después de la auditoría (Asignación 3)
Todo proceso que pueda correr en dos copias a la vez, como el procesador de la cola, reserva cada registro y guarda la reserva antes de trabajarlo. Si otra copia ya lo reservó, lo salta. Siempre lleva una prueba con dos contextos sobre la misma base (Hallazgo 1).
Un reporte filtra por la fecha del hecho que cuenta: si cuenta envíos entregados, usa EntregadoEnUtc, no CreadoEnUtc. Si la entidad no guarda esa fecha, agrégala con una migración nueva y pregunta antes de usar otra fecha en su lugar (Hallazgo 2).
Las notificaciones se crean con AvisoSeguro.EjecutarAsync después de guardar la operación. Si la notificación falla, se registra como advertencia y la operación responde éxito. Nunca ignores el Resultado que devuelve un servicio (Hallazgo 3).
Toda funcionalidad nueva incluye pruebas de dos casos que antes no pedía: dos peticiones al mismo tiempo y una dependencia que falla (otra base, el correo o una notificación).
