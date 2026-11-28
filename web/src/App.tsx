import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { MarcoInterno } from './componentes/MarcoInterno'
import { RutaProtegida } from './componentes/RutaProtegida'
import { ProveedorSesion } from './contexto/ProveedorSesion'
import { Activar } from './paginas/Activar'
import { Administracion } from './paginas/Administracion'
import { CrearEnvio } from './paginas/CrearEnvio'
import { DetalleEnvio } from './paginas/DetalleEnvio'
import { IniciarSesion } from './paginas/IniciarSesion'
import { MisEnvios } from './paginas/MisEnvios'
import { NoExiste } from './paginas/NoExiste'
import { Recuperar } from './paginas/Recuperar'
import { Registro } from './paginas/Registro'
import { Repartir } from './paginas/Repartir'

export default function App() {
  return (
    <BrowserRouter>
      <ProveedorSesion>
        <Routes>
          <Route path="/iniciar-sesion" element={<IniciarSesion />} />
          <Route path="/registro" element={<Registro />} />
          <Route path="/recuperar" element={<Recuperar />} />
          <Route path="/activar" element={<Activar />} />
          <Route element={<RutaProtegida />}>
            <Route element={<MarcoInterno />}>
              <Route index element={<MisEnvios />} />
              <Route path="envios/nuevo" element={<CrearEnvio />} />
              <Route path="envios/:id" element={<DetalleEnvio />} />
              <Route path="repartir" element={<Repartir />} />
              <Route path="administracion" element={<Administracion />} />
            </Route>
          </Route>
          <Route path="*" element={<NoExiste />} />
        </Routes>
      </ProveedorSesion>
    </BrowserRouter>
  )
}
