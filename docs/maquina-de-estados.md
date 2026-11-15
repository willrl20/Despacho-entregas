# Máquina de estados del Envío

Los estados están en `EstadoEnvio` y las transiciones en `MaquinaEstadosEnvio` (`src/Despacho.Negocio/Envios`). El envío solo cambia de estado con `Envio.CambiarEstado`, que consulta la máquina. Quién puede ejecutar cada transición lo verifica `ServicioEnvios`. Esta máquina es independiente de la de Gestión de permisos (RF-NEG-09).

## Estados

- Pendiente: el cliente creó el envío y espera repartidor.
- Asignado: un repartidor lo aceptó y va a recoger el paquete.
- EnRuta: el repartidor recogió el paquete y va hacia la entrega.
- Entregado (terminal)
- Cancelado (terminal)

## Transiciones permitidas

| Desde | Hacia | Quién | Condición |
|---|---|---|---|
| Pendiente | Cancelado | Cliente del envío o Administrador | Solo el cliente que creó el envío o un Administrador |
| Asignado | Cancelado | Cliente del envío o Administrador | Solo el cliente que creó el envío o un Administrador |
| Pendiente | Asignado | Repartidor | Tiene el permiso Repartidor aprobado en Gestión de permisos y no es su propio envío |
| Asignado | EnRuta | Repartidor asignado | Es el repartidor de ese envío y recogió el paquete |
| EnRuta | Entregado | Repartidor asignado | Es el repartidor de ese envío y entregó el paquete |

## Transiciones prohibidas

| Desde | Hacia | Motivo |
|---|---|---|
| Pendiente | EnRuta | Primero un repartidor tiene que aceptar el envío |
| Entregado | EnRuta | Prohibida de forma explícita en `MaquinaEstadosEnvio` |
| Pendiente | Entregado | No se puede entregar sin repartidor ni recogida |
| Asignado | Entregado | Primero hay que recoger el paquete |
| EnRuta | Cancelado | Un paquete en camino ya no se cancela |
| Cancelado | Pendiente | Cancelado es terminal |

Cualquier otra transición que no esté en la tabla de permitidas también se rechaza y el estado no cambia.

## Estados terminales

Entregado y Cancelado son terminales: ninguna transición parte de ellos.
