import axios, { AxiosInstance } from 'axios';
import { API_CONFIG, AUTH_STORAGE_KEYS } from '../constants';

const API_BASE_URL = `${API_CONFIG.baseURL}/api`;

let apiInstance: AxiosInstance;

const createApiInstance = (token?: string) => {
  apiInstance = axios.create({
    baseURL: API_BASE_URL,
    headers: {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    },
  });
  apiInstance.interceptors.request.use((config) => {
    const storedToken = localStorage.getItem(AUTH_STORAGE_KEYS.TOKEN);
    if (storedToken) {
      config.headers['Authorization'] = `Bearer ${storedToken}`;
    }
    return config;
  });
  return apiInstance;
};

export const initializeApi = (token?: string) => {
  return createApiInstance(token);
};

export const setAuthToken = (token: string) => {
  if (apiInstance) {
    apiInstance.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  }
};

export const getApi = () => {
  if (!apiInstance) {
    createApiInstance();
  }
  return apiInstance;
};

export const inventoryApi = {
  getAll: () => getApi().get('/v1/inventory'),
  getInventory: (productId: number) =>
    getApi().get(`/v1/inventory/products/${productId}`),
  getAvailable: (productId: number) =>
    getApi().get(`/v1/inventory/products/${productId}/available`),
  getAlerts: () => getApi().get('/v1/inventory/alerts'),
  getMovements: (params?: any) =>
    getApi().get('/v1/inventory/movements', { params }),
  adjustInventory: (data: any) =>
    getApi().post('/v1/inventory/adjustments', data),
};

export const authApi = {
  login: (username: string, password: string) =>
    axios.post(`${API_CONFIG.baseURL}/api/auth/login`, { username, password }, {
      httpsAgent: { rejectUnauthorized: false },
    }),
  register: (username: string, email: string, password: string, role: string = 'Employee') =>
    axios.post(`${API_CONFIG.baseURL}/api/auth/register`, { username, email, password, role }, {
      httpsAgent: { rejectUnauthorized: false },
    }),
};

export const productsApi = {
  getAll: () => getApi().get('/products'),
  getById: (id: number) => getApi().get(`/products/${id}`),
  create: (data: { name: string; description: string; price: number; quantity: number; category: string }) =>
    getApi().post('/products', data),
  update: (id: number, data: { name: string; description: string; price: number; quantity: number; category: string }) =>
    getApi().put(`/products/${id}`, data),
  delete: (id: number) => getApi().delete(`/products/${id}`),
};
