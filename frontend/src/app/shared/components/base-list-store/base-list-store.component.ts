import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { BaseDto } from '../../models/base-model';
import { BaseComponent } from '../base/base.component';
import { Subject, takeUntil } from 'rxjs';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { BaseService } from '../../services/base.service';
import { Store } from '@ngrx/store';
import { EnumTypeOfProperty, FilterModel, FilterStrWithOperatorModel, SortingModelForStore } from '../../models/table-conf';
import { ConstString } from '../../models/const-string';
import { instanceToInstance } from 'class-transformer';
import { IBaseList } from '../../models/common-models';
import { ConfirmDialogPopupComponent } from '../confirm-dialog-popup/confirm-dialog-popup.component';

@Component({
    selector: 'app-base-list-store',
    imports: [],
    templateUrl: './base-list-store.component.html',
    styleUrl: './base-list-store.component.scss'
})
export class BaseListStoreComponent<T extends BaseDto> extends BaseComponent implements OnInit, OnDestroy {

  list: T[] = [];
  paginatedList: T[] = [];
  filteredList: T[] = [];
  totalRecords: number = 0;

  DEFAULT_PAGE_NO: number = 1;
  pageNo: number = this.DEFAULT_PAGE_NO;
  DEFAULT_SIZE: number = 10;
  pageSize: number = this.DEFAULT_SIZE;
  sortingState: SortingModelForStore = { column: '', order: '', typeOfProperty: EnumTypeOfProperty.String };
  sortQuery: string = '';
  filterQuery: string = '';
  filters: FilterModel[] = [];
  enumTypeOfProperty = EnumTypeOfProperty;

  allowMultipleSelect: boolean = false;
  selectedRecord?: T;
  selectedRecordList?: T[];
  selectAll: boolean = false;

  protected destroy$ = new Subject<boolean>();

  // inject services
  protected _router = inject(Router);
  protected _activatedRoute = inject(ActivatedRoute);
  protected _store = inject(Store);
  protected _baseService = inject(BaseService<any>);

  constructor() {
    super();
    this.selectedRecordList = [];
  }

  //#region Init and destroy methods starts

