import { HttpErrorResponse } from '@angular/common/http';

export function extractHttpErrorMessage(error: unknown): string | null {
  if (!(error instanceof HttpErrorResponse)) return null;

  const body = error.error;
  if (typeof body === 'string' && body.trim()) return body.trim();
  if (body && typeof body === 'object') {
    const message = (body as { message?: string }).message;
    if (message?.trim()) return message.trim();
  }

  if (error.status === 0) return 'تعذر الاتصال بالخادم. تحقق من الشبكة.';
  if (error.status >= 500) return 'حدث خطأ غير متوقع. حاول مرة أخرى.';
  return error.message || null;
}
