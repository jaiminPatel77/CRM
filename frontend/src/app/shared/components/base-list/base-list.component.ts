import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { BaseDto } from '../../models/base-model';
import { BaseComponent } from '../base/base.component';
import { Observable, Subject, debounceTime, map, takeUntil } from 'rxjs';
import { DataStatusEnum, IBaseList } from '../../models/common-models';
import { ActivatedRoute, Router } from '@angular/router';
import { FilterModel, FilterStrWithOperatorModel, SortingModel, SourceInfo } from '../../models/table-conf';
import { ConstString } from '../../models/const-string';
import { ApiError, ApiOkResponse } from '../../models/api-response';
import { instanceToInstance } from 'class-transformer';
import { ConfirmDialogPopupComponent } from '../confirm-dialog-popup/confirm-dialog-popup.component';
import { ListHelper } from '../../models/list-helper';
import { HttpClient } from '@angular/common/http';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ToastService } from '../../../core/services/toast.service';
import { TranslateService } from '@ngx-translate/core';
import { ICrudService } from '../../interfaces/crud-service.interface';
import { environment } from '../../../../environments/environment';
import { BaseService } from '../../services/base.service';

@Component({
  selector: 'app-base-list',
  imports: [],
  templateUrl: './base-list.component.html',
  styleUrl: './base-list.component.scss'
})
export class BaseListComponent<T extends BaseDto> extends BaseComponent implements OnInit, OnDestroy {

  allowMultipleSelect: boolean = false;
  selectedRecord?: T;
  selectedRecordList?: T[];
  selectAll: boolean = false;
  protected destroy$ = new Subject<boolean>();

  sourceInfo: SourceInfo = new SourceInfo();

  // inject services
  protected _router = inject(Router);
  protected _activatedRoute = inject(ActivatedRoute);
  protected _http = inject(HttpClient);
  protected _modal = inject(NgbModal);
  protected _toastService = inject(ToastService);
  protected _translateService = inject(TranslateService);
  protected _cdr = inject(ChangeDetectorRef);

  constructor(
    service?: BaseService<any>) {
    super();
    this.selectedRecordList = [];
    if (service) {
      this.sourceInfo._service = service;
    }
  }

  //#region Init and destroy methods starts

  override ngOnInit() {
    super.ngOnInit();
    this.getDataList(this.sourceInfo);
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    if (this.destroy$) {
      this.destroy$.next(true);
      // This completes the subject property.
      this.destroy$.complete();
    }
  }

  //#endregion Init and destroy methods ends


  //#region set value from query param starts

  getDataList(sourceInfo: SourceInfo) {
    this.dataStatus = DataStatusEnum.Fetching;
    sourceInfo.gridDataStatus = DataStatusEnum.Fetching;
    this.addDefaultFilters(sourceInfo);
    this.applyFilterBeforeListCalled(sourceInfo);
    this.loadDataFromServerSubscription(sourceInfo);
    this.afterLoadedDataSubscription(sourceInfo);
    this.afterLoadedErrorSubscription(sourceInfo);
    sourceInfo.filterChanged.next('');
  }

  addDefaultFilters(sourceInfo: SourceInfo) {
    // override if needed
  }

  applyFilterBeforeListCalled(sourceInfo: SourceInfo) {
    let { page, pageSize, filter, orderBy } = this._activatedRoute.snapshot.queryParams;
    let queryParams: Record<string, any> = { page, pageSize, filter, orderBy };

    // set page and pageSize
    if (page && pageSize) {
      sourceInfo.pagination.pageNo = Number(page);
      sourceInfo.pagination.pageSize = Number(pageSize);
      sourceInfo.paginationQuery = ListHelper.getPaginationQuery(sourceInfo);
    }
    else {
      queryParams[ConstString.PageNoStr] = sourceInfo.pagination.pageNo;
      queryParams[ConstString.SizeStr] = sourceInfo.pagination.pageSize;
      sourceInfo.paginationQuery = ListHelper.getPaginationQuery(sourceInfo);
    }

    // set filter string
    if (filter?.trim()) {
      sourceInfo.filterQuery = `&${ConstString.FilterStr}=${filter}`;
      sourceInfo.filters = this.getFilters(filter);
      this.setFilterValuesToModel(sourceInfo);
    }
    else {
      if (sourceInfo.filters?.length) {
        let filterStr = ListHelper.getFilterStrFromFilters(sourceInfo);
        sourceInfo.filterQuery = `&${ConstString.FilterStr}=${filterStr}`;
        queryParams[ConstString.FilterStr] = filterStr;
      }
    }

    // set orderby string
    if (orderBy?.trim()) {
      sourceInfo.sortQuery = `&${ConstString.OrderByStr}=${orderBy}`;
      sourceInfo.sortingState = this.getSortingState(orderBy);
    }
    else {
      if (sourceInfo.sortingState.column.trim() !== '' && sourceInfo.sortingState.order.trim() !== '') {
        let sortingStr = ListHelper.getSortingStrFromSortingState(sourceInfo);
        sourceInfo.sortQuery = `&${ConstString.OrderByStr}=${sortingStr}`;
        queryParams[ConstString.OrderByStr] = sortingStr;
      }
    }

    this._router.navigate([], {
      relativeTo: this._activatedRoute,
      queryParams: queryParams,
    });
  }

