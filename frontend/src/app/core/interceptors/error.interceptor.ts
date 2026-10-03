import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { LoggerService } from '../services/logger.service';
import { HttpErrorUtil } from '../utils/http-error.util';
import { AuthService } from '../../features/auth/services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const logger = inject(LoggerService);
    const authService = inject(AuthService);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {

            const apiError = HttpErrorUtil.formatError(error);

            logger.error(`API Error: ${req.method} ${req.url}`, apiError);

            if (error.status === 401) {
                // Auto logout if 401
                logger.warn('Unauthorized access - logging out');
                authService.clearToken();
            }

            return throwError(() => apiError);
        })
    );
};
