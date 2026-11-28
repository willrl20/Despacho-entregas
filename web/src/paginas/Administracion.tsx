import { Navigate } from 'react-router-dom'
import { MuyPronto } from '../componentes/MuyPronto'
import { useSesion } from '../contexto/useSesion'

export function Administracion() {
  const { usuario } = useSesion()

  if (usuario?.rol !== 'Administrador') {
    return <Navigate to="/" replace />
  }

  return <MuyPronto titulo="Administración" apoyo="Pronto podrás gestionar usuarios y solicitudes desde aquí." />
}
