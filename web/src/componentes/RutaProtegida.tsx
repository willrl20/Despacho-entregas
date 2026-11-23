import { Navigate, Outlet } from 'react-router-dom'
import { useSesion } from '../contexto/useSesion'

export function RutaProtegida() {
  const { usuario, cargando } = useSesion()

  if (cargando) {
    return (
      <p className="cargando-pagina" role="status">
        Cargando...
      </p>
    )
  }

  if (!usuario) {
    return <Navigate to="/iniciar-sesion" replace />
  }

  return <Outlet />
}
