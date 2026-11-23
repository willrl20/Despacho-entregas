import { createContext } from 'react'
import type { Usuario } from '../api/tipos'

export interface ValorSesion {
  usuario: Usuario | null
  cargando: boolean
  iniciarSesion: (correo: string, contrasena: string) => Promise<void>
  cerrarSesion: () => Promise<void>
}

export const ContextoSesion = createContext<ValorSesion | null>(null)