  getFilters(filter: string): FilterModel[] {
    let filters: FilterModel[] = [];
    let totalFieldValueStringArray: FilterStrWithOperatorModel[] = [];
    this.getFieldValueStringArray(filter, totalFieldValueStringArray, '');
    for (let index = 0; index < totalFieldValueStringArray.length; index++) {
      const element = totalFieldValueStringArray[index];
      if (element) {
        let splittedElement = element.filterStr.split(',');
        if (splittedElement?.length > 2) {
          filters.push({ filterName: splittedElement[0], filterVal: splittedElement[2], operator: `,${splittedElement[1]},`, splitOp: element.splitOp });
        }
      }
    }
    return filters;
  }

  getFieldValueStringArray(filter: string, totalFieldValueStringArray: FilterStrWithOperatorModel[] = [], splitOp: string) {
    const andIndex = filter.indexOf(ConstString.AndSplit);
    if (andIndex !== -1) {
      let splittedArray = filter.split(ConstString.AndSplit);
      for (let index = 0; index < splittedArray.length; index++) {
        const element = splittedArray[index];
        this.getFieldValueStringArray(element, totalFieldValueStringArray, ConstString.AndSplit);
      }
    }
    else {
      const orIndex = filter.indexOf(ConstString.OrSplit);
      if (orIndex !== -1) {
        let splittedArray = filter.split(ConstString.OrSplit);
        for (let index = 0; index < splittedArray.length; index++) {
          const element = splittedArray[index];
          this.getFieldValueStringArray(element, totalFieldValueStringArray, ConstString.OrSplit);
        }
      }
      else {
        totalFieldValueStringArray.push({ filterStr: filter, splitOp: splitOp });
      }
    }
  }

  getSortingState(orderBy: string): SortingModel {
    let sortingState: SortingModel = { column: '', order: '' };
    if (orderBy.endsWith(ConstString.SortOrderDesc)) {
      let splittedArray = orderBy.split(ConstString.SortOrderDesc);
      if (splittedArray?.length) {
        sortingState.column = splittedArray[0];
        sortingState.order = ConstString.SortOrderDesc;
      }
    }
    else if (orderBy.endsWith(ConstString.SortOrderAsc)) {
      let splittedArray = orderBy.split(ConstString.SortOrderAsc);
      if (splittedArray?.length) {
        sortingState.column = splittedArray[0];
        sortingState.order = ConstString.SortOrderAsc;
      }
    }
    else {
      // do nothing
    }
    return sortingState;
  }

  setFilterValuesToModel(sourceInfo: SourceInfo) {
    // override if search txt exist
  }

  //#endregion query param related code ends


  //#region load list data starts

  getList(sourceInfo: SourceInfo): Observable<ApiOkResponse<any>> {
    // apiName can be empty (for default list) or e.g. 'lookup-list'
    const apiPath = sourceInfo.apiName?.trim() ? `/${sourceInfo.apiName.trim()}` : '';

    let url = `${apiPath}${sourceInfo.paginationQuery}${sourceInfo.filterQuery}${sourceInfo.sortQuery}`;
    console.log('----------------------');
    console.log(url);
    console.log('----------------------');
    // Using simple get here, interceptors handle auth and errors
    const fullUrl = `${sourceInfo._service?.baseUrl}${url}`;
    return this._http.get<ApiOkResponse<any>>(fullUrl);
  }