  override ngOnInit() {
    super.ngOnInit();
    this.applyFilterBeforeListCalled();
    this.initDispatch();
    this.loadInitialDataSubscription();
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

  applyFilterBeforeListCalled() {
    let { pageNo, size, filter, orderBy } = this._activatedRoute.snapshot.queryParams;
    let queryParams = { pageNo, size, filter, orderBy };

    // set pageNo and size
    if (pageNo && size) {
      this.pageNo = Number(pageNo);
      this.pageSize = Number(size);
    }
    else {
      (queryParams as any)[ConstString.PageNoStr] = this.pageNo;
      (queryParams as any)[ConstString.SizeStr] = this.pageSize;
    }

    // set filter string
    if (filter?.trim()) {
      this.filters = this.getFilters(filter);
      this.setFilterValuesToModel();
    }
    else {
      if (this.filters?.length) {
        let filterStr = this.getFilterStrFromFilters();
        (queryParams as any)[ConstString.FilterStr] = filterStr;
      }
    }

    // set orderby string
    if (orderBy?.trim()) {
      this.sortingState = this.getSortingState(orderBy);
    }
    else {
      if (this.sortingState.column.trim() !== '' && this.sortingState.order.trim() !== ''
        && this.sortingState.typeOfProperty) {
        let sortingStr = this.getSortingStrFromSortingState();
        (queryParams as any)[ConstString.OrderByStr] = sortingStr;
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

  setFilterValuesToModel() {
    // override if search txt exist
  }

  getFilterStrFromFilters(): string {
    let filterStr: string = '';
    for (let index = 0; index < this.filters.length; index++) {
      const element = this.filters[index];

      if (index === this.filters.length - 1) {
        element.splitOp = '';
      }

      if (index === 0) {
        filterStr = element.filterName + element.operator + element.filterVal + element.splitOp;
      }
      else {
        filterStr = filterStr + element.filterName + element.operator + element.filterVal + element.splitOp;
      }
    }
    return filterStr;
  }

  getSortingState(orderBy: string): SortingModelForStore {
    let sortingState: SortingModelForStore = { column: '', order: '', typeOfProperty: EnumTypeOfProperty.String };
    if (orderBy.includes(ConstString.SortOrderDesc)) {
      let splittedArray = orderBy.split(ConstString.SortOrderDesc);
      if (splittedArray?.length) {
        sortingState.column = splittedArray[0];
        sortingState.order = ConstString.SortOrderDesc;
        if (splittedArray?.length > 1) {
          let splittedTypeOfProperty = splittedArray[1].split(ConstString.customValueSep);
          if (splittedTypeOfProperty?.length > 1) {
            sortingState.typeOfProperty = Number(splittedTypeOfProperty[1]);
          }
        }
      }
    }
    else if (orderBy.includes(ConstString.SortOrderAsc)) {
      let splittedArray = orderBy.split(ConstString.SortOrderAsc);
      if (splittedArray?.length) {
        sortingState.column = splittedArray[0];
        sortingState.order = ConstString.SortOrderAsc;
        if (splittedArray?.length > 1) {
          let splittedTypeOfProperty = splittedArray[1].split(ConstString.customValueSep);
          if (splittedTypeOfProperty?.length > 1) {
            sortingState.typeOfProperty = Number(splittedTypeOfProperty[1]);
          }
        }
      }
    }
    else {
      // do nothing
    }
    return sortingState;
  }

  getSortingStrFromSortingState(): string {
    let sortingStr: string = '';
    sortingStr = `${this.sortingState.column}${this.sortingState.order}${ConstString.customValueSep}${this.sortingState.typeOfProperty}`;
    return sortingStr;
  }

  //#endregion set value from query param ends


  //#region list allocation starts

  initDispatch() {
    // Note: Store should be initialized in master component. ex. dashboard. Here we will use store list only.
    this._store.dispatch(this.dispatchSelectorForGettingList);
  }

  get dispatchSelectorForGettingList(): any {
    throw new Error("DispatchSelectorForGettingList Method must be override!!");
  }

  loadInitialDataSubscription() {
    this._store.select(this.getStoreListSelector).pipe(takeUntil(this.destroy$)).subscribe((item) => {
      if (Array.isArray(item)) {
        this.list = instanceToInstance<T[]>(item);
        this.filteredList = instanceToInstance<T[]>(item);
      } else {
        this.list = [];
        this.filteredList = [];
      }
      this.afterDataLoaded();
      this.makePaginationListRelatedData();
    });
    this._store.select(this.getStoreListStatusSelector).pipe(takeUntil(this.destroy$)).subscribe((item) => {
      this.dataStatus = item;
    });
    this._store.select(this.getStoreListErrorSelector).pipe(takeUntil(this.destroy$)).subscribe((item) => {
      if (item) {
        let errorMessage = item;
        this._baseService.showApiErrorToast(errorMessage);
      }
    });
  }

  get getStoreListSelector(): any {
    throw new Error("GetStoreListSelector Method must be override!!");
  }

  get getStoreListStatusSelector(): any {
    throw new Error("GetStoreListStatusSelector Method must be override!!");
  }

  get getStoreListErrorSelector(): any {
    throw new Error("GetStoreListErrorSelector Method must be override!!");
  }

  afterDataLoaded() {
    // override if you want to apply some filter
  }

  makePaginationListRelatedData() {
    this.resetSelectionValues();
    let list = instanceToInstance<T[]>(this.filteredList);
    this.totalRecords = list.length;
    let begin = ((this.pageNo - 1) * this.pageSize);
    let end = begin + this.pageSize;
    this.paginatedList = list.slice(begin, end);
  }

  //#endregion list allocation ends


  //#region filter/sorting operation on list starts

  onRefresh() {
    this.pageNo = this.DEFAULT_PAGE_NO;
    const queryParams: Params = { pageNo: this.pageNo };
    this.mergeUpdatedValueToQueryParam(queryParams);
    this._store.dispatch(this.dispatchSelectorForGettingList);
  }

  pageChanged() {
    const queryParams: Params = { pageNo: this.pageNo };
    this.mergeUpdatedValueToQueryParam(queryParams);
    this.makePaginationListRelatedData();
  }

  onFilter(key: string, value: string, operator: string = ConstString.ContainsSplit, splitOperatorToBeAddedBefore: string = ConstString.AndSplit) {
    this.updateFilters(key, value, operator, splitOperatorToBeAddedBefore);
    let filterStr = this.removeEmptyValueFromFilterQuery();
    this.pageNo = this.DEFAULT_PAGE_NO;
    const queryParams: Params = { pageNo: this.pageNo, filter: filterStr };
    this.mergeUpdatedValueToQueryParam(queryParams);
    this.afterDataLoaded();
    this.makePaginationListRelatedData();
  }

  updateFilters(key: string, value: string, operator: string = ConstString.ContainsSplit, splitOperatorToBeAddedBefore: string = ConstString.AndSplit) {
    let findFilter = this.filters.find((obj: FilterModel) => obj.filterName === key);
    if (findFilter) {
      findFilter.filterVal = value;
    }
    else {
      if (this.filters?.length) {
        this.filters[this.filters.length - 1].splitOp = splitOperatorToBeAddedBefore;
      }
      this.filters.push({ filterName: key, filterVal: value, operator: operator, splitOp: '' });
    }
  }

  removeEmptyValueFromFilterQuery() {
    this.filters = this.filters.filter(e => e.filterVal && e.filterVal.trim() !== '');
    let filterStr: string | undefined = undefined;
    if (this.filters?.length) {
      this.filters[this.filters?.length - 1].splitOp = '';
      filterStr = this.getFilterStrFromFilters();
    }
    return filterStr;
  }

  onClearFilter() {
    this.clearModelValuesOnFilter();
    this.pageNo = this.DEFAULT_PAGE_NO;
    const queryParams: Params = { filter: undefined, pageNo: this.pageNo };
    this.mergeUpdatedValueToQueryParam(queryParams);
    this.afterDataLoaded();
    this.makePaginationListRelatedData();
  }

  clearModelValuesOnFilter() {
    // override if necessary
  }

  onClearSorting() {
    this.clearModelValuesOnSorting();
    this.pageNo = this.DEFAULT_PAGE_NO;
    const queryParams: Params = { orderBy: undefined, pageNo: this.pageNo };
    this.mergeUpdatedValueToQueryParam(queryParams);
    this.afterDataLoaded();
    this.makePaginationListRelatedData();
  }

  clearModelValuesOnSorting() {
    this.sortingState = { column: '', order: '', typeOfProperty: EnumTypeOfProperty.String };
  }

  onColumnSort(data: SortingModelForStore) {
    this.pageNo = this.DEFAULT_PAGE_NO;
    this.onSortingList(data);
    let sortingStr: string | undefined = undefined;
    if (this.sortingState.column.trim() !== '' && this.sortingState.order.trim() !== ''
      && this.sortingState.typeOfProperty) {
      sortingStr = this.getSortingStrFromSortingState();
    }

    const queryParams: Params = { orderBy: sortingStr, pageNo: this.pageNo };
    this.mergeUpdatedValueToQueryParam(queryParams);
    this.makePaginationListRelatedData();
  }

  onSortingList(data: SortingModelForStore) {
    let column = data.column as keyof T;
    switch (data.typeOfProperty) {
      case EnumTypeOfProperty.String:
        this.filteredList.sort((a: T, b: T) => {
          const aValue = a[column] as string | undefined;
          const bValue = b[column] as string | undefined;
          // Handle cases where aValue or bValue might be undefined
          if (aValue !== undefined && bValue !== undefined) {
            return (data.order === ConstString.asc) ? aValue.localeCompare(bValue) : bValue.localeCompare(aValue);
          } else if (aValue !== undefined) {
            return 1; // Move aValue to a higher index
          } else if (bValue !== undefined) {
            return -1; // Move bValue to a higher index
          } else {
            return 0; // Both values are undefined, maintain order
          }
        });
        break;
      case EnumTypeOfProperty.Date:
        this.filteredList.sort((a: T, b: T) => {
          const aValue = a[column] as Date | undefined;
          const bValue = b[column] as Date | undefined;
          if (aValue && bValue) {
            // Compare the values based on the sorting order specified
            if (data.order === ConstString.asc) {
              return aValue.getTime() - bValue.getTime();
            } else {
              return bValue.getTime() - aValue.getTime();
            }
          } else if (aValue) {
            return 1; // Move aValue to a higher index
          } else if (bValue) {
            return -1; // Move bValue to a higher index
          } else {
            return 0; // Both values are undefined, maintain order
          }
        });
        break;
      case EnumTypeOfProperty.Number:
        this.filteredList.sort((a: T, b: T) => {
          const aValue = a[column] as number | undefined;
          const bValue = b[column] as number | undefined;
          if (aValue !== undefined && bValue !== undefined) {
            // Compare the values based on the sorting order specified
            if (data.order === ConstString.asc) {
              return aValue - bValue;
            } else {
              return bValue - aValue;
            }
          } else if (aValue !== undefined) {
            return 1; // Move aValue to a higher index
          } else if (bValue !== undefined) {
            return -1; // Move bValue to a higher index
          } else {
            return 0; // Both values are undefined, maintain order
          }
        });
        break;
      case EnumTypeOfProperty.Boolean:
        this.filteredList.sort((a: T, b: T) => {
          const aValue = a[column] as boolean | undefined;
          const bValue = b[column] as boolean | undefined;
          if (aValue !== undefined && bValue !== undefined) {
            // Compare the boolean values directly
            if (data.order === ConstString.asc) {
              return (aValue === bValue) ? 0 : aValue ? 1 : -1;
            } else {
              return (bValue === aValue) ? 0 : bValue ? 1 : -1;
            }
          } else if (aValue !== undefined) {
            return 1; // Move aValue to a higher index
          } else if (bValue !== undefined) {
            return -1; // Move bValue to a higher index
          } else {
            return 0; // Both values are undefined, maintain order
          }
        });
        break;
      default:
        throw new Error("Invalid column type");
    }
  }

  mergeUpdatedValueToQueryParam(queryParams: Params) {
    this._router?.navigate(
      [],
      {
        relativeTo: this._activatedRoute,
        queryParams,
        queryParamsHandling: 'merge',
      }
    );
  }

  //#endregion filter/sorting operation on list ends


  //#region record selection related functions starts

  resetSelectionValues() {
    this.selectedRecordList = [];
    this.selectedRecord = undefined;
    this.selectAll = false;
  }

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
        this.selectAll = this.selectedRecordList.length === this.paginatedList.length;
      }
    }
  }

  onSelectDeselectAll() {
    if (this.allowMultipleSelect) {
      this.selectAll = !this.selectAll;
      this.selectedRecordList = [];
      (<T[]>this.paginatedList).forEach(element => {
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

  clearSelection() {
    this.selectedRecordList = [];
    this.selectAll = false;
    if (this.paginatedList) {
      (<T[]>this.paginatedList).forEach(element => {
        (<IBaseList>element).$selected = false;
      });
    }
  }

  //#endregion record selection related functions ends


  //#region open/delete/enable record related functions starts

  onDeleteClick(record: T): void {
    const modalRef = this._baseService?.modal.open(ConfirmDialogPopupComponent);
    if (modalRef) {
      const confirmDlg: ConfirmDialogPopupComponent = modalRef.componentInstance;
      this.setDeleteDialogInfo(confirmDlg);
      modalRef.result.then((result) => {
        if (record.id) {
          this.onDeleteRecord(record);
        }
      }, (reason) => {
        return false;
      });
    }
  }

  setDeleteDialogInfo(component: ConfirmDialogPopupComponent) {
    component.model.messageTextId = 'DELETE_CONFIRM';
  }

  onDeleteRecord(record: T) {
    throw new Error("onDeleteRecord Method must be override!!");
  }

  //#endregion open/delete/enable record related functions ends
}