# Máquina de estados del Pedido

Los estados están en EstadoPedido y las transiciones en MaquinaEstadosPedido (src/Despacho.Negocio/Pedidos). El Pedido solo cambia de estado con CambiarEstado, que consulta la máquina.

## Estados

- Pendiente
- Asignado
- EnRuta
- Entregado (terminal)
- Cancelado (terminal)

## Transiciones permitidas

| Desde | Hacia | Quién | Condición |
|-------|-------|-------|-----------|
| Pendiente | Asignado | Administrador | Se asigna un repartidor al pedido |
| Pendiente | Cancelado | Administrador o tienda | El pedido todavía no tiene repartidor |
| Asignado | EnRuta | Repartidor asignado | El repartidor sale a entregar; se avisa al destinatario (RF-NOT-03) |
| Asignado | Cancelado | Administrador o tienda | El pedido todavía no ha salido |
| EnRuta | Entregado | Repartidor asignado | El repartidor confirma la entrega |

## Transiciones prohibidas

Entregado → EnRuta está prohibida de forma explícita. Entregado y Cancelado son terminales: no salen a ningún otro estado. Cualquier transición que no esté en la tabla se rechaza.
