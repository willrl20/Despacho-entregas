# Despacho de Entregas a Domicilio

Sistema de gestión de entregas para un servicio de reparto que opera
para varios comercios. Registra pedidos, los asigna a repartidores y
sigue el estado de cada entrega hasta su cierre.

Proyecto de **Programación III (TDS-007)** · ITLA · 2026-C-3

---

## Tecnologías

- **Lenguaje:** C# / .NET 8
- **Base de datos:** SQL Server
- **ORM:** Entity Framework Core
- **Pruebas:** xUnit
- **Contenedores:** Docker (desde la semana 9)

---

## Arquitectura

El proyecto tiene dos partes. El **Core** es la especificación fija del
curso, igual para los 25 proyectos. El **módulo de negocio** es el
dominio propio: el despacho de entregas.

La dependencia va en un solo sentido: el módulo de negocio consume el
Core, y el Core no conoce el módulo de negocio (RD-03).

### Diagrama de componentes

```mermaid
flowchart TB
  subgraph CORE["CORE — especificación fija del curso"]
    direction TB
    ACC["<b>Control de acceso</b><br/><i>Autentica usuarios y les asigna un rol</i>"]
    PERM["<b>Gestión de permisos</b><br/><i>Resuelve solicitudes de acceso elevado</i>"]
    DOC["<b>Manejador de documentos</b><br/><i>Guarda, lista y da de baja archivos</i>"]
    NOTI["<b>Notificaciones</b><br/><i>Entrega avisos internos por evento</i>"]
    REP["<b>Reportes</b><br/><i>Agrega datos y los filtra por rol</i>"]
    AUD["<b>Auditoría</b><br/><i>Conserva el rastro de cada acción</i>"]
  end

  subgraph NEGOCIO["MÓDULO DE NEGOCIO"]
    direction TB
    DESP["<b>Despacho de entregas</b><br/><i>Registra pedidos y sigue su estado</i>"]
  end

  DESP -->|quién es y qué rol tiene| ACC
  PERM -->|quién es y qué rol tiene| ACC
  DOC  -->|quién es y qué rol tiene| ACC
  REP  -->|quién es y qué rol tiene| ACC

  DESP -->|avisa que el pedido va en ruta| NOTI
  PERM -->|avisa la resolución al solicitante| NOTI

  DESP -->|aporta los datos del reporte del dominio| REP

  DESP -.->|registra cada cambio de estado| AUD
  ACC  -.->|registra cambios de rol| AUD
  PERM -.->|registra la resolución| AUD
  DOC  -.->|registra subida y eliminación| AUD
```

Todas las piezas se construyen en C#. Las flechas punteadas van hacia
Auditoría: casi toda operación relevante deja rastro, y se dibujan
punteadas para no saturar el diagrama.

**Ninguna flecha sale del Core hacia el módulo de negocio.** El Core no
sabe que existen los pedidos (RD-03).

---

## Módulo de negocio

### Entidades

| Entidad | Responsabilidad |
|---|---|
| `Tienda` | Comercio que despacha pedidos |
| `Destinatario` | Persona que recibe el pedido |
| `Pedido` | Solicitud de entrega. Contiene la máquina de estados |
| `LineaPedido` | Cada artículo dentro de un pedido |
| `Repartidor` | Persona asignada para entregar |

### Relaciones

```mermaid
erDiagram
    TIENDA       ||--o{ PEDIDO       : despacha
    DESTINATARIO ||--o{ PEDIDO       : recibe
    REPARTIDOR   ||--o{ PEDIDO       : entrega
    PEDIDO       ||--|{ LINEAPEDIDO  : contiene
```

### Máquina de estados del Pedido

```mermaid
stateDiagram-v2
    [*] --> Pendiente
    Pendiente --> Asignado  : se asigna un repartidor
    Asignado  --> EnRuta    : el repartidor recoge el pedido
    EnRuta    --> Entregado : el destinatario recibe el pedido
    Pendiente --> Cancelado : cancelación antes de asignar
    Asignado  --> Cancelado : cancelación antes de salir
    Entregado --> [*]
    Cancelado --> [*]
```

- **Estado inicial:** `Pendiente`
- **Estados terminales:** `Entregado` y `Cancelado`. Ninguna transición
  parte de ellos (RF-NEG-05)
- **Transición prohibida explícita:** `Entregado → EnRuta`. El intento
  se rechaza y el estado no cambia (RF-NEG-04)
- Las transiciones se resuelven en un único punto del código (RD-04)

### Funcionalidades

1. Registrar un pedido con sus líneas
2. Asignar un repartidor a un pedido pendiente
3. Marcar un pedido como en ruta
4. Confirmar la entrega
5. Consultar el historial de pedidos por destinatario

---

## Cómo ejecutar el proyecto

> Pendiente. Se completa cuando exista código ejecutable (Práctica 1).

---

## Estado del proyecto

| Pieza | Semanas | Estado |
|---|---|---|
| Diseño de componentes | 2 | Hecho |
| Control de acceso | 2–4 | En curso |
| Gestión de permisos | 6–8 | Pendiente |
| Manejador de documentos | 9 | Pendiente |
| Notificaciones | 11–12 | Pendiente |
| Reportes | 12 | Pendiente |
| Auditoría | 14 | Pendiente |