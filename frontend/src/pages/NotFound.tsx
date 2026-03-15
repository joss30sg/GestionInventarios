import './NotFound.css';

export function NotFound() {
  return (
    <div className="not-found-page">
      <div className="not-found-container">
        <h1 className="not-found-code">404</h1>
        <h2>Página no encontrada</h2>
        <p>La página que buscas no existe.</p>
        <a href="/" className="btn-home">
          Volver al inicio
        </a>
      </div>
    </div>
  );
}
