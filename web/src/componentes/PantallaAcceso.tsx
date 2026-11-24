import type { ReactNode } from 'react'
import repartidor from '../assets/repartidor.svg'
import '../estilos/acceso.css'
import { Logo } from './Logo'
import { useIntroAcceso } from './useIntroAcceso'

interface PropiedadesPantallaAcceso {
  titulo: string
  apoyo?: string
  children: ReactNode
}

export function PantallaAcceso({ titulo, apoyo, children }: PropiedadesPantallaAcceso) {
  const { raizRef, enIntro, sinTransicion } = useIntroAcceso()
  const clases = ['acceso', enIntro && 'acceso--intro', sinTransicion && 'acceso--sin-transicion']
    .filter(Boolean)
    .join(' ')

  return (
    <main ref={raizRef} className={clases}>
      <div className="acceso__rejilla">
        <section className="acceso__presentacion">
          <Logo />
          <p className="acceso__lema acceso__aparece">Lo recogemos y lo llevamos.</p>
          <p className="acceso__texto acceso__aparece">Pide un repartidor para tu paquete o reparte con tu moto.</p>
          <img className="acceso__repartidor" src={repartidor} alt="" width={480} height={300} />
        </section>
        <div className="acceso__formulario" inert={enIntro}>
          <section className="tarjeta">
            <h1>{titulo}</h1>
            {apoyo && <p className="tarjeta__apoyo">{apoyo}</p>}
            {children}
          </section>
        </div>
      </div>
    </main>
  )
}
