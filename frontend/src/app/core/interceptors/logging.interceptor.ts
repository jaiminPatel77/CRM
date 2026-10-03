import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '@env/environment';
import { tap } from 'rxjs/operators';

const isDevelopment = () => !environment.production;

export const loggingInterceptor: HttpInterceptorFn = (req, next) => {
  if (!isDevelopment()) {
    return next(req);
  }

  const startTime = performance.now();
  const method = req.method;
  const url = req.url;

  return next(req).pipe(
    tap({
      next: (event) => {
        if (event instanceof HttpResponse) {
          const duration = Math.round(performance.now() - startTime);
          const status = event.status;
          const statusColor = status >= 400 ? '🔴' : '🟢';

          console.log(
            `%c[HTTP] ${statusColor} ${method} ${url} ${status} ${duration}ms`,
            'color: #10b981; font-weight: bold'
          );
        }
      },
      error: (error) => {
        const duration = Math.round(performance.now() - startTime);
        console.error(
          `%c[HTTP] 🔴 ${method} ${url} ERROR ${duration}ms`,
          'color: #ef4444; font-weight: bold',
          error
        );
      }
    })
  );
};
