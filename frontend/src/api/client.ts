import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import i18n from '../i18n';
import { useAuthStore } from '../app/authStore';
import type { ApiResponse } from '../types/api';
import type { AuthResponse } from '../types/auth';

const baseURL = import.meta.env.VITE_API_URL;

export const apiClient = axios.create({ baseURL, withCredentials: true });

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  config.headers['X-Language'] = i18n.language;
  return config;
});

let refreshPromise: Promise<string | null> | null = null;

export async function refreshAccessToken(): Promise<string | null> {
  try {
    const response = await axios.post<ApiResponse<AuthResponse>>(
      `${baseURL}/api/auth/refresh-token`,
      {},
      { withCredentials: true },
    );
    const data = response.data.data;
    if (!data) {
      return null;
    }
    useAuthStore.getState().setAuth(data.user, data.accessToken);
    return data.accessToken;
  } catch {
    useAuthStore.getState().clearAuth();
    return null;
  }
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;
    const status = error.response?.status;
    const isAuthEndpoint = originalRequest?.url?.includes('/api/auth/');

    if (status === 401 && originalRequest && !originalRequest._retry && !isAuthEndpoint) {
      originalRequest._retry = true;

      refreshPromise ??= refreshAccessToken().finally(() => {
        refreshPromise = null;
      });

      const newToken = await refreshPromise;
      if (newToken) {
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(originalRequest);
      }
    }

    return Promise.reject(error);
  },
);
