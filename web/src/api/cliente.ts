const CLAVE_TOKEN = 'dashgo.token'

export const MENSAJE_RED = 'No se pudo conectar con el servidor. Revisa tu conexión e inténtalo de nuevo.'
export const MENSAJE_GENERICO = 'Ocurrió un error inesperado. Inténtalo de nuevo.'

const LARGO_MAXIMO_MENSAJE = 300

export class ErrorApi extends Error {
  estado: number

  constructor(mensaje: string, estado: number) {
    super(mensaje)
    this.name = 'ErrorApi'
    this.estado = estado
  }
}

let manejadorSesionInvalida: (() => void) | null = null

export function registrarSesionInvalida(manejador: (() => void) | null): void {
  manejadorSesionInvalida = manejador
}

export function obtenerToken(): string | null {
  return sessionStorage.getItem(CLAVE_TOKEN)
}

export function guardarToken(token: string): void {
  sessionStorage.setItem(CLAVE_TOKEN, token)
}

export function borrarToken(): void {
  sessionStorage.removeItem(CLAVE_TOKEN)
}

export function mensajeDeError(error: unknown): string {
  return error instanceof ErrorApi ? error.message : MENSAJE_GENERICO
}

interface OpcionesLlamada {
  metodo?: 'GET' | 'POST' | 'PUT' | 'DELETE'
  cuerpo?: unknown
}

async function leerJson(respuesta: Response): Promise<unknown> {
  const texto = await respuesta.text()
  if (texto === '') return null
  try {
    return JSON.parse(texto)
  } catch {
    return null
  }
}

function extraerError(datos: unknown): string | null {
  if (typeof datos !== 'object' || datos === null || !('error' in datos)) return null
  const error = (datos as { error: unknown }).error
  if (typeof error !== 'string' || error.trim() === '' || error.length > LARGO_MAXIMO_MENSAJE) return null
  return error
}

export async function llamarApi<T>(ruta: string, opciones: OpcionesLlamada = {}): Promise<T> {
  const token = obtenerToken()
  const encabezados: Record<string, string> = { Accept: 'application/json' }

  if (opciones.cuerpo !== undefined) encabezados['Content-Type'] = 'application/json'
  if (token) encabezados.Authorization = `Bearer ${token}`

  let respuesta: Response
  try {
    respuesta = await fetch(ruta, {
      method: opciones.metodo ?? 'GET',
      headers: encabezados,
      body: opciones.cuerpo !== undefined ? JSON.stringify(opciones.cuerpo) : undefined,
    })
  } catch {
    throw new ErrorApi(MENSAJE_RED, 0)
  }

  const datos = await leerJson(respuesta)

  if (respuesta.status === 401 && token) {
    borrarToken()
    manejadorSesionInvalida?.()
  }

  if (!respuesta.ok) {
    throw new ErrorApi(extraerError(datos) ?? MENSAJE_GENERICO, respuesta.status)
  }

  return datos as T
}
