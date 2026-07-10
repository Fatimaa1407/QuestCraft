import { isAxiosError } from 'axios';
import type { ApiResponse } from '../types/api';

export function getApiErrorMessage(error: unknown, fallback = 'Xəta baş verdi.'): string {
  if (isAxiosError<ApiResponse<unknown>>(error)) {
    const body = error.response?.data;
    if (body?.errors) {
      const firstField = Object.values(body.errors)[0];
      if (firstField?.length) {
        return firstField[0];
      }
    }
    if (body?.message) {
      return body.message;
    }
  }
  return fallback;
}
