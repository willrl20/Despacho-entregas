import { useEffect, useState } from 'react'
import { tienePermisoRepartidor } from '../api/repartidor'

export interface EstadoPermisoRepartidor {
  puedeRepartir: boolean
  cargandoPermisos: boolean
}

export function usePermisoRepartidor(idUsuario: number | null): EstadoPermisoRepartidor {
  const [estado, setEstado] = useState<EstadoPermisoRepartidor>({ puedeRepartir: false, cargandoPermisos: true })

  useEffect(() => {
    if (idUsuario === null) return

    let activo = true
    tienePermisoRepartidor().then((puede) => {
      if (activo) setEstado({ puedeRepartir: puede, cargandoPermisos: false })
    })

    return () => {
      activo = false
    }
  }, [idUsuario])

  return estado
}
