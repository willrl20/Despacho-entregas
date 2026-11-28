import type { EstadoEnvio } from '../api/tipos'
import '../estilos/linea-estado.css'
import { nombreEstado } from '../utilidades/formato'

const PASOS: EstadoEnvio[] = ['Pendiente', 'Asignado', 'EnRuta', 'Entregado']

export function LineaEstado({ estado }: { estado: EstadoEnvio }) {
  if (estado === 'Cancelado') {
    return (
      <p className="linea-cancelado">
        <span className="guia__estado guia__estado--cancelado">Cancelado</span>
        <span>Este envío fue cancelado.</span>
      </p>
    )
  }

  const actual = PASOS.indexOf(estado)

  return (
    <ol className="linea" aria-label="Progreso del envío">
      {PASOS.map((paso, indice) => {
        const situacion = indice < actual ? 'hecho' : indice === actual ? 'actual' : 'siguiente'
        return (
          <li key={paso} className={`linea__paso linea__paso--${situacion}`} aria-current={situacion === 'actual' ? 'step' : undefined}>
            <span className="linea__numero">{indice + 1}</span>
            <span className="linea__texto">
              {nombreEstado(paso)}
              {situacion === 'actual' && <span className="linea__aqui">Aquí va</span>}
            </span>
          </li>
        )
      })}
    </ol>
  )
}
