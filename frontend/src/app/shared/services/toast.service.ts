import { Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

export type ToastType = 'success' | 'warning' | 'error';
export interface Toast {
  message: string;
  type: ToastType;
  header?: string;
  delay?: number;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private _translateService = inject(TranslateService);

  toasts = signal<Toast[]>([]);

  /**
   * Check if a message looks like a translation key (e.g., USER_LOGIN, COMMON_CREATE_ITEM)
   * Translation keys are typically ALL_CAPS_WITH_UNDERSCORES
   */
  private isTranslationKey(message: string): boolean {
    return /^[A-Z][A-Z0-9_]+$/.test(message);
  }

  /**
   * Translate message if it's a translation key, otherwise return as-is
   */
  private resolveMessage(message: string): string {
    if (this.isTranslationKey(message)) {
      const translated = this._translateService.instant(message);
      // If translation returns the same key, it means no translation found - return original
      return translated !== message ? translated : message;
    }
    return message;
  }

  show(message: string, type: ToastType, header?: string, delay = 5000) {
    const resolvedMessage = this.resolveMessage(message);
    setTimeout(() => {
      this.toasts.update(current => [...current, { message: resolvedMessage, type, header, delay }]);
    });
  }

  successToast(message: string, header?: string, delay = 5000) {
    this.show(message, 'success', header, delay);
  }

  warningToast(message: string, header?: string, delay = 5000) {
    this.show(message, 'warning', header, delay);
  }

  errorToast(message: string, header?: string, delay = 5000) {
    this.show(message, 'error', header, delay);
  }

  remove(toast: Toast) {
    this.toasts.update(current => current.filter(t => t !== toast));
  }
}
