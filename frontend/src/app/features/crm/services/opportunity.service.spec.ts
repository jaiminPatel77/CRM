import { getTestBed, TestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, ActivatedRoute, convertToParamMap } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';
import { OpportunityService } from './opportunity.service';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

@Component({ template: '' })
class DummyComponent {}

describe('OpportunityService', () => {
  let service: OpportunityService;

  beforeEach(() => {
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
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: '**', component: DummyComponent }])
      ]
    });
    service = TestBed.inject(OpportunityService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
