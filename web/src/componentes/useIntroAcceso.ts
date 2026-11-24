import { useEffect, useLayoutEffect, useRef, useState } from 'react'

const CLAVE_INTRO = 'dashgo.intro'
const ESPERA_MS = 1200

type Fase = 'sin' | 'intro' | 'saltada' | 'fin'

function debeMostrarIntro(): boolean {
  try {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return false
    return sessionStorage.getItem(CLAVE_INTRO) === null
  } catch {
    return false
  }
}

const SEPARACION_PX = 8

function centrarConjunto(raiz: HTMLElement): void {
  const logo = raiz.querySelector<HTMLElement>('.logo')
  const repartidor = raiz.querySelector<HTMLElement>('.acceso__repartidor')
  if (!logo || !repartidor) return

  const escala = parseFloat(getComputedStyle(raiz).getPropertyValue('--acceso-escala-intro')) || 1
  const centroX = raiz.clientWidth / 2
  const alto = raiz.clientHeight

  const logoX = logo.offsetLeft + logo.offsetWidth / 2
  const logoY = logo.offsetTop + logo.offsetHeight / 2
  const repartidorX = repartidor.offsetLeft + repartidor.offsetWidth / 2
  const repartidorY = repartidor.offsetTop + repartidor.offsetHeight / 2

  const altoLogo = logo.offsetHeight * escala
  const total = altoLogo + SEPARACION_PX + repartidor.offsetHeight
  const arriba = (alto - total) / 2

  const destinoLogoY = arriba + altoLogo / 2
  const destinoRepartidorY = arriba + altoLogo + SEPARACION_PX + repartidor.offsetHeight / 2

  raiz.style.setProperty('--intro-logo-x', `${centroX - logoX}px`)
  raiz.style.setProperty('--intro-logo-y', `${destinoLogoY - logoY}px`)
  raiz.style.setProperty('--intro-repartidor-x', `${centroX - repartidorX}px`)
  raiz.style.setProperty('--intro-repartidor-y', `${destinoRepartidorY - repartidorY}px`)
}

export function useIntroAcceso() {
  const raizRef = useRef<HTMLElement>(null)
  const [fase, setFase] = useState<Fase>(() => (debeMostrarIntro() ? 'intro' : 'sin'))

  useLayoutEffect(() => {
    const raiz = raizRef.current
    if (fase !== 'intro' || !raiz) return

    let activo = true
    const medir = () => {
      if (activo) centrarConjunto(raiz)
    }

    medir()
    window.addEventListener('resize', medir)
    document.fonts?.ready.then(medir)

    return () => {
      activo = false
      window.removeEventListener('resize', medir)
    }
  }, [fase])

  useEffect(() => {
    if (fase !== 'intro') return

    try {
      sessionStorage.setItem(CLAVE_INTRO, '1')
    } catch {
      // Si no se puede recordar, la entrada simplemente puede repetirse.
    }

    const temporizador = window.setTimeout(() => setFase('fin'), ESPERA_MS)
    const saltar = () => setFase('saltada')
    document.addEventListener('click', saltar)
    document.addEventListener('keydown', saltar)

    return () => {
      window.clearTimeout(temporizador)
      document.removeEventListener('click', saltar)
      document.removeEventListener('keydown', saltar)
    }
  }, [fase])

  return {
    raizRef,
    enIntro: fase === 'intro',
    sinTransicion: fase === 'sin' || fase === 'saltada',
  }
}
