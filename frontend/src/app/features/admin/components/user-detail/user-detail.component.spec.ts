import '@angular/localize/init';
import { getTestBed, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgSelectModule } from '@ng-select/ng-select';
import { of, throwError } from 'rxjs';

import { UserDetailComponent } from './user-detail.component';
import { UserService } from '../../../auth/services/user.service';
import { RoleService } from '../../../auth/services/role.service';
import { ToastService } from "@shared/services/toast.service";
import { UserDto } from '../../models/user-dto';
import { RoleLookUpDto } from '../../models/role-dto';
import { EnumPermissionFor } from '@shared/models/common-enums';
import { vi } from 'vitest';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

@Component({ template: '' })
class DummyComponent {}

describe('UserDetailComponent', () => {
  let component: UserDetailComponent;
  let fixture: ComponentFixture<UserDetailComponent>;
  let userServiceSpy: any;
  let roleServiceSpy: any;
  let routerSpy: any;
  let toastServiceSpy: any;

  const mockUser: UserDto = {
    id: 1,
    email: 'test@example.com',
    fullName: 'Test User',
    title: 'Manager',
    phoneNumber: '1234567890',
    roles: ['Admin']
  };

  const mockRoles: RoleLookUpDto[] = [
    { id: 1, name: 'Admin', description: 'Administrator' },
    { id: 2, name: 'User', description: 'Standard User' }
  ];

  beforeEach(async () => {
    userServiceSpy = {
      getRecord: vi.fn().mockReturnValue(of({ data: mockUser })),
      update: vi.fn(),
      showApiErrorToast: vi.fn()
    };
    roleServiceSpy = {
      getLookUpList: vi.fn().mockReturnValue(of(mockRoles)),
      showApiErrorToast: vi.fn()
    };
    routerSpy = { navigate: vi.fn() };
    toastServiceSpy = { successToast: vi.fn(), errorToast: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [
        UserDetailComponent,
        ReactiveFormsModule,
        TranslateModule.forRoot(),
        NgSelectModule
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: '**', component: DummyComponent }]),
        { provide: UserService, useValue: userServiceSpy },
        { provide: RoleService, useValue: roleServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ToastService, useValue: toastServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: { get: () => '1' }
            }
          }
        }
      ]
    })
      .compileComponents();

    fixture = TestBed.createComponent(UserDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
    expect(component.requiredPermissionType).toBe(EnumPermissionFor.USER);
  });

  it('should load user and roles on init', () => {
    expect(userServiceSpy.getRecord).toHaveBeenCalledWith(1);
    expect(roleServiceSpy.getLookUpList).toHaveBeenCalled();
    expect(component.model?.email).toBe(mockUser.email);
    expect(component.roleList.length).toBe(mockRoles.length);
  });

  it('should initialize form with user data', () => {
    expect(component.detailForm?.get('email')?.value).toBe(mockUser.email);
    expect(component.detailForm?.get('fullName')?.value).toBe(mockUser.fullName);
    const selectedRoles = component.detailForm?.get('roles')?.value as RoleLookUpDto[];
    expect(selectedRoles[0].name).toBe('Admin');
  });

  it('should validate email format', () => {
    const email = component.email;
    email?.setValue('invalid-email');
    expect(email?.valid).toBe(false);

    email?.setValue('valid@example.com');
    expect(email?.valid).toBe(true);
  });

  it('should call userService.update on submit', () => {
    userServiceSpy.update.mockReturnValue(of({ eventMessageId: 'SAVE_SUCCESS' }));
    if (component.detailForm) {
      component.detailForm.patchValue({
        email: 'test@example.com',
        fullName: 'Test User',
        title: 'Manager',
        phoneNumber: '1234567890'
      });
    }
    component.onSubmit();
    expect(userServiceSpy.update).toHaveBeenCalled();
  });

  it('should handle error when loading user', () => {
    userServiceSpy.getRecord.mockReturnValue(throwError(() => ({ errorMessage: 'Not Found' })));
    component.getRecord();
    expect(component.dataStatus).toBe(3);
  });
});
