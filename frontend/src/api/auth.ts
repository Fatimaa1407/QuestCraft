import { apiClient } from './client';
import type { ApiResponse } from '../types/api';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types/auth';

export async function login(payload: LoginRequest): Promise<ApiResponse<AuthResponse>> {
  const { data } = await apiClient.post<ApiResponse<AuthResponse>>('/api/auth/login', payload);
  return data;
}

export async function register(payload: RegisterRequest): Promise<ApiResponse<AuthResponse>> {
  const { data } = await apiClient.post<ApiResponse<AuthResponse>>('/api/auth/register', payload);
  return data;
}

export async function logout(): Promise<void> {
  await apiClient.post('/api/auth/logout');
}