  loadDataFromServerSubscription(sourceInfo: SourceInfo) {
    sourceInfo.filterChanged.pipe(debounceTime(500), takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.getList(sourceInfo).pipe(map((response: any) => {
          // Gridify returns { count, data } not { totalRecords, items }
          sourceInfo.pagination.totalRecords = response?.data?.count ?? response?.data?.totalRecords ?? 0;
          return response.data?.data ?? response.data?.items ?? [];
        })).subscribe({
          next: (response: T[]) => {
            sourceInfo.afterLoadedData.next(response);
          },
          error: (error: ApiError) => {
            sourceInfo.afterLoadedError.next(error);
          }
        })
      }
    })
  }

  afterLoadedDataSubscription(sourceInfo: SourceInfo) {
    sourceInfo.afterLoadedData.pipe(takeUntil(this.destroy$)).subscribe({
      next: (data: T[]) => {
        this.makeDataAvailable(sourceInfo, data);
      }
    })
  }

  afterLoadedErrorSubscription(sourceInfo: SourceInfo) {
    sourceInfo.afterLoadedError.pipe(takeUntil(this.destroy$)).subscribe({
      next: (data: ApiError) => {
        this.makeDataAvailable(sourceInfo);
        this.showApiErrorToast(data);
      }
    })
  }

  makeDataAvailable(sourceInfo: SourceInfo, data: T[] = []) {
    sourceInfo.list = instanceToInstance<T[]>(data);
    sourceInfo.gridDataStatus = DataStatusEnum.DataAvailable;
    this.selectedRecordList = [];
    this.selectedRecord = undefined;
    this.selectAll = false;
    this.makeFinalDataAvailable();
  }

  makeFinalDataAvailable() {
    this.dataStatus = DataStatusEnum.DataAvailable;
    this._cdr.detectChanges();
  }

  //#endregion load list data ends


  //#region filter/sorting operation on list starts

  onClearFilter() {
    ListHelper.onClearFilter(this.sourceInfo, true, this._router, this._activatedRoute);
  }

  onClearSorting() {
    ListHelper.onClearSorting(this.sourceInfo, true, this._router, this._activatedRoute);
  }

  onRefresh() {
    ListHelper.onRefresh(this.sourceInfo);
  }

  pageChanged() {
    ListHelper.pageChanged(this.sourceInfo, true, this._router, this._activatedRoute);
  }

  onFilter(key: string, value: string, operator: string = ConstString.ContainsSplit, splitOperatorToBeAddedBefore: string = ConstString.AndSplit) {
    ListHelper.onFilter(this.sourceInfo, key, value, operator, splitOperatorToBeAddedBefore, true, this._router, this._activatedRoute);
  }

  onColumnSort(data: SortingModel) {
    ListHelper.onSorting(this.sourceInfo, true, this._router, this._activatedRoute);
  }

  onFilterMultipleFields(customFilterModel: FilterModel[], splitOperatorToBeAddedBefore: string = ConstString.AndSplit) {
    ListHelper.onFilterMultipleFields(this.sourceInfo, customFilterModel, splitOperatorToBeAddedBefore, true, this._router, this._activatedRoute);
  }

  //#endregion filter/sorting operation on list ends


  //#region open/delete/enable record related functions starts

  gotoDetail(record?: T) {
    if (record) {
      this._router.navigate(['.', record.id], { relativeTo: this._activatedRoute });
    } else {
      this._router.navigate(['.', '0'], { relativeTo: this._activatedRoute });
    }
  }

  onDeleteClick(record: T): void {
    const modalRef = this._modal.open(ConfirmDialogPopupComponent);
    if (modalRef) {
      const confirmDlg: ConfirmDialogPopupComponent = modalRef.componentInstance;
      this.setDeleteDialogInfo(confirmDlg);
      modalRef.result.then((result) => {
        if (record.id) {
          this.onDeleteRecord(record.id);
        }
      }, (reason) => {
        return false;
      });
    }
  }

  setDeleteDialogInfo(component: ConfirmDialogPopupComponent) {
    component.model.messageTextId = 'DELETE_CONFIRM';
  }

  onDeleteRecord(id: number): void {
    if (this.sourceInfo._service) {
      this.sourceInfo._service.deleteRecord(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result: any) => {
            if (result && result.eventMessageId) {
              this.showSuccessIdToast(result.eventMessageId);
            } else {
              this.showSuccessIdToast('DELETE_SUCCESS');
            }
            this.onRefresh();
          },
          error: (error: ApiError) => {
            this.showApiErrorToast(error);
          }
        }
        );
    }
  }

  //#endregion open/delete/enable record related functions ends


  //#region record selection related functions starts

  changeSelection(record: T) {
    record.$selected = !record.$selected;

    // single selection
    if (!this.allowMultipleSelect) {
      // remove pre-selection..
      if (record != this.selectedRecord) {
        if (this.selectedRecord) {
          this.selectedRecord.$selected = false;
        }
      }
      // Update selection.
      if (record.$selected) {
        this.selectedRecord = <T>record;
        // take a reference in array also so one can use this.selectedRecord or list it will work.
        this.selectedRecordList = [];
        this.selectedRecordList[0] = this.selectedRecord;
      } else {
        this.selectedRecord = undefined;
        this.selectedRecordList = [];
      }
    } else {
      // Multi-select case.
      if (this.selectedRecordList) {
        if (record.$selected) {
          // add to selected list only if not added already
          const index = this.selectedRecordList.findIndex(c => c === record);
          if (index < 0) {
            this.selectedRecordList.push(<T>record);
          }
          this.selectedRecord = <T>record;
        } else {
          const index = this.selectedRecordList.indexOf(<T>record);
          if (index >= 0) {
            this.selectedRecordList.splice(index, 1);
            if (this.selectedRecordList.length == 0) {
              this.selectedRecord = undefined;
            } else {
              this.selectedRecord = this.selectedRecordList[0];
            }
          }
        }
        this.selectAll = this.selectedRecordList.length === this.sourceInfo.list.length;
      }
    }
  }

  onSelectDeselectAll() {
    if (this.allowMultipleSelect) {
      this.selectAll = !this.selectAll;
      this.selectedRecordList = [];
      (<T[]>this.sourceInfo.list).forEach(element => {
        (<T>element).$selected = this.selectAll;
        if (this.selectAll) {
          this.selectedRecordList?.push(element);
        }
      });
      if (this.selectAll) {
        this.selectedRecord = this.selectedRecordList[0];
      } else {
        this.selectedRecord = undefined;
      }
    }
  }

  protected clearSelection() {
    this.selectedRecordList = [];
    this.selectAll = false;
    if (this.sourceInfo.list) {
      (<T[]>this.sourceInfo.list).forEach(element => {
        (<IBaseList>element).$selected = false;
      });
    }
  }

  //#endregion record selection related functions ends


  // TODO: need to be checked following region
  //#region rights related functions starts

  public get isOpenAllowed(): boolean {
    if (!this.allowMultipleSelect && this.selectedRecord) {
      return this.isViewAccess;
    } else if (this.allowMultipleSelect && this.selectedRecordList?.length === 1) {
      return this.isViewAccess;
    } else {
      return false;
    }
  }

  public get isDeleteAllowed(): boolean {
    if (!this.allowMultipleSelect && this.selectedRecord) {
      return this.isDeleteAccess;
    } else if (this.allowMultipleSelect && this.selectedRecordList?.length) {
      return this.isDeleteAccess;
    } else {
      return false;
    }
  }

  public get isCreateAccess(): boolean {
    return this.evaluatedPermission.createAccess;
  }

  public get isUpdateAccess(): boolean {
    return this.evaluatedPermission.updateAccess;
  }

  public get isViewAccess(): boolean {
    return this.evaluatedPermission.viewAccess;
  }

  public get isDeleteAccess(): boolean {
    return this.evaluatedPermission.deleteAccess;
  }

  //#endregion rights related functions ends

  // Toast Helpers
  showSuccessIdToast(titleTextId: string, body?: string) {
    let title = this._translateService.instant(titleTextId);
    this._toastService.successToast(title, body);
  }

  showApiErrorToast(apiError: ApiError) {
    let errorTitle: string;
    let errorDetail = apiError.errorDetail;
    if (apiError.eventMessageId) {
      errorTitle = this._translateService.instant(apiError.eventMessageId);
    } else if (apiError.errorMessage) {
      errorTitle = apiError.errorMessage;
    } else if (apiError.statusCode === 404) {
      errorTitle = this._translateService.instant('SERVICE_NOT_AVAILABLE_ERROR');
    } else if (apiError.statusCode === 401) {
      errorTitle = this._translateService.instant('USER_NOT_AUTHORIZED_ERROR');
    } else {
      errorTitle = this._translateService.instant('API_SERVICE_RETURN_UNKNOWN_ERROR');
    }

    this._toastService.errorToast(errorTitle, errorDetail);
  }
}
