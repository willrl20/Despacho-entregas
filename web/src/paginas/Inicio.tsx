import { useState } from 'react'
import { mensajeDeError } from '../api/cliente'
import { crearSolicitudPermiso } from '../api/solicitudes'
import { Boton } from '../componentes/Boton'
import { Logo } from '../componentes/Logo'
import { Mensaje } from '../componentes/Mensaje'
import { leerEleccionUso } from '../contexto/eleccionUso'
import { useSesion } from '../contexto/useSesion'

const NOMBRES_ROL: Record<string, string> = {
  Estandar: 'Estándar',
  Administrador: 'Administrador',
}

export function Inicio() {
  const { usuario, cerrarSesion } = useSesion()
  const [error, setError] = useState<string | null>(null)
  const [exito, setExito] = useState<string | null>(null)
  const [pidiendo, setPidiendo] = useState(false)
  const [cerrando, setCerrando] = useState(false)
  const [esRepartidor] = useState(() => leerEleccionUso() === 'repartidor')

  if (!usuario) return null

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

  async function salir() {
    setCerrando(true)
    await cerrarSesion()
  }

  const accesos = ['Mis envíos', 'Repartir']
  if (usuario.rol === 'Administrador') accesos.push('Administración')

  return (
    <main className="pantalla">
      <div className="pantalla__contenido pantalla__contenido--ancha">
        <header className="encabezado">
          <Logo />
          <Boton cargando={cerrando} onClick={salir}>
            Cerrar sesión
          </Boton>
        </header>

        <section className="saludo" aria-labelledby="titulo-saludo">
          <h1 id="titulo-saludo">Hola, {usuario.correo}</h1>
          <p className="pastilla">Rol: {NOMBRES_ROL[usuario.rol] ?? usuario.rol}</p>
        </section>

        {esRepartidor && (
          <section className="tarjeta" aria-labelledby="titulo-repartidor">
            <h2 id="titulo-repartidor">Trabaja como repartidor</h2>
            <p className="tarjeta__apoyo">
              Para aceptar envíos necesitas el permiso de repartidor. Un administrador revisa tu solicitud.
            </p>
            <Mensaje tipo="error" texto={error} />
            <Mensaje tipo="exito" texto={exito} />
            <Boton variante="principal" cargando={pidiendo} onClick={pedirPermiso}>
              Pedir permiso de repartidor
            </Boton>
          </section>
        )}

        <ul className="accesos">
          {accesos.map((nombre) => (
            <li key={nombre} className="acceso">
              <span className="acceso__nombre">{nombre}</span>
              <span className="pastilla">Próximamente</span>
            </li>
          ))}
        </ul>
      </div>
    </main>
  )
}
