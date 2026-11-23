import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { borrarToken, guardarToken, obtenerToken, registrarSesionInvalida } from '../api/cliente'
import { cerrarSesionApi, iniciarSesionApi, obtenerUsuarioActual } from '../api/sesion'
import type { Usuario } from '../api/tipos'
import { ContextoSesion, type ValorSesion } from './ContextoSesion'

export function ProveedorSesion({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(null)
  const [cargando, setCargando] = useState<boolean>(() => obtenerToken() !== null)

  useEffect(() => {
    registrarSesionInvalida(() => setUsuario(null))
    return () => registrarSesionInvalida(null)
  }, [])

  useEffect(() => {
    if (obtenerToken() === null) return

    let activo = true
    obtenerUsuarioActual()
      .then((actual) => {
        if (activo) setUsuario(actual)
      })
      .catch(() => undefined)
      .finally(() => {
        if (activo) setCargando(false)
      })

    return () => {
      activo = false
    }
  }, [])

  const iniciarSesion = useCallback(async (correo: string, contrasena: string) => {
    borrarToken()
    const { token } = await iniciarSesionApi(correo, contrasena)
    guardarToken(token)
    try {
      setUsuario(await obtenerUsuarioActual())
    } catch (error) {
      borrarToken()
      throw error
    }
  }, [])

  const cerrarSesion = useCallback(async () => {
    await cerrarSesionApi().catch(() => undefined)
    borrarToken()
    setUsuario(null)
  }, [])

  const valor = useMemo<ValorSesion>(
    () => ({ usuario, cargando, iniciarSesion, cerrarSesion }),
    [usuario, cargando, iniciarSesion, cerrarSesion],
  )

  return <ContextoSesion.Provider value={valor}>{children}</ContextoSesion.Provider>
}
