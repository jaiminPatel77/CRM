import { Component, ErrorHandler, Injectable, inject, signal, TemplateRef, ViewChild, Provider, APP_INITIALIZER } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class GlobalErrorHandlerService implements ErrorHandler {
  private readonly router = inject(Router);
  private readonly _hasError = signal<boolean>(false);
  private readonly _error = signal<Error | null>(null);

  readonly hasError = this._hasError.asReadonly();
  readonly error = this._error.asReadonly();

  handleError(error: Error): void {
    this._hasError.set(true);
    this._error.set(error);

    console.error('[GlobalErrorHandler]', error);

    if (!error.message.includes('Zone')) {
      this.router.navigate(['/auth'], {
        state: { error: error.message }
      });
    }
  }

  clearError(): void {
    this._hasError.set(false);
    this._error.set(null);
  }
}

export function provideErrorBoundary(): Provider {
  return {
    provide: ErrorHandler,
    useExisting: GlobalErrorHandlerService
  };
}

export const ErrorBoundaryProvider = provideErrorBoundary();

@Component({
  standalone: true,
  selector: 'app-error-boundary',
  template: `
    @if (errorService.hasError()) {
      <div class="error-boundary">
        <div class="error-content">
          <h2>Something went wrong</h2>
          <p>{{ errorService.error()?.message }}</p>
          <button (click)="retry()">Try Again</button>
        </div>
      </div>
    } @else {
      <ng-content></ng-content>
    }
  `,
  styles: [`
    .error-boundary {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 200px;
      padding: 2rem;
    }
    .error-content {
      text-align: center;
      background: var(--bs-danger-bg-subtle, #fee2e2);
      padding: 2rem;
      border-radius: 8px;
      border: 1px solid var(--bs-danger-border, #fecaca);
    }
    h2 {
      color: var(--bs-danger, #dc2626);
      margin-bottom: 1rem;
    }
    button {
      margin-top: 1rem;
      padding: 0.5rem 1rem;
      background: var(--bs-danger, #dc2626);
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
    }
    button:hover {
      background: var(--bs-danger-hover, #b91c1c);
    }
  `]
})
export class ErrorBoundaryComponent {
  readonly errorService = inject(GlobalErrorHandlerService);

  retry(): void {
    this.errorService.clearError();
  }
}