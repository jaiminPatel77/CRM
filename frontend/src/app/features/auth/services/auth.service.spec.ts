import { getTestBed, TestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter, Router, ActivatedRoute, convertToParamMap } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { AuthService } from './auth.service';
import { JwtResponse } from '../models/token';
import { environment } from '@env/environment';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

@Component({ template: '' })
class DummyComponent {}

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let routerSpy: { navigate: ReturnType<typeof vi.fn> };

  const mockJwtResponse: JwtResponse = {
    id: '1',
    access_token: 'header.payload.signature',
    refresh_token: 'refresh_token_abc',
    expires_in: 3600
  };

  beforeEach(() => {
    routerSpy = { navigate: vi.fn() };

    TestBed.configureTestingModule({
      imports: [TranslateModule.forRoot()],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { queryParams: {}, paramMap: convertToParamMap({}) },
            queryParams: of({}),
            paramMap: of(convertToParamMap({}))
          }
        },
        provideRouter([{ path: '**', component: DummyComponent }]),
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService,
        { provide: Router, useValue: routerSpy }
      ]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);

    localStorage.clear();
    sessionStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('setToken', () => {
    it('should store token in localStorage when rememberMe is true', () => {
      service.setToken(mockJwtResponse, true);
      const stored = localStorage.getItem('crm_auth_token');
      expect(stored).toBeTruthy();
      expect(JSON.parse(stored!)).toEqual(mockJwtResponse);
      expect(sessionStorage.getItem('crm_auth_token')).toBeFalsy();
    });

    it('should store token in sessionStorage when rememberMe is false', () => {
      service.setToken(mockJwtResponse, false);
      const stored = sessionStorage.getItem('crm_auth_token');
      expect(stored).toBeTruthy();
      expect(JSON.parse(stored!)).toEqual(mockJwtResponse);
      expect(localStorage.getItem('crm_auth_token')).toBeFalsy();
    });
  });

  describe('clearToken', () => {
    it('should remove token from all storages and navigate to /auth', () => {
      service.setToken(mockJwtResponse, true);
      service.clearToken();
      expect(localStorage.getItem('crm_auth_token')).toBeNull();
      expect(sessionStorage.getItem('crm_auth_token')).toBeNull();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth']);
    });
  });

  describe('login', () => {
    it('should call the login API and process the result', () => {
      const loginViewModel = { email: 'test@example.com', password: 'password', rememberMe: true, deviceId: '123' };

      service.login(loginViewModel as any).subscribe(res => {
        expect(res.data).toEqual(mockJwtResponse);
      });

      const req = httpMock.expectOne(`${environment.Setting.apiServiceUrl}/api/v1/auth/login`);
      expect(req.request.method).toBe('POST');
      req.flush({ data: mockJwtResponse });

      expect(localStorage.getItem('crm_auth_token')).toBeTruthy();
    });
  });

  describe('isAuthenticated', () => {
    it('should return false when no token exists', () => {
      (service as any)._checkSession = false;
      (service as any)._authJWTToken = undefined;
      service.isAuthenticated().subscribe(isAuthenticated => {
        expect(isAuthenticated).toBe(false);
      });
    });
  });
});
