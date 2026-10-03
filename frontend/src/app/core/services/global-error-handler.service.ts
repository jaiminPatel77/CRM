import { ErrorHandler, Injectable, Injector, inject } from '@angular/core';
import { LoggerService } from './logger.service';
import { ToastService } from './toast.service';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {

    private _logger = inject(LoggerService);
    private _injector = inject(Injector); // Use Injector to avoid Cyclic Dependency if ToastService uses HttpClient

    handleError(error: any): void {
        const toastService = this._injector.get(ToastService);

        let message = 'An unexpected error occurred.';
        let stackTrace = error;

        if (error instanceof HttpErrorResponse) {
            // Http Errors are usually handled by Interceptor, but if one slips through:
            message = error.message;
            stackTrace = JSON.stringify(error);
        } else if (error instanceof Error) {
            message = error.message;
            stackTrace = error.stack;
        }

        this._logger.error('Unhandled Exception:', stackTrace);

        // Optional: Only show toast for non-http errors if interceptor is catching http ones
        // For now, we show generic error
        toastService.errorToast(message, 'Application Error');

        // Rethrow if you want standard Angular error reporting in console too
        // throw error; 
    }
}
