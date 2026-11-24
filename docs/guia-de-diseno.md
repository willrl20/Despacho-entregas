# Guía de diseño de DashGo

DashGo es la interfaz web de Despacho de Entregas: un cliente pide que un repartidor recoja un paquete en un punto y lo lleve a otro, en República Dominicana. Esta guía define cómo se ve y cómo habla la aplicación. Toda pantalla nueva la respeta.

## Idea visual

- **El elemento memorable es la guía de envío.** Cada envío se muestra como la etiqueta de un paquete: el número grande (por ejemplo `DG-1042`), la recogida y la entrega unidas por una línea de ruta punteada, el estado y el precio.
- **Lenguaje de calle y carretera:** letra de señalización (Barlow), líneas de ruta, sensación de movimiento.
- **Todo lo demás, tranquilo:** pocas decoraciones, mucho aire, un solo botón principal por pantalla.

## Logo

- **Caja en marcha:** una caja inclinada color mango con su cinta, y tres líneas de velocidad color menta.
- `logo/dashgo-simbolo.svg`: el símbolo solo, para usar sobre el fondo petróleo.
- `logo/dashgo-icono.svg`: el ícono de la app y el favicon (fondo mango, caja petróleo).
- Al lado del símbolo, la palabra **DashGo** en Barlow Condensed 700 inclinada: "Dash" en color texto y "Go" en mango.
- No se deforma, no se le cambian los colores y siempre tiene aire alrededor (como mínimo, la mitad de su alto).

## Colores (modo oscuro, el principal)

| Token | Valor | Uso |
|---|---|---|
| `--color-fondo` | `#0D3B40` | Fondo de la aplicación (petróleo) |
| `--color-superficie` | `#12505A` | Tarjetas, guías de envío, campos |
| `--color-superficie-alta` | `#176470` | Elementos sobre una tarjeta, estado al pasar el ratón |
| `--color-principal` | `#FF9F1C` | Botón principal, "Go" del logo, línea de ruta, lo más importante (mango) |
| `--color-principal-texto` | `#0D3B40` | Texto sobre el color principal |
| `--color-secundario` | `#7FE0C9` | Estados, foco del teclado, detalles (menta) |
| `--color-texto` | `#EAF6F6` | Texto principal |
| `--color-texto-suave` | `#9CC5C7` | Texto de apoyo |
| `--color-borde` | `#1E6670` | Bordes y separadores |
| `--color-exito` | `#5BD68A` | Entregado, acciones que salieron bien |
| `--color-error` | `#FF7A7A` | Errores, cancelado |

Reglas:
- Solo **un** botón mango por pantalla; los demás son secundarios (contorno menta o texto).
- El color nunca es la única señal: los estados llevan siempre su palabra (por ejemplo, "En ruta").
- Contraste mínimo AA (4.5:1 para texto normal).
- Más adelante habrá modo claro, derivado de estos mismos colores (`prefers-color-scheme`).

## Letra

- **Barlow** (400, 500 y 600) para todo el texto.
- **Barlow Condensed** (600 y 700) para números: guías (`DG-1042`), montos (`RD$ 172.50`), distancias y placas.
- Escala: 14, 16 (texto normal), 20, 25 y 31 px. Interlineado 1.5 en texto.
- Mayúscula solo al inicio de la frase ("Crear envío", no "Crear Envío"). Nada de etiquetas en MAYÚSCULAS.

## Forma y espacio

- Esquinas: 10 px en botones y campos, 14 px en tarjetas, completamente redondas en las pastillas de estado.
- Espaciado en pasos de 4 px (4, 8, 12, 16, 24, 32).
- Zonas táctiles de 44 px como mínimo.
- Pensada primero para celular: barra de navegación **abajo** en celular y **a un lado** en pantallas anchas.

## Componentes base

- **Guía de envío:** número, estado, recogida → entrega con línea punteada mango, tamaño, distancia y precio.
- **Línea de estado:** Pendiente → Asignado → En ruta → Entregado (y Cancelado aparte). Es una secuencia real, por eso va numerada.
- **Crear envío por pasos:** ¿Enviar o recibir? → Puntos en el mapa → El paquete → Precio y confirmar.
- **Selector del color del vehículo:** una fila de motores dibujados (un SVG que se pinta con el color), con el elegido marcado en mango.
- **Foto de perfil del repartidor:** obligatoria al registrarse. Se elige de la galería o se toma con la cámara del celular, se recorta en círculo y se muestra redonda.
- **Tarjeta "Tu repartidor":** foto de perfil, nombre, vehículo, color, placa y el dibujo del vehículo en su color.
- **Mensajes:** error en rojo suave con lo que hay que hacer; éxito en verde; vacío con una invitación a actuar.

## Movimiento

- Un solo momento con animación: la caja del logo "arranca" al confirmar un envío.
- El repartidor de las pantallas de entrada también se mueve (ruedas, calle y líneas de velocidad) y se detiene con `prefers-reduced-motion`.
- Entrada animada de las pantallas de entrada: primero se ven solo el logo y el repartidor, centrados; tras 1.2 segundos se mueven a su lugar y aparece el formulario. Reglas: dura menos de 2 segundos en total, sale solo la primera vez por pestaña (`sessionStorage`, clave `dashgo.intro`), se salta con un clic o una tecla, y no aparece con `prefers-reduced-motion`. Mientras dura, el formulario no recibe el foco del teclado.
- Lo demás, sin animaciones decorativas. Se respeta `prefers-reduced-motion`.

## Cómo habla DashGo

- Tutea, en español claro y cercano.
- Los botones empiezan con un verbo: "Crear envío", "Aceptar envío", "Marcar recogido".
- La acción conserva su nombre: el botón "Aceptar envío" produce el aviso "Envío aceptado".
- Los errores dicen qué pasó y qué hacer, sin disculparse: "Esa contraseña no es correcta. Revísala o recupérala."
- Nada de "por favor", "exitosamente" ni signos de exclamación en mensajes del sistema.

## Accesibilidad

- Cada campo con su etiqueta visible; tipos correctos (`email`, `password`, `tel`).
- Foco del teclado siempre visible (contorno menta).
- Los mensajes de error se anuncian a los lectores de pantalla.
- Imágenes y dibujos con texto alternativo; los decorativos, ocultos para el lector.
