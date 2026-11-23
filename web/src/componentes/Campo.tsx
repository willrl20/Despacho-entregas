import { useId, type InputHTMLAttributes } from 'react'

interface PropiedadesCampo extends InputHTMLAttributes<HTMLInputElement> {
  etiqueta: string
  ayuda?: string
}

export function Campo({ etiqueta, ayuda, id, ...resto }: PropiedadesCampo) {
  const idAutomatico = useId()
  const idCampo = id ?? idAutomatico
  const idAyuda = ayuda ? `${idCampo}-ayuda` : undefined

  return (
    <div className="campo">
      <label htmlFor={idCampo}>{etiqueta}</label>
      <input id={idCampo} aria-describedby={idAyuda} {...resto} />
      {ayuda && (
        <p id={idAyuda} className="campo__ayuda">
          {ayuda}
        </p>
      )}
    </div>
  )
}
