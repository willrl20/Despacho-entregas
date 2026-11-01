# Máquina de estados del Pedido

Los estados están en `EstadoPedido` y las transiciones en `MaquinaEstadosPedido` (`src/Despacho.Negocio/Pedidos`). El pedido solo cambia de estado con `Pedido.CambiarEstado`, que consulta la máquina. Quién puede ejecutar cada transición lo verifica `ServicioPedidos`. Esta máquina es independiente de la de Gestión de permisos (RF-NEG-09).

## Estados

- Pendiente
- Asignado
- EnRuta
- Entregado (terminal)
- Cancelado (terminal)

## Transiciones permitidas

| Desde | Hacia | Quién | Condición |
|---|---|---|---|
| Pendiente | Cancelado | Cliente del pedido o Administrador | Solo el cliente que hizo el pedido o un Administrador |
| Asignado | Cancelado | Cliente del pedido o Administrador | Solo el cliente que hizo el pedido o un Administrador |
| Pendiente | Asignado | Repartidor | Tiene el permiso Repartidor aprobado en Gestión de permisos y no es su propio pedido |
| Asignado | EnRuta | Repartidor asignado | Es el repartidor de ese pedido |
| EnRuta | Entregado | Repartidor asignado | Es el repartidor de ese pedido |

## Transiciones prohibidas

| Desde | Hacia | Motivo |
|---|---|---|
| Pendiente | EnRuta | Primero hay que aceptar el pedido |
| Entregado | EnRuta | Prohibida de forma explícita en `MaquinaEstadosPedido` |
| Pendiente | Entregado | No se puede entregar sin repartidor ni ruta |
| Asignado | Entregado | Primero tiene que salir en ruta |
| EnRuta | Cancelado | Un pedido en camino ya no se cancela |
| Cancelado | Pendiente | Cancelado es terminal |

Cualquier otra transición que no esté en la tabla de permitidas también se rechaza y el estado no cambia.

## Estados terminales

Entregado y Cancelado son terminales: ninguna transición parte de ellos.
