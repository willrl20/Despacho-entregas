import { llamarApi } from './cliente'
import type { RespuestaMensaje } from './tipos'

export function registrarCuenta(correo: string, contrasena: string): Promise<RespuestaMensaje> {
  return llamarApi<RespuestaMensaje>('/api/cuentas/registro', {
    metodo: 'POST',
    cuerpo: { correo, contrasena },
  })
}

export function activarCuenta(token: string): Promise<RespuestaMensaje> {
  return llamarApi<RespuestaMensaje>(`/api/cuentas/activar?token=${encodeURIComponent(token)}`)
}

export function reenviarActivacion(correo: string): Promise<RespuestaMensaje> {
  return llamarApi<RespuestaMensaje>('/api/cuentas/reenviar-activacion', {
    metodo: 'POST',
    cuerpo: { correo },
  })
}
