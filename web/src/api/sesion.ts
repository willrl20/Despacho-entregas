import { llamarApi } from './cliente'
import type { RespuestaMensaje, RespuestaToken, Usuario } from './tipos'

export function iniciarSesionApi(correo: string, contrasena: string): Promise<RespuestaToken> {
  return llamarApi<RespuestaToken>('/api/sesion/iniciar', {
    metodo: 'POST',
    cuerpo: { correo, contrasena },
  })
}

export function obtenerUsuarioActual(): Promise<Usuario> {
  return llamarApi<Usuario>('/api/sesion/yo')
}

export function cerrarSesionApi(): Promise<RespuestaMensaje> {
  return llamarApi<RespuestaMensaje>('/api/sesion/cerrar', { metodo: 'POST' })
}
