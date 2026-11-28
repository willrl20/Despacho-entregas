import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { mensajeDeError } from '../api/cliente'
import { listarMisEnvios } from '../api/envios'
import type { Envio } from '../api/tipos'
import { useContextoInterno } from '../componentes/contextoInterno'
import { GuiaEnvio } from '../componentes/GuiaEnvio'
import { Mensaje } from '../componentes/Mensaje'
import { TarjetaPermisoRepartidor } from '../componentes/TarjetaPermisoRepartidor'
import { leerEleccionUso } from '../contexto/eleccionUso'

function masNuevoPrimero(envios: Envio[]): Envio[] {
  return [...envios].sort((a, b) => new Date(b.creadoEnUtc).getTime() - new Date(a.creadoEnUtc).getTime() || b.id - a.id)
}

export function MisEnvios() {
  const { puedeRepartir, cargandoPermisos } = useContextoInterno()
  const [envios, setEnvios] = useState<Envio[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [quiereRepartir] = useState(() => leerEleccionUso() === 'repartidor')

  useEffect(() => {
    let activo = true
    listarMisEnvios()
      .then((lista) => {
        if (activo) setEnvios(masNuevoPrimero(lista))
      })
      .catch((fallo: unknown) => {
        if (activo) setError(mensajeDeError(fallo))
      })

    return () => {
      activo = false
    }
  }, [])

  const cargando = envios === null && error === null
  const vacio = envios !== null && envios.length === 0

  return (
    <>
      <div className="pagina__cabecera">
        <h1>Mis envíos</h1>
        {envios !== null && !vacio && (
          <Link className="boton boton--principal" to="/envios/nuevo">
            Crear envío
          </Link>
        )}
      </div>

      {quiereRepartir && !cargandoPermisos && !puedeRepartir && <TarjetaPermisoRepartidor />}

      <Mensaje tipo="error" texto={error} />

      {cargando && (
        <p role="status" className="tarjeta__apoyo">
          Cargando...
        </p>
      )}

      {vacio && (
        <section className="pagina__vacia">
          <p>Todavía no tienes envíos.</p>
          <Link className="boton boton--principal" to="/envios/nuevo">
            Crear envío
          </Link>
        </section>
      )}

      {envios !== null && !vacio && (
        <ul className="envios__lista">
          {envios.map((envio) => (
            <li key={envio.id}>
              <Link className="guia-enlace" to={`/envios/${envio.id}`}>
                <GuiaEnvio
                  id={envio.id}
                  estado={envio.estado}
                  tamano={envio.tamano}
                  distanciaKm={envio.distanciaKm}
                  costo={envio.costo}
                  descripcion={envio.descripcionPaquete}
                />
              </Link>
            </li>
          ))}
        </ul>
      )}
    </>
  )
}
