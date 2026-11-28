import type { EstadoEnvio, TamanoPaquete } from '../api/tipos'

const NOMBRES_ESTADO: Record<EstadoEnvio, string> = {
  Pendiente: 'Pendiente',
  Asignado: 'Asignado',
  EnRuta: 'En ruta',
  Entregado: 'Entregado',
  Cancelado: 'Cancelado',
}

const NOMBRES_TAMANO: Record<TamanoPaquete, string> = {
  Pequeno: 'Pequeño',
  Mediano: 'Mediano',
  Grande: 'Grande',
}

export function numeroGuia(id: number): string {
  return `DG-${String(id).padStart(4, '0')}`
}

export function nombreEstado(estado: EstadoEnvio): string {
  return NOMBRES_ESTADO[estado] ?? estado
}

export function nombreTamano(tamano: TamanoPaquete): string {
  return NOMBRES_TAMANO[tamano] ?? tamano
}

export function formatearPrecio(costo: number): string {
  const numero = new Intl.NumberFormat('es-DO', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(costo)
  return `RD$ ${numero}`
}

export function formatearDistancia(km: number): string {
  return `${new Intl.NumberFormat('es-DO', { maximumFractionDigits: 1 }).format(km)} km`
}

export function formatearFecha(fechaIso: string): string {
  const fecha = new Date(fechaIso)
  if (Number.isNaN(fecha.getTime())) return ''
  return new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium' }).format(fecha)
}
