import { Link } from 'react-router-dom'
import { PantallaAcceso } from '../componentes/PantallaAcceso'

export function NoExiste() {
  return (
    <PantallaAcceso titulo="Esta página no existe" apoyo="Revisa la dirección o vuelve al inicio.">
      <Link className="boton boton--principal" to="/">
        Ir al inicio
      </Link>
    </PantallaAcceso>
  )
}
