import { Navigate } from 'react-router-dom'
import { useContextoInterno } from '../componentes/contextoInterno'
import { MuyPronto } from '../componentes/MuyPronto'

export function Repartir() {
  const { puedeRepartir, cargandoPermisos } = useContextoInterno()

  if (cargandoPermisos) {
    return (
      <p role="status" className="tarjeta__apoyo">
        Cargando...
      </p>
    )
  }

  if (!puedeRepartir) {
    return <Navigate to="/" replace />
  }

  return <MuyPronto titulo="Repartir" apoyo="Pronto verás aquí los envíos disponibles para llevar." />
}
