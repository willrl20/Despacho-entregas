import { llamarApi } from './cliente'
import type { RespuestaMensaje } from './tipos'

export function recuperarContrasena(correo: string): Promise<RespuestaMensaje> {
  return llamarApi<RespuestaMensaje>('/api/contrasenas/recuperar', {
    metodo: 'POST',
    cuerpo: { correo },
  })
}

export function restablecerContrasena(correo: string, codigo: string, contrasenaNueva: string): Promise<RespuestaMensaje> {
  return llamarApi<RespuestaMensaje>('/api/contrasenas/restablecer', {
    metodo: 'POST',
    cuerpo: { correo, codigo, contrasenaNueva },
  })
}
