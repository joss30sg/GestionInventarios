import { ReactNode, createContext, useContext, useState } from 'react';
import { API_CONFIG, AUTH_STORAGE_KEYS } from '../constants';
import { UserRole } from '../types';

interface User {
  id?: number;
  username: string;
  email?: string;
  role?: UserRole;
}

interface AuthContextType {
  token: string | null;
  user: User | null;
  login: (username: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string, role?: UserRole) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isEmployee: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const storedUser = localStorage.getItem(AUTH_STORAGE_KEYS.USER);
  const initialUser: User | null = storedUser ? JSON.parse(storedUser) : null;

  const [token, setToken] = useState<string | null>(
    localStorage.getItem(AUTH_STORAGE_KEYS.TOKEN)
  );
  const [user, setUser] = useState<User | null>(initialUser);

  const login = async (username: string, password: string) => {
    try {
      const response = await fetch(`${API_CONFIG.baseURL}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }),
      });

      const text = await response.text();
      let data;
      try {
        data = JSON.parse(text);
      } catch {
        throw new Error('Error de conexión con el servidor');
      }

      if (!response.ok) {
        throw new Error(data.message || 'Login fallido');
      }
      const newToken = data.token;
      const userData: User = {
        id: data.user?.id,
        username: data.user?.username || username,
        email: data.user?.email,
        role: data.user?.role || 'Employee',
      };

      setToken(newToken);
      setUser(userData);
      localStorage.setItem(AUTH_STORAGE_KEYS.TOKEN, newToken);
      localStorage.setItem(AUTH_STORAGE_KEYS.USER, JSON.stringify(userData));
    } catch (error) {
      console.error('Error en login:', error);
      throw error;
    }
  };

  const register = async (username: string, email: string, password: string, role: UserRole = 'Employee') => {
    try {
      const response = await fetch(`${API_CONFIG.baseURL}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, email, password, role }),
      });

      const text = await response.text();
      let data;
      try {
        data = JSON.parse(text);
      } catch {
        throw new Error('Error de conexión con el servidor');
      }

      if (!response.ok) {
        throw new Error(data.message || 'Registro fallido');
      }
      const newToken = data.token;
      const userData: User = {
        id: data.user?.id,
        username: data.user?.username || username,
        email: data.user?.email || email,
        role: data.user?.role || role,
      };

      setToken(newToken);
      setUser(userData);
      localStorage.setItem(AUTH_STORAGE_KEYS.TOKEN, newToken);
      localStorage.setItem(AUTH_STORAGE_KEYS.USER, JSON.stringify(userData));
    } catch (error) {
      console.error('Error en registro:', error);
      throw error;
    }
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem(AUTH_STORAGE_KEYS.TOKEN);
    localStorage.removeItem(AUTH_STORAGE_KEYS.USER);
  };

  const isAdmin = user?.role === 'Admin';
  const isEmployee = user?.role === 'Employee';

  return (
    <AuthContext.Provider 
      value={{ 
        token, 
        user, 
        login, 
        register,
        logout, 
        isAuthenticated: !!token,
        isAdmin,
        isEmployee,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth debe ser usado dentro de AuthProvider');
  }
  return context;
}
