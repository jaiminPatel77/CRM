import { getTestBed, TestBed, ComponentFixture, fakeAsync, tick } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router, ActivatedRoute, convertToParamMap } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { of, throwError, BehaviorSubject, Subject } from 'rxjs';
import { NgbModal, NgbPaginationModule, NgbTooltipModule, NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { NgSelectModule } from '@ng-select/ng-select';
import { ToastService } from '@shared/services/toast.service';
import { CommonService } from '@shared/services/common.service';
import { UserPermissionService } from '@shared/services/user-permission.service';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';
import { vi } from 'vitest';
import { FocusFirstInvalidFieldDirective } from './focus-first-invalid-field.directive';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

@Component({ template: '' })
class DummyComponent {}

describe('FocusFirstInvalidFieldDirective', () => {
  it('should create an instance', () => {
    const directive = new FocusFirstInvalidFieldDirective({ nativeElement: {} } as any);
    expect(directive).toBeTruthy();
  });
});
