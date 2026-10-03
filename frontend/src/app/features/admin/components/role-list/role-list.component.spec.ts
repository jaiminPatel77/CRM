import { getTestBed, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgbPaginationModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { of } from 'rxjs';

import { RoleListComponent } from './role-list.component';
import { RoleService } from '../../../auth/services/role.service';
import { ToastService } from "@shared/services/toast.service";
import { ColumnSorterComponent } from '@shared/components/column-sorter/column-sorter.component';
import { LoadingComponent } from '@shared/components/loading/loading.component';
import { EnumPermissionFor } from '@shared/models/common-enums';
import { RoleListDto } from '../../models/role-dto';
import { vi } from 'vitest';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

@Component({ template: '' })
class DummyComponent {}

describe('RoleListComponent', () => {
  let component: RoleListComponent;
  let fixture: ComponentFixture<RoleListComponent>;
  let roleServiceSpy: any;
  let routerSpy: any;
  let toastServiceSpy: any;

  const mockRoles: RoleListDto[] = [
    { id: 1, name: 'Admin', description: 'Administrator' },
    { id: 2, name: 'User', description: 'Standard User' }
  ];

  beforeEach(async () => {
    roleServiceSpy = {
      showSuccessIdToast: vi.fn(),
      showApiErrorToast: vi.fn(),
      getList: vi.fn(),
      baseUrl: 'api/v1/Roles'
    };
    
    routerSpy = { navigate: vi.fn() };
    toastServiceSpy = { successToast: vi.fn(), errorToast: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [
        RoleListComponent,
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
        { provide: RoleService, useValue: roleServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ToastService, useValue: toastServiceSpy },
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
    
    fixture = TestBed.createComponent(RoleListComponent);
    component = fixture.componentInstance;
    component.sourceInfo.list = [...mockRoles];
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
    expect(component.requiredPermissionType).toBe(EnumPermissionFor.ROLE);
  });

  it('should update search filter on search click', () => {
    const searchTxt = 'admin';
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

  it('should restore search filter from sourceInfo', () => {
    const sourceInfo = {
      filters: [{ filterName: 'name', filterVal: 'saved-role' }]
    } as any;
    
    component.setFilterValuesToModel(sourceInfo);
    expect(component.searchTxt()).toBe('saved-role');
  });
});
