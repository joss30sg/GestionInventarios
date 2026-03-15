export type UserRole = 'Admin' | 'Employee';

export interface User {
  id: number;
  username: string;
  email: string;
  role: UserRole;
}

export interface AuthResponse {
  success: boolean;
  data?: {
    token: string;
    user?: User;
    expiresIn?: number;
  };
  message?: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  role?: UserRole;
}

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  quantity: number;
  category: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface Inventory {
  id: number;
  productId: number;
  productName: string;
  category: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityOnOrder: number;
  availableQuantity: number;
  reorderLevel: number;
  reorderQuantity: number;
  status: 'OK' | 'LOW' | 'OUT_OF_STOCK';
  lastCountedAt?: string;
  lastMovementAt?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: Array<{
    field: string;
    message: string;
  }>;
}
