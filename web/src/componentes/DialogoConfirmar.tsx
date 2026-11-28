import { useEffect, useId, useRef } from 'react'
import { Boton } from './Boton'

interface PropiedadesDialogoConfirmar {
  abierto: boolean
  titulo: string
  texto: string
  textoConfirmar: string
  textoConservar: string
  cargando: boolean
  alConfirmar: () => void
  alCerrar: () => void
}

export function DialogoConfirmar({
  abierto,
  titulo,
  texto,
  textoConfirmar,
  textoConservar,
  cargando,
  alConfirmar,
  alCerrar,
}: PropiedadesDialogoConfirmar) {
  const referencia = useRef<HTMLDialogElement>(null)
  const idTitulo = useId()

  useEffect(() => {
    const dialogo = referencia.current
    if (!dialogo) return
    if (abierto && !dialogo.open) dialogo.showModal()
    if (!abierto && dialogo.open) dialogo.close()
  }, [abierto])

  return (
    <dialog ref={referencia} className="dialogo" aria-labelledby={idTitulo} onClose={alCerrar}>
      <div className="dialogo__contenido">
        <h2 id={idTitulo}>{titulo}</h2>
        <p>{texto}</p>
        <div className="dialogo__acciones">
          <Boton variante="principal" disabled={cargando} onClick={alCerrar}>
            {textoConservar}
          </Boton>
          <Boton cargando={cargando} onClick={alConfirmar}>
            {textoConfirmar}
          </Boton>
        </div>
      </div>
    </dialog>
  )
}
