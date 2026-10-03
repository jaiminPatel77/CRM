import { getTestBed, TestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter, ActivatedRoute, convertToParamMap } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';
import { LoggerService } from './logger.service';
import { environment } from '@env/environment';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

@Component({ template: '' })
class DummyComponent {}

describe('LoggerService', () => {
  let service: LoggerService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    environment.Setting.remoteLogUrl = 'http://test-remote-log.com';

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
        LoggerService
      ]
    });
    service = TestBed.inject(LoggerService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should log error and attempt remote logging if configured', () => {
    service.error('Test Error');

    const req = httpMock.expectOne('http://test-remote-log.com');
    expect(req.request.method).toBe('POST');
    expect(req.request.body.message).toBe('Test Error');
    expect(req.request.body.level).toBe('Error');

    req.flush({});
  });

  it('should NOT attempt remote logging for info level', () => {
    service.info('Test Info');
    httpMock.expectNone('http://test-remote-log.com');
  });
});
