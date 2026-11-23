import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { mensajeDeError } from '../api/cliente'
import { registrarCuenta } from '../api/cuentas'
import { Boton } from '../componentes/Boton'
import { Campo } from '../componentes/Campo'
import { Mensaje } from '../componentes/Mensaje'
import { OpcionUso } from '../componentes/OpcionUso'
import { PantallaAcceso } from '../componentes/PantallaAcceso'
import { guardarEleccionUso, type EleccionUso } from '../contexto/eleccionUso'

export function Registro() {
  const [correo, setCorreo] = useState('')
  const [contrasena, setContrasena] = useState('')
  const [uso, setUso] = useState<EleccionUso>('cliente')
  const [error, setError] = useState<string | null>(null)
  const [exito, setExito] = useState<string | null>(null)
  const [cargando, setCargando] = useState(false)

  async function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault()
    setError(null)
    setCargando(true)
    try {
      const respuesta = await registrarCuenta(correo.trim(), contrasena)
      guardarEleccionUso(uso)
      setExito(respuesta.mensaje)
    } catch (fallo) {
      setError(mensajeDeError(fallo))
    } finally {
      setCargando(false)
    }
  }

  return (
    <PantallaAcceso titulo="Crea tu cuenta" apoyo="Te enviaremos un correo para activarla.">
      <Mensaje tipo="exito" texto={exito} />
      {exito ? (
        <Link className="boton boton--principal" to="/iniciar-sesion">
          Ir a iniciar sesión
        </Link>
      ) : (
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
            autoComplete="new-password"
            minLength={8}
            required
            ayuda="Mínimo 8 caracteres, con letras y números."
            value={contrasena}
            onChange={(evento) => setContrasena(evento.target.value)}
          />
          <fieldset className="grupo">
            <legend>¿Cómo quieres usar DashGo?</legend>
            <OpcionUso
              valor="cliente"
              texto="Quiero enviar o recibir paquetes"
              elegida={uso === 'cliente'}
              alElegir={setUso}
            />
            <OpcionUso
              valor="repartidor"
              texto="Quiero trabajar como repartidor"
              elegida={uso === 'repartidor'}
              alElegir={setUso}
            />
          </fieldset>
          <Mensaje tipo="error" texto={error} />
          <Boton type="submit" variante="principal" cargando={cargando}>
            Crear cuenta
          </Boton>
        </form>
      )}
      {!exito && (
        <nav className="enlaces" aria-label="Otras opciones">
          <Link to="/iniciar-sesion">Ya tengo una cuenta</Link>
        </nav>
      )}
    </PantallaAcceso>
  )
}
