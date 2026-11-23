import { useEffect, useRef, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { mensajeDeError } from '../api/cliente'
import { activarCuenta } from '../api/cuentas'
import type { RespuestaMensaje } from '../api/tipos'
import { Mensaje } from '../componentes/Mensaje'
import { PantallaAcceso } from '../componentes/PantallaAcceso'

const MENSAJE_SIN_TOKEN = 'El enlace de activación no es válido. Abre el enlace que te enviamos por correo.'

type Estado = { fase: 'cargando' } | { fase: 'exito'; texto: string } | { fase: 'error'; texto: string }

export function Activar() {
  const [parametros] = useSearchParams()
  const token = parametros.get('token')
  const solicitud = useRef<Promise<RespuestaMensaje> | null>(null)
  const [estado, setEstado] = useState<Estado>(
    token ? { fase: 'cargando' } : { fase: 'error', texto: MENSAJE_SIN_TOKEN },
  )

  useEffect(() => {
    if (!token) return

    let activo = true
    solicitud.current ??= activarCuenta(token)
    solicitud.current
      .then((respuesta) => {
        if (activo) setEstado({ fase: 'exito', texto: respuesta.mensaje })
      })
      .catch((fallo: unknown) => {
        if (activo) setEstado({ fase: 'error', texto: mensajeDeError(fallo) })
      })

    return () => {
      activo = false
    }
  }, [token])

  return (
    <PantallaAcceso titulo="Activa tu cuenta">
      {estado.fase === 'cargando' && (
        <p role="status" className="tarjeta__apoyo">
          Cargando...
        </p>
      )}
      <Mensaje
        tipo={estado.fase === 'error' ? 'error' : 'exito'}
        texto={estado.fase === 'cargando' ? null : estado.texto}
      />
      {estado.fase !== 'cargando' && (
        <Link className="boton boton--principal" to="/iniciar-sesion">
          Iniciar sesión
        </Link>
      )}
    </PantallaAcceso>
  )
}
