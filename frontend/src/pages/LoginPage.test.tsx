import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LoginPage } from './LoginPage';

const mockNavigate = vi.fn();
const mockLogin = vi.fn();

vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}));

vi.mock('../context/AuthContext', () => ({
  useAuth: () => ({
    login: mockLogin,
  }),
}));

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders login form', () => {
    render(<LoginPage />);
    expect(screen.getByLabelText('Usuario')).toBeInTheDocument();
    expect(screen.getByLabelText('Contraseña')).toBeInTheDocument();
    expect(screen.getByText('Iniciar sesión')).toBeInTheDocument();
  });

  it('renders app title', () => {
    render(<LoginPage />);
    expect(screen.getByText(/Gestión de Inventarios/)).toBeInTheDocument();
  });

  it('allows typing username and password', async () => {
    const user = userEvent.setup();
    render(<LoginPage />);

    const usernameInput = screen.getByLabelText('Usuario');
    const passwordInput = screen.getByLabelText('Contraseña');

    await user.type(usernameInput, 'admin');
    await user.type(passwordInput, 'password123');

    expect(usernameInput).toHaveValue('admin');
    expect(passwordInput).toHaveValue('password123');
  });

  it('calls login and navigates on successful submit', async () => {
    mockLogin.mockResolvedValueOnce(undefined);
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.type(screen.getByLabelText('Usuario'), 'admin');
    await user.type(screen.getByLabelText('Contraseña'), 'password123');
    await user.click(screen.getByText('Iniciar sesión'));

    expect(mockLogin).toHaveBeenCalledWith('admin', 'password123');
    expect(mockNavigate).toHaveBeenCalledWith('/inventory');
  });

  it('shows error on login failure', async () => {
    mockLogin.mockRejectedValueOnce(new Error('Credenciales inválidas'));
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.type(screen.getByLabelText('Usuario'), 'admin');
    await user.type(screen.getByLabelText('Contraseña'), 'wrong');
    await user.click(screen.getByText('Iniciar sesión'));

    expect(await screen.findByText('Credenciales inválidas')).toBeInTheDocument();
  });

  it('shows loading state during submission', async () => {
    // Login never resolves to test loading state
    mockLogin.mockImplementation(() => new Promise(() => {}));
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.type(screen.getByLabelText('Usuario'), 'admin');
    await user.type(screen.getByLabelText('Contraseña'), 'pass');
    await user.click(screen.getByText('Iniciar sesión'));

    expect(screen.getByText('Iniciando sesión...')).toBeInTheDocument();
  });
});
