export type EleccionUso = 'cliente' | 'repartidor'

const CLAVE_USO = 'dashgo.uso'

export function guardarEleccionUso(uso: EleccionUso): void {
  localStorage.setItem(CLAVE_USO, uso)
}

export function leerEleccionUso(): EleccionUso | null {
  const valor = localStorage.getItem(CLAVE_USO)
  return valor === 'cliente' || valor === 'repartidor' ? valor : null
}
