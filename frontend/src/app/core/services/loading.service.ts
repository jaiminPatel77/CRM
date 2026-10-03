import { Injectable, signal } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class LoadingService {
    private _loading = signal<boolean>(false);
    public loading = this._loading.asReadonly();
    private _requestCount = 0;

    show() {
        this._requestCount++;
        if (this._requestCount > 0) {
            this._loading.set(true);
        }
    }

    hide() {
        this._requestCount--;
        if (this._requestCount <= 0) {
            this._requestCount = 0;
            this._loading.set(false);
        }
    }
}
