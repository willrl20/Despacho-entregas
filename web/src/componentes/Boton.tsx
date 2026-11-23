import type { ButtonHTMLAttributes } from 'react'

interface PropiedadesBoton extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: 'principal' | 'secundario' | 'texto'
  cargando?: boolean
}

export function Boton({
  variante = 'secundario',
  cargando = false,
  type = 'button',
  disabled,
  children,
  ...resto
}: PropiedadesBoton) {
  return (
    <button
      {...resto}
      type={type}
      className={`boton boton--${variante}`}
      disabled={disabled || cargando}
      aria-busy={cargando || undefined}
    >
      {cargando ? 'Cargando...' : children}
    </button>
  )
}
