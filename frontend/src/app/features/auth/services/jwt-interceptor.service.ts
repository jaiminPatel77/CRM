import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';
import { AuthToken } from '../models/token';

@Injectable({
  providedIn: 'root'
})
export class JwtInterceptorService implements HttpInterceptor {

  protected _authService = inject(AuthService);

  constructor() {

  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    let token: AuthToken | undefined = this._authService.getCurrentToken;
    if (token && token.isValid()) {
      const JWT = `Bearer ${token.token()}`;
      this._authService.setIdleTimer(req);
      const headers: { [name: string]: string } = {
        Authorization: JWT
      };
      const tenantId = token.tenantId();
      if (tenantId) {
        headers['X-Tenant-ID'] = tenantId;
      }
      req = req.clone({
        setHeaders: headers
      });
    }
    return next.handle(req);
  }

}
