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

export type EstadoEnvio = 'Pendiente' | 'Asignado' | 'EnRuta' | 'Entregado' | 'Cancelado'

export type TamanoPaquete = 'Pequeno' | 'Mediano' | 'Grande'

export interface Envio {
  id: number
  modalidad: string
  estado: EstadoEnvio
  descripcionPaquete: string
  tamano: TamanoPaquete
  distanciaKm: number
  costo: number
  repartidorId: number | null
  creadoEnUtc: string
}
