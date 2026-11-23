import type { EleccionUso } from '../contexto/eleccionUso'

interface PropiedadesOpcionUso {
  valor: EleccionUso
  texto: string
  elegida: boolean
  alElegir: (valor: EleccionUso) => void
}

export function OpcionUso({ valor, texto, elegida, alElegir }: PropiedadesOpcionUso) {
  return (
    <label className="opcion">
      <input type="radio" name="uso" value={valor} checked={elegida} onChange={() => alElegir(valor)} />
      <span>{texto}</span>
    </label>
  )
}
