export const API_CONFIG = {
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  timeout: 10000,
  retries: 3,
};

export const ROUTES = {
  LOGIN: '/login',
  INVENTORY: '/inventory',
  HOME: '/',
};

export const AUTH_STORAGE_KEYS = {
  TOKEN: 'auth_token',
  USER: 'auth_user',
};

export const INVENTORY_STATUS = {
  OK: 'OK',
  LOW: 'LOW',
  OUT_OF_STOCK: 'OUT_OF_STOCK',
};
