import { getTestBed, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgbModal, NgbPaginationModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { of } from 'rxjs';

import { UserListComponent } from './user-list.component';
import { UserService } from '../../../auth/services/user.service';
import { ToastService } from "@shared/services/toast.service";
import { ColumnSorterComponent } from '@shared/components/column-sorter/column-sorter.component';
import { LoadingComponent } from '@shared/components/loading/loading.component';
import { EnumUserStatus, EnumPermissionFor } from '@shared/models/common-enums';
import { UserListDto } from '../../models/user-dto';
import { vi } from 'vitest';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

@Component({ template: '' })
class DummyComponent {}

describe('UserListComponent', () => {
  let component: UserListComponent;
  let fixture: ComponentFixture<UserListComponent>;
  let userServiceSpy: any;
  let routerSpy: any;
  let toastServiceSpy: any;
  let modalServiceSpy: any;

  const mockUsers: UserListDto[] = [
    { id: 1, email: 'user1@example.com', fullName: 'User One', status: EnumUserStatus.Active },
    { id: 2, email: 'user2@example.com', fullName: 'User Two', status: EnumUserStatus.Disabled }
  ];

  beforeEach(async () => {
    userServiceSpy = {
      adminInviteUser: vi.fn(),
      showSuccessIdToast: vi.fn(),
      showApiErrorToast: vi.fn(),
      getList: vi.fn(),
      baseUrl: 'api/v1/Users'
    };
    
    routerSpy = { navigate: vi.fn() };
    toastServiceSpy = { successToast: vi.fn(), errorToast: vi.fn() };
    modalServiceSpy = { open: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [
        UserListComponent,
        TranslateModule.forRoot(),
        FormsModule,
        NgbPaginationModule,
        NgbTooltipModule,
        LoadingComponent,
        ColumnSorterComponent
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: '**', component: DummyComponent }]),
        { provide: UserService, useValue: userServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ToastService, useValue: toastServiceSpy },
        { provide: NgbModal, useValue: modalServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { queryParams: {} },
            queryParams: of({})
          }
        }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(UserListComponent);
    component = fixture.componentInstance;
    component.sourceInfo.list = [...mockUsers];
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
    expect(component.requiredPermissionType).toBe(EnumPermissionFor.USER);
  });

  it('should update search filter on search click', () => {
    const searchTxt = 'testuser';
    component.searchTxt.set(searchTxt);
    
    const filterSpy = vi.spyOn(component.sourceInfo.filters, 'push');
    component.onSearch();
    
    expect(component.sourceInfo.filters.some(f => f.filterVal === searchTxt)).toBe(true);
  });

  it('should clear search filter when empty', () => {
    component.searchTxt.set('');
    component.onSearch();
    expect(component.searchTxt()).toBe('');
  });

  it('should handle user invitation successfully', () => {
    const userToInvite = mockUsers[0];
    userServiceSpy.adminInviteUser.mockReturnValue(of({ eventMessageId: 'INVITE_SUCCESS' }));
    
    component.onInviteUser(userToInvite);
    
    expect(userServiceSpy.adminInviteUser).toHaveBeenCalledWith(userToInvite.id!);
    expect(userToInvite.status).toBe(EnumUserStatus.Invited);
    expect(userServiceSpy.showSuccessIdToast).toHaveBeenCalledWith('INVITE_SUCCESS');
  });

  it('should restore search filter values from sourceInfo', () => {
    const sourceInfo = {
      filters: [{ filterName: 'email', filterVal: 'saved-search' }]
    } as any;
    
    component.setFilterValuesToModel(sourceInfo);
    expect(component.searchTxt()).toBe('saved-search');
  });
});
