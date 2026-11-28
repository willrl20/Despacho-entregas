import { Link } from 'react-router-dom'

interface PropiedadesMuyPronto {
  titulo: string
  apoyo?: string
}

export function MuyPronto({ titulo, apoyo }: PropiedadesMuyPronto) {
  return (
    <>
      <h1>{titulo}</h1>
      <section className="pagina__vacia">
        <p className="pastilla">Muy pronto</p>
        {apoyo && <p className="tarjeta__apoyo">{apoyo}</p>}
        <Link className="boton boton--secundario" to="/">
          Volver a mis envíos
        </Link>
      </section>
    </>
  )
}
