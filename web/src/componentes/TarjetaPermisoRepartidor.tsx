import { useState } from 'react'
import { mensajeDeError } from '../api/cliente'
import { crearSolicitudPermiso } from '../api/solicitudes'
import { Boton } from './Boton'
import { Mensaje } from './Mensaje'

export function TarjetaPermisoRepartidor() {
  const [error, setError] = useState<string | null>(null)
  const [exito, setExito] = useState<string | null>(null)
  const [pidiendo, setPidiendo] = useState(false)

  async function pedirPermiso() {
    setError(null)
    setExito(null)
    setPidiendo(true)
    try {
      const respuesta = await crearSolicitudPermiso('Repartidor')
      setExito(respuesta.mensaje)
    } catch (fallo) {
      setError(mensajeDeError(fallo))
    } finally {
      setPidiendo(false)
    }
  }

  return (
    <section className="tarjeta" aria-labelledby="titulo-permiso-repartidor">
      <h2 id="titulo-permiso-repartidor">Trabaja como repartidor</h2>
      <p className="tarjeta__apoyo">
        Para aceptar envíos necesitas el permiso de repartidor. Un administrador revisa tu solicitud.
      </p>
      <Mensaje tipo="error" texto={error} />
      <Mensaje tipo="exito" texto={exito} />
      <Boton cargando={pidiendo} onClick={pedirPermiso}>
        Pedir permiso de repartidor
      </Boton>
    </section>
  )
}
