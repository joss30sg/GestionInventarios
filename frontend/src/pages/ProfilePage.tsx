import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import './ProfilePage.css';

export function ProfilePage() {
  const { user, logout, isAdmin, isEmployee } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (!user) {
    return null;
  }

  return (
    <div className="profile-page">
      <div className="profile-container">
        <div className="profile-card">
          <div className="profile-header">
            <div className="profile-avatar">
              {isAdmin ? '👨‍💼' : '👤'}
            </div>
            <h1>{user.username}</h1>
          </div>

          <div className="profile-info">
            <div className="info-item">
              <label>Email</label>
              <p>{user.email || 'No especificado'}</p>
            </div>

            <div className="info-item">
              <label>Rol</label>
              <div className="role-badge">
                {isAdmin ? (
                  <>
                    <span className="badge admin">👨‍💼 Administrador</span>
                    <p className="role-description">Acceso completo al sistema</p>
                  </>
                ) : (
                  <>
                    <span className="badge employee">👤 Empleado</span>
                    <p className="role-description">Acceso limitado (solo lectura y reportes)</p>
                  </>
                )}
              </div>
            </div>

            {isAdmin && (
              <div className="admin-features">
                <h3>✨ Características de Administrador</h3>
                <ul>
                  <li>✓ Ver historial completo de inventarios</li>
                  <li>✓ Crear y editar productos</li>
                  <li>✓ Registrar movimientos de stock</li>
                  <li>✓ Gestionar usuarios del sistema</li>
                  <li>✓ Generar reportes avanzados</li>
                </ul>
              </div>
            )}

            {isEmployee && (
              <div className="employee-features">
                <h3>✨ Características de Empleado</h3>
                <ul>
                  <li>✓ Ver productos y disponibilidad</li>
                  <li>✓ Reportar inventarios bajos</li>
                  <li>✓ Visualizar alertas de stock</li>
                </ul>
              </div>
            )}
          </div>

          <div className="profile-actions">
            <button className="btn-back" onClick={() => navigate('/inventory')}>
              ← Volver al Inventario
            </button>
            <button className="btn-logout" onClick={handleLogout}>
              Cerrar Sesión
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
