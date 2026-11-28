import { llamarApi } from './cliente'

export async function tienePermisoRepartidor(): Promise<boolean> {
  try {
    await llamarApi('/api/repartidor/envios-disponibles')
    return true
  } catch {
    return false
  }
}
