import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../../features/auth/services/auth.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const token = authService.getCurrentToken;

    if (token && token.isValid()) {
        authService.setIdleTimer(req);
        req = req.clone({
            setHeaders: {
                Authorization: `Bearer ${token.token()}`
            }
        });
    }

    return next(req);
};
