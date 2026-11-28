import type { EstadoEnvio, TamanoPaquete } from '../api/tipos'
import '../estilos/guia-envio.css'
import {
  formatearDistancia,
  formatearPrecio,
  nombreEstado,
  nombreTamano,
  numeroGuia,
} from '../utilidades/formato'

interface PropiedadesGuiaEnvio {
  id: number
  estado: EstadoEnvio
  tamano: TamanoPaquete
  distanciaKm: number
  costo: number
  descripcion?: string
}

export function GuiaEnvio({ id, estado, tamano, distanciaKm, costo, descripcion }: PropiedadesGuiaEnvio) {
  return (
    <article className="guia">
      <div className="guia__arriba">
        <p className="guia__numero">{numeroGuia(id)}</p>
        <p className={`guia__estado guia__estado--${estado.toLowerCase()}`}>{nombreEstado(estado)}</p>
      </div>
      {descripcion && <p className="guia__descripcion">{descripcion}</p>}
      <div className="guia__perforacion" aria-hidden="true" />
      <dl className="guia__datos">
        <div className="guia__dato">
          <dt>Tamaño</dt>
          <dd>{nombreTamano(tamano)}</dd>
        </div>
        <div className="guia__dato">
          <dt>Distancia</dt>
          <dd className="guia__cifra">{formatearDistancia(distanciaKm)}</dd>
        </div>
        <div className="guia__dato">
          <dt>Precio</dt>
          <dd className="guia__cifra">{formatearPrecio(costo)}</dd>
        </div>
      </dl>
    </article>
  )
}
