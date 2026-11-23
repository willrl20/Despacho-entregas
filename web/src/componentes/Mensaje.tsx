interface PropiedadesMensaje {
  tipo: 'error' | 'exito'
  texto: string | null
}

export function Mensaje({ tipo, texto }: PropiedadesMensaje) {
  return (
    <div role={tipo === 'error' ? 'alert' : 'status'} className="mensaje-region">
      {texto && (
        <p className={`mensaje mensaje--${tipo}`}>
          <strong>{tipo === 'error' ? 'Error:' : 'Listo:'}</strong> {texto}
        </p>
      )}
    </div>
  )
}
