import { useOutletContext } from 'react-router-dom'
import type { EstadoPermisoRepartidor } from '../contexto/usePermisoRepartidor'

export type ContextoInterno = EstadoPermisoRepartidor

export function useContextoInterno(): ContextoInterno {
  return useOutletContext<ContextoInterno>()
}
