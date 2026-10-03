import { getTestBed, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router, ActivatedRoute, convertToParamMap } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { SetPasswordComponent } from './set-password.component';
import { AuthService } from '../../services/auth.service';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

@Component({ template: '' })
class DummyComponent {}

describe('SetPasswordComponent', () => {
  let component: SetPasswordComponent;
  let fixture: ComponentFixture<SetPasswordComponent>;
  let authServiceSpy: any;

  beforeEach(async () => {
    authServiceSpy = { resetPassword: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [
        ReactiveFormsModule,
        TranslateModule.forRoot(),
        SetPasswordComponent
      ],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { queryParams: {}, paramMap: convertToParamMap({ code: 'test-code' }) },
            queryParams: of({}),
            paramMap: of(convertToParamMap({ code: 'test-code' }))
          }
        },
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: '**', component: DummyComponent }]),
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SetPasswordComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
