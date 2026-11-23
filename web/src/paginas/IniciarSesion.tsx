import { useState, type FormEvent } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { mensajeDeError } from '../api/cliente'
import { Boton } from '../componentes/Boton'
import { Campo } from '../componentes/Campo'
import { Mensaje } from '../componentes/Mensaje'
import { PantallaAcceso } from '../componentes/PantallaAcceso'
import { useSesion } from '../contexto/useSesion'

export function IniciarSesion() {
  const { usuario, iniciarSesion } = useSesion()
  const [correo, setCorreo] = useState('')
  const [contrasena, setContrasena] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [cargando, setCargando] = useState(false)

  if (usuario) {
    return <Navigate to="/" replace />
  }

  async function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault()
    setError(null)
    setCargando(true)
    try {
      await iniciarSesion(correo.trim(), contrasena)
    } catch (fallo) {
      setError(mensajeDeError(fallo))
    } finally {
      setCargando(false)
    }
  }

  return (
    <PantallaAcceso titulo="Inicia sesión" apoyo="Entra para pedir o hacer entregas.">
      <form className="formulario" onSubmit={enviar}>
        <Campo
          etiqueta="Correo"
          type="email"
          name="correo"
          autoComplete="username"
          inputMode="email"
          required
          value={correo}
          onChange={(evento) => setCorreo(evento.target.value)}
        />
        <Campo
          etiqueta="Contraseña"
          type="password"
          name="contrasena"
          autoComplete="current-password"
          required
          value={contrasena}
          onChange={(evento) => setContrasena(evento.target.value)}
        />
        <Mensaje tipo="error" texto={error} />
        <Boton type="submit" variante="principal" cargando={cargando}>
          Iniciar sesión
        </Boton>
      </form>
      <nav className="enlaces" aria-label="Otras opciones">
        <Link to="/registro">Crear una cuenta</Link>
        <Link to="/recuperar">Recuperar mi contraseña</Link>
      </nav>
    </PantallaAcceso>
  )
}
