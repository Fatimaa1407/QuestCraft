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

// For requests made with `responseType: 'blob'` (file downloads) — axios applies that responseType
// to error responses too, so a JSON error body (e.g. the certificate endpoint's 403 explaining which
// levels are still unfinished) arrives as an opaque Blob instead of parsed JSON, and the sync
// getApiErrorMessage above can't see past it to the real message. This reads the Blob's text and
// parses it the same way, falling back to the sync path for anything that isn't a JSON blob error.
export async function getApiErrorMessageFromBlob(error: unknown, fallback = 'Xəta baş verdi.'): Promise<string> {
  if (isAxiosError(error) && error.response?.data instanceof Blob) {
    try {
      const text = await error.response.data.text();
      const body = JSON.parse(text) as ApiResponse<unknown>;
      if (body?.message) {
        return body.message;
      }
    } catch {
      // Not JSON (or empty) — fall through to the generic handling below.
    }
  }
  return getApiErrorMessage(error, fallback);
}
