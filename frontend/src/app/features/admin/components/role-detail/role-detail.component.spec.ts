import { getTestBed, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgSelectModule } from '@ng-select/ng-select';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { RoleDetailComponent } from './role-detail.component';
import { RoleService } from '../../../auth/services/role.service';
import { UserService } from '../../../auth/services/user.service';
import { ToastService } from '@shared/services/toast.service';
import { EnumPermissionFor } from '@shared/models/common-enums';
import { PermissionDto, RoleDto } from '../../models/role-dto';
import { UserLookUpDto } from '../../models/user-dto';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

@Component({ template: '' })
class DummyComponent {}

describe('RoleDetailComponent', () => {
  let component: RoleDetailComponent;
  let fixture: ComponentFixture<RoleDetailComponent>;
  let roleServiceSpy: any;
  let userServiceSpy: any;
  let routerSpy: any;
  let toastServiceSpy: any;

  const mockPermissions: PermissionDto[] = [
    { permissionFor: EnumPermissionFor.USER, viewAccess: true, createAccess: false, updateAccess: false, deleteAccess: false, allAccess: false },
    { permissionFor: EnumPermissionFor.ROLE, viewAccess: true, createAccess: true, updateAccess: true, deleteAccess: true, allAccess: true }
  ];

  const mockRole: RoleDto = {
    id: 1,
    name: 'Admin',
    description: 'Administrator role',
    users: [{ id: 1, fullName: 'User One' } as any],
    permissions: mockPermissions
  };

  const mockUsers: UserLookUpDto[] = [
    { id: 1, fullName: 'User One', email: 'user1@example.com' },
    { id: 2, fullName: 'User Two', email: 'user2@example.com' }
  ];

  beforeEach(async () => {
    roleServiceSpy = {
      getRecord: vi.fn().mockReturnValue(of({ data: mockRole })),
      createRecord: vi.fn().mockReturnValue(of({ eventMessageId: 'CREATE_SUCCESS' })),
      updateRecord: vi.fn().mockReturnValue(of({ eventMessageId: 'SAVE_SUCCESS' })),
      showApiErrorToast: vi.fn()
    };
    userServiceSpy = {
      getLookUpList: vi.fn().mockReturnValue(of(mockUsers)),
      showApiErrorToast: vi.fn()
    };
    routerSpy = { navigate: vi.fn() };
    toastServiceSpy = { successToast: vi.fn(), errorToast: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [
        RoleDetailComponent,
        ReactiveFormsModule,
        TranslateModule.forRoot(),
        NgSelectModule
      ],
      providers: [
        provideRouter([{ path: '**', component: DummyComponent }]), 
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RoleService, useValue: roleServiceSpy },
        { provide: UserService, useValue: userServiceSpy },
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
    }).compileComponents();

    fixture = TestBed.createComponent(RoleDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
    expect(component.requiredPermissionType).toBe(EnumPermissionFor.ROLE);
  });

  it('should load role and users on init', () => {
    expect(roleServiceSpy.getRecord).toHaveBeenCalledWith(1);
    expect(userServiceSpy.getLookUpList).toHaveBeenCalled();
    expect(component.model?.name).toBe(mockRole.name);
    expect(component.userList.length).toBe(mockUsers.length);
  });

  it('should initialize permissions FormArray', () => {
    expect(component.rolePermissions.length).toBeGreaterThan(0);
  });

  it('should call roleService.updateRecord on existing role submit', () => {
    if (component.detailForm) {
      component.detailForm.patchValue({ name: 'Admin', description: 'Desc' });
    }
    component.onSubmit();
    expect(roleServiceSpy.updateRecord).toHaveBeenCalled();
  });
});
