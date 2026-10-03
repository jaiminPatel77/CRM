import { getTestBed, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, ActivatedRoute, convertToParamMap } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { BaseDetailPopupComponent } from './base-detail-popup.component';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

@Component({ template: '' })
class DummyComponent {}

describe('BaseDetailPopupComponent', () => {
  let component: BaseDetailPopupComponent<any>;
  let fixture: ComponentFixture<BaseDetailPopupComponent<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TranslateModule.forRoot(), BaseDetailPopupComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: '**', component: DummyComponent }]),
        { provide: 'baseUrl', useValue: 'api/v1/test' },
        { provide: NgbActiveModal, useValue: { close: vi.fn(), dismiss: vi.fn() } },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { queryParams: {}, paramMap: convertToParamMap({}) },
            queryParams: of({}),
            paramMap: of(convertToParamMap({}))
          }
        }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent<BaseDetailPopupComponent<any>>(BaseDetailPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
