import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import './Header.css';

export function Header() {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="header">
      <div className="header-container">
        <div className="header-title">
          <h1>📦 Gestión de Inventarios</h1>
        </div>
        
        {user && (
          <div className="header-user">
            <div className="user-info">
              <span className="user-name">
                {isAdmin ? '👨‍💼' : '👤'} {user.username}
              </span>
              <span className="user-role">
                {isAdmin ? 'Administrador' : 'Empleado'}
              </span>
            </div>
            {isAdmin && (
              <button 
                className="btn-reports" 
                onClick={() => navigate('/reports')}
                title="Ver reportes"
              >
                📊 Reportes
              </button>
            )}
            <button 
              className="btn-products" 
              onClick={() => navigate('/products')}
              title="Ver productos"
            >
              📦 Productos
            </button>
            <button 
              className="btn-profile" 
              onClick={() => navigate('/profile')}
              title="Ver perfil"
            >
              👤 Perfil
            </button>
            <button className="btn-logout" onClick={handleLogout}>
              🚪 Salir
            </button>
          </div>
        )}
      </div>
    </header>
  );
}
