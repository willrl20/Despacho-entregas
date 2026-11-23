import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { mensajeDeError } from '../api/cliente'
import { recuperarContrasena, restablecerContrasena } from '../api/contrasenas'
import { Boton } from '../componentes/Boton'
import { Campo } from '../componentes/Campo'
import { Mensaje } from '../componentes/Mensaje'
import { PantallaAcceso } from '../componentes/PantallaAcceso'

type Paso = 'correo' | 'codigo' | 'listo'

export function Recuperar() {
  const [paso, setPaso] = useState<Paso>('correo')
  const [correo, setCorreo] = useState('')
  const [codigo, setCodigo] = useState('')
  const [contrasenaNueva, setContrasenaNueva] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [exito, setExito] = useState<string | null>(null)
  const [cargando, setCargando] = useState(false)

  async function pedirCodigo(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault()
    setError(null)
    setCargando(true)
    try {
      const respuesta = await recuperarContrasena(correo.trim())
      setExito(respuesta.mensaje)
      setPaso('codigo')
    } catch (fallo) {
      setError(mensajeDeError(fallo))
    } finally {
      setCargando(false)
    }
  }

  async function cambiarContrasena(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault()
    setError(null)
    setCargando(true)
    try {
      const respuesta = await restablecerContrasena(correo.trim(), codigo.trim(), contrasenaNueva)
      setExito(respuesta.mensaje)
      setPaso('listo')
    } catch (fallo) {
      setError(mensajeDeError(fallo))
    } finally {
      setCargando(false)
    }
  }

  function cambiarCorreo() {
    setError(null)
    setExito(null)
    setCodigo('')
    setContrasenaNueva('')
    setPaso('correo')
  }

  const apoyo =
    paso === 'correo'
      ? 'Escribe tu correo y te enviaremos un código.'
      : paso === 'codigo'
        ? `Escribe el código que enviamos a ${correo.trim()} y elige tu nueva contraseña.`
        : undefined

  return (
    <PantallaAcceso titulo="Recupera tu contraseña" apoyo={apoyo}>
      <Mensaje tipo="exito" texto={exito} />
      {paso === 'correo' && (
        <form className="formulario" onSubmit={pedirCodigo}>
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
          <Mensaje tipo="error" texto={error} />
          <Boton type="submit" variante="principal" cargando={cargando}>
            Pedir código
          </Boton>
        </form>
      )}
      {paso === 'codigo' && (
        <form className="formulario" onSubmit={cambiarContrasena}>
          <Campo
            etiqueta="Código"
            type="text"
            name="codigo"
            autoComplete="one-time-code"
            inputMode="numeric"
            maxLength={6}
            required
            value={codigo}
            onChange={(evento) => setCodigo(evento.target.value)}
          />
          <Campo
            etiqueta="Contraseña nueva"
            type="password"
            name="contrasenaNueva"
            autoComplete="new-password"
            minLength={8}
            required
            ayuda="Mínimo 8 caracteres, con letras y números."
            value={contrasenaNueva}
            onChange={(evento) => setContrasenaNueva(evento.target.value)}
          />
          <Mensaje tipo="error" texto={error} />
          <Boton type="submit" variante="principal" cargando={cargando}>
            Cambiar contraseña
          </Boton>
          <Boton variante="texto" onClick={cambiarCorreo}>
            Usar otro correo
          </Boton>
        </form>
      )}
      {paso === 'listo' && (
        <Link className="boton boton--principal" to="/iniciar-sesion">
          Ir a iniciar sesión
        </Link>
      )}
      {paso !== 'listo' && (
        <nav className="enlaces" aria-label="Otras opciones">
          <Link to="/iniciar-sesion">Volver a iniciar sesión</Link>
        </nav>
      )}
    </PantallaAcceso>
  )
}
