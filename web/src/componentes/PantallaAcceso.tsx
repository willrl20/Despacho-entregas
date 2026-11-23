import type { ReactNode } from 'react'
import { Logo } from './Logo'

interface PropiedadesPantallaAcceso {
  titulo: string
  apoyo?: string
  children: ReactNode
}

export function PantallaAcceso({ titulo, apoyo, children }: PropiedadesPantallaAcceso) {
  return (
    <main className="pantalla">
      <Logo />
      <div className="pantalla__contenido">
        <section className="tarjeta">
          <h1>{titulo}</h1>
          {apoyo && <p className="tarjeta__apoyo">{apoyo}</p>}
          {children}
        </section>
      </div>
    </main>
  )
}
