import { getTestBed, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideZonelessChangeDetection, NgModule, Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { provideStore } from '@ngrx/store';
import { of } from 'rxjs';
import { plainToInstance } from 'class-transformer';
import { BaseListStoreComponent } from './base-list-store.component';

@NgModule({ providers: [provideZonelessChangeDetection()] })
class GlobalTestSetupModule {}
try {
  getTestBed().initTestEnvironment([BrowserDynamicTestingModule, GlobalTestSetupModule], platformBrowserDynamicTesting());
} catch (e) {}

class TestModel {
  id: number = 1;
}

@Component({ template: '', standalone: true })
class TestBaseListStoreComponent extends BaseListStoreComponent<TestModel> {
  override get dispatchSelectorForGettingList(): any {
    return () => ({ type: '[Test] Get List' });
  }
  override get getStoreListSelector(): any {
    return () => of(plainToInstance(TestModel, [{ id: 1 }]));
  }
  override get getStoreListStatusSelector(): any {
    return () => of(1);
  }
  override get getStoreListTotalRecordSelector(): any {
    return () => of(1);
  }
  override get getStoreListRecordFilterSelector(): any {
    return () => of({});
  }
  override get getStoreListErrorSelector(): any {
    return () => of(null);
  }
}

@Component({ template: '' })
class DummyComponent {}

describe('BaseListStoreComponent', () => {
  let component: TestBaseListStoreComponent;
  let fixture: ComponentFixture<TestBaseListStoreComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TranslateModule.forRoot(), TestBaseListStoreComponent],
      providers: [
        provideStore({}),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: '**', component: DummyComponent }]),
        { provide: 'baseUrl', useValue: 'api/v1/test' }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TestBaseListStoreComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
