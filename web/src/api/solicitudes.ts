import { llamarApi } from './cliente'
import type { RespuestaSolicitud } from './tipos'

export function crearSolicitudPermiso(permiso: 'Repartidor' | 'Administrador'): Promise<RespuestaSolicitud> {
  return llamarApi<RespuestaSolicitud>('/api/solicitudes', {
    metodo: 'POST',
    cuerpo: { permiso },
  })
}
