export type RolUsuario = 'Estandar' | 'Administrador'

export interface Usuario {
  id: number
  correo: string
  rol: RolUsuario
}

export interface RespuestaMensaje {
  mensaje: string
}

export interface RespuestaToken {
  token: string
}

export interface RespuestaSolicitud {
  id: number
  mensaje: string
}
