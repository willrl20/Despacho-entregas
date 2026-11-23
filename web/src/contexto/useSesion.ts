import { useContext } from 'react'
import { ContextoSesion, type ValorSesion } from './ContextoSesion'

export function useSesion(): ValorSesion {
  const valor = useContext(ContextoSesion)
  if (valor === null) {
    throw new Error('useSesion debe usarse dentro de ProveedorSesion.')
  }
  return valor
}
