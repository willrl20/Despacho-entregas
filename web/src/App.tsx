import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { RutaProtegida } from './componentes/RutaProtegida'
import { ProveedorSesion } from './contexto/ProveedorSesion'
import { Activar } from './paginas/Activar'
import { Inicio } from './paginas/Inicio'
import { IniciarSesion } from './paginas/IniciarSesion'
import { NoExiste } from './paginas/NoExiste'
import { Recuperar } from './paginas/Recuperar'
import { Registro } from './paginas/Registro'

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
            <Route index element={<Inicio />} />
          </Route>
          <Route path="*" element={<NoExiste />} />
        </Routes>
      </ProveedorSesion>
    </BrowserRouter>
  )
}
