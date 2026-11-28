import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { mensajeDeError } from '../api/cliente'
import { cancelarEnvio, listarMisEnvios } from '../api/envios'
import type { Envio } from '../api/tipos'
import { Boton } from '../componentes/Boton'
import { DialogoConfirmar } from '../componentes/DialogoConfirmar'
import { GuiaEnvio } from '../componentes/GuiaEnvio'
import { LineaEstado } from '../componentes/LineaEstado'
import { Mensaje } from '../componentes/Mensaje'
import { formatearFecha, numeroGuia } from '../utilidades/formato'

const MENSAJE_NO_ENCONTRADO = 'No encontramos ese envío. Vuelve a tus envíos y elige otro.'

export function DetalleEnvio() {
  const { id } = useParams()
  const idEnvio = Number(id)
  const [envios, setEnvios] = useState<Envio[] | null>(null)
  const [recarga, setRecarga] = useState(0)
  const [error, setError] = useState<string | null>(null)
  const [exito, setExito] = useState<string | null>(null)
  const [confirmando, setConfirmando] = useState(false)
  const [cancelando, setCancelando] = useState(false)

  useEffect(() => {
    let activo = true
    listarMisEnvios()
      .then((lista) => {
        if (activo) setEnvios(lista)
      })
      .catch((fallo: unknown) => {
        if (activo) setError(mensajeDeError(fallo))
      })

    return () => {
      activo = false
    }
  }, [recarga])

  async function cancelar() {
    setError(null)
    setExito(null)
    setCancelando(true)
    try {
      const respuesta = await cancelarEnvio(idEnvio)
      setExito(respuesta.mensaje)
      setRecarga((valor) => valor + 1)
    } catch (fallo) {
      setError(mensajeDeError(fallo))
    } finally {
      setCancelando(false)
      setConfirmando(false)
    }
  }

  const envio = envios?.find((e) => e.id === idEnvio)
  const cargando = envios === null && error === null
  const noExiste = envios !== null && !envio
  const puedeCancelar = envio !== undefined && (envio.estado === 'Pendiente' || envio.estado === 'Asignado')

  return (
    <>
      <div className="pagina__cabecera">
        <h1>{Number.isInteger(idEnvio) ? `Envío ${numeroGuia(idEnvio)}` : 'Envío'}</h1>
        <Link className="boton boton--texto" to="/">
          Volver a mis envíos
        </Link>
      </div>

      <Mensaje tipo="error" texto={error ?? (noExiste ? MENSAJE_NO_ENCONTRADO : null)} />
      <Mensaje tipo="exito" texto={exito} />

      {cargando && (
        <p role="status" className="tarjeta__apoyo">
          Cargando...
        </p>
      )}

      {envio && (
        <div className="detalle">
          <GuiaEnvio
            id={envio.id}
            estado={envio.estado}
            tamano={envio.tamano}
            distanciaKm={envio.distanciaKm}
            costo={envio.costo}
            descripcion={envio.descripcionPaquete}
          />

          <section className="tarjeta" aria-labelledby="titulo-progreso">
            <h2 id="titulo-progreso">Estado del envío</h2>
            <LineaEstado estado={envio.estado} />
            <dl className="detalle__datos">
              <dt>Modalidad</dt>
              <dd>{envio.modalidad}</dd>
              <dt>Creado el</dt>
              <dd>{formatearFecha(envio.creadoEnUtc)}</dd>
            </dl>
          </section>

          {puedeCancelar && (
            <div className="pagina__acciones">
              <Boton onClick={() => setConfirmando(true)}>Cancelar envío</Boton>
            </div>
          )}
        </div>
      )}

      <DialogoConfirmar
        abierto={confirmando}
        titulo="¿Cancelar este envío?"
        texto="Si lo cancelas, el envío no se podrá reactivar. Tendrás que crear uno nuevo."
        textoConfirmar="Cancelar envío"
        textoConservar="Conservar envío"
        cargando={cancelando}
        alConfirmar={cancelar}
        alCerrar={() => setConfirmando(false)}
      />
    </>
  )
}
