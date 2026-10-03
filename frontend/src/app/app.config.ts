import { ApplicationConfig, ErrorHandler, importProvidersFrom, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter, withPreloading } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { PreloadingStrategyService } from './services/preloading-strategy.service';
import { translateModule } from './models/application-configurations/ngx-translate-config';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { loggingInterceptor } from './core/interceptors/logging.interceptor';
import { GlobalErrorHandler } from './core/services/global-error-handler.service';
import { ErrorBoundaryProvider } from './shared/components/error-boundary/error-boundary.component';
import { ToastService } from './core/services/toast.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes, withPreloading(PreloadingStrategyService)),
    provideHttpClient(
      withFetch(),
      withInterceptors([jwtInterceptor, errorInterceptor, loadingInterceptor, loggingInterceptor])
    ),
    importProvidersFrom(translateModule),
    ErrorBoundaryProvider,
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    ToastService
  ]
};