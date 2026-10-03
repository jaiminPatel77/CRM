import { Injectable } from '@angular/core';

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
    toasts: Toast[] = [];

    show(message: string, type: ToastType, header?: string, delay = 5000) {
        this.toasts.push({ message, type, header, delay });
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
        this.toasts = this.toasts.filter(t => t !== toast);
    }
}
