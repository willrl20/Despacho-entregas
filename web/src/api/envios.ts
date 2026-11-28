import { llamarApi } from './cliente'
import type { Envio, RespuestaMensaje } from './tipos'

export function listarMisEnvios(): Promise<Envio[]> {
  return llamarApi<Envio[]>('/api/envios/mios')
}

export function cancelarEnvio(id: number): Promise<RespuestaMensaje> {
  return llamarApi<RespuestaMensaje>(`/api/envios/${id}/cancelar`, { metodo: 'POST' })
}
