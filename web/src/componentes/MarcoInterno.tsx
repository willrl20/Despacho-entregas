import { useState } from 'react'
import { Link, Outlet, useLocation } from 'react-router-dom'
import '../estilos/interno.css'
import { useSesion } from '../contexto/useSesion'
import { usePermisoRepartidor } from '../contexto/usePermisoRepartidor'
import { Boton } from './Boton'
import type { ContextoInterno } from './contextoInterno'
import { Logo } from './Logo'

interface OpcionMenu {
  ruta: string
  texto: string
  activa: boolean
}

export function MarcoInterno() {
  const { usuario, cerrarSesion } = useSesion()
  const { pathname } = useLocation()
  const permisos = usePermisoRepartidor(usuario?.id ?? null)
  const [cerrando, setCerrando] = useState(false)

  if (!usuario) return null

  async function salir() {
    setCerrando(true)
    await cerrarSesion()
  }

  const opciones: OpcionMenu[] = [
    { ruta: '/', texto: 'Mis envíos', activa: pathname === '/' || pathname.startsWith('/envios') },
  ]
  if (permisos.puedeRepartir) {
    opciones.push({ ruta: '/repartir', texto: 'Repartir', activa: pathname.startsWith('/repartir') })
  }
  if (usuario.rol === 'Administrador') {
    opciones.push({ ruta: '/administracion', texto: 'Administración', activa: pathname.startsWith('/administracion') })
  }

  const contexto: ContextoInterno = permisos

  return (
    <div className="interno">
      <div className="interno__marca">
        <Logo />
      </div>
      <div className="interno__salir">
        <Boton cargando={cerrando} onClick={salir}>
          Cerrar sesión
        </Boton>
      </div>
      <nav className="interno__menu" aria-label="Principal">
        <ul>
          {opciones.map((opcion) => (
            <li key={opcion.ruta}>
              <Link to={opcion.ruta} aria-current={opcion.activa ? 'page' : undefined}>
                {opcion.texto}
              </Link>
            </li>
          ))}
        </ul>
      </nav>
      <main className="interno__contenido">
        <div className="interno__pagina">
          <Outlet context={contexto} />
        </div>
      </main>
    </div>
  )
}
