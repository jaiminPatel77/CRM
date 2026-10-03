import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { JwtInterceptorService } from '../../modules/auth/services/jwt-interceptor.service';

export const httpInterceptors = {
  provide: HTTP_INTERCEPTORS,
  useClass: JwtInterceptorService,
  multi: true
}