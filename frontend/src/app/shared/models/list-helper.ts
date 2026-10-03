import { ActivatedRoute, Params, Router } from "@angular/router";
import { FilterModel, SourceInfo } from "./table-conf";
import { DataStatusEnum } from "./common-models";
import { ConstString } from "./const-string";

export class ListHelper {

    //#region filter/sorting operation on list starts

    static onClearFilter(sourceInfo: SourceInfo, needToUpdateActivateRoute: boolean = true, _router?: Router, _activatedRoute?: ActivatedRoute) {
        sourceInfo.filters = [];
        sourceInfo.filterQuery = '';
        if (needToUpdateActivateRoute) {
            sourceInfo.pagination.pageNo = 1;
            this.updatePaginationQuery(sourceInfo);
            const queryParams: Params = { pageNo: sourceInfo.pagination.pageNo, filter: undefined };
            _router?.navigate(
                [],
                {
                    relativeTo: _activatedRoute,
                    queryParams,
                    queryParamsHandling: 'merge',
                }
            );
        }
        this.onRefresh(sourceInfo);
    }

    static onClearSorting(sourceInfo: SourceInfo, needToUpdateActivateRoute: boolean = true, _router?: Router, _activatedRoute?: ActivatedRoute) {
        sourceInfo.sortingState = { column: '', order: '' };
        sourceInfo.sortQuery = '';
        if (needToUpdateActivateRoute) {
            sourceInfo.pagination.pageNo = 1;
            this.updatePaginationQuery(sourceInfo);
            const queryParams: Params = { pageNo: sourceInfo.pagination.pageNo, orderBy: undefined };
            _router?.navigate(
                [],
                {
                    relativeTo: _activatedRoute,
                    queryParams,
                    queryParamsHandling: 'merge',
                }
            );
        }
        this.onRefresh(sourceInfo);
    }

    static onRefresh(sourceInfo: SourceInfo) {
        sourceInfo.gridDataStatus = DataStatusEnum.Fetching;
        sourceInfo.filterChanged.next('');
    }

    static pageChanged(sourceInfo: SourceInfo, needToUpdateActivateRoute: boolean = true, _router?: Router, _activatedRoute?: ActivatedRoute) {
        this.updatePaginationQuery(sourceInfo);
        if (needToUpdateActivateRoute) {
            const queryParams: Params = { pageNo: sourceInfo.pagination.pageNo };
            _router?.navigate(
                [],
                {
                    relativeTo: _activatedRoute,
                    queryParams,
                    queryParamsHandling: 'merge',
                }
            );
        }
        this.onRefresh(sourceInfo);
    }

    static updatePaginationQuery(sourceInfo: SourceInfo) {
        sourceInfo.paginationQuery = ListHelper.getPaginationQuery(sourceInfo);
    }

    static getPaginationQuery(sourceInfo: SourceInfo): string {
        return `?${ConstString.PageNoStr}=${sourceInfo.pagination.pageNo}&${ConstString.SizeStr}=${sourceInfo.pagination.pageSize}`;
    }

    static onFilter(sourceInfo: SourceInfo, key: string, value: string, operator: string = ConstString.ContainsSplit, splitOperatorToBeAddedBefore: string = ConstString.AndSplit, needToUpdateActivateRoute: boolean = true, _router?: Router, _activatedRoute?: ActivatedRoute) {
        let findFilter = sourceInfo.filters.find((obj: FilterModel) => obj.filterName === key);
        if (findFilter) {
            findFilter.filterVal = value;
        }
        else {
            if (sourceInfo.filters?.length) {
                sourceInfo.filters[sourceInfo.filters.length - 1].splitOp = splitOperatorToBeAddedBefore;
            }
            sourceInfo.filters.push({ filterName: key, filterVal: value, operator: operator, splitOp: '' });
        }
        ListHelper.removeEmptyValueFromFilterQuery(sourceInfo, needToUpdateActivateRoute, _router, _activatedRoute);
        this.onRefresh(sourceInfo);
    }

    static onFilterMultipleFields(sourceInfo: SourceInfo, customFilterModel: FilterModel[] = [], splitOperatorToBeAddedBefore: string = ConstString.AndSplit, needToUpdateActivateRoute: boolean = true, _router?: Router, _activatedRoute?: ActivatedRoute) {
        // Removes filters from sourceInfo.filters whose filterName matches any filterName in customFilterModel
        sourceInfo.filters = sourceInfo.filters.filter(e => !customFilterModel.some(e1 => e1.filterName === e.filterName));
        if (sourceInfo.filters?.length) {
            sourceInfo.filters[sourceInfo.filters.length - 1].splitOp = splitOperatorToBeAddedBefore;
        }
        for (let index = 0; index < customFilterModel.length; index++) {
            const element = customFilterModel[index];
            sourceInfo.filters.push(element);
        }
        ListHelper.removeEmptyValueFromFilterQuery(sourceInfo, needToUpdateActivateRoute, _router, _activatedRoute);
        this.onRefresh(sourceInfo);
    }

    static removeEmptyValueFromFilterQuery(sourceInfo: SourceInfo, needToUpdateActivateRoute: boolean = true, _router?: Router, _activatedRoute?: ActivatedRoute) {
        sourceInfo.filters = sourceInfo.filters.filter(e => e.filterVal && e.filterVal.trim() !== '');
        let filterStr: string | undefined = undefined;
        if (sourceInfo.filters?.length) {
            sourceInfo.filters[sourceInfo.filters?.length - 1].splitOp = '';
            filterStr = ListHelper.getFilterStrFromFilters(sourceInfo);
            sourceInfo.filterQuery = `&${ConstString.FilterStr}=${filterStr}`;
        }
        else {
            sourceInfo.filterQuery = '';
        }

        if (needToUpdateActivateRoute) {
            sourceInfo.pagination.pageNo = 1;
            this.updatePaginationQuery(sourceInfo);
            const queryParams: Params = { pageNo: sourceInfo.pagination.pageNo, filter: filterStr };
            _router?.navigate(
                [],
                {
                    relativeTo: _activatedRoute,
                    queryParams,
                    queryParamsHandling: 'merge',
                }
            );
        }
    }

    static getFilterStrFromFilters(sourceInfo: SourceInfo): string {
        let filterStr: string = '';
        let orGroup: string[] = [];
        let inOrGroup = false;

        for (let index = 0; index < sourceInfo.filters.length; index++) {
            const element = sourceInfo.filters[index];

            if (index === sourceInfo.filters.length - 1) {
                element.splitOp = '';
            }

            // Build Gridify-compatible filter string
            // For contains (=*), format is: field=*value*
            // For equals (=), format is: field=value
            let filterPart = '';
            if (element.operator === ConstString.ContainsSplit) {
                filterPart = `${element.filterName}=*${element.filterVal}*`;
            } else if (element.operator === ConstString.EqSplit) {
                filterPart = `${element.filterName}=${element.filterVal}`;
            } else {
                // Legacy format fallback
                filterPart = element.filterName + element.operator + element.filterVal;
            }

            // Handle OR grouping - Gridify requires parentheses around OR conditions
            if (element.splitOp === ConstString.OrSplit) {
                orGroup.push(filterPart);
                inOrGroup = true;
            } else if (inOrGroup) {
                // End of OR group - add current element and close group
                orGroup.push(filterPart);
                filterPart = '(' + orGroup.join('|') + ')';
                orGroup = [];
                inOrGroup = false;

                // Add separator if there's a next element
                const separator = element.splitOp === ConstString.AndSplit ? ',' : '';
                filterStr += filterPart + separator;
            } else {
                // Regular AND condition
                const separator = element.splitOp === ConstString.AndSplit ? ',' : '';
                filterStr += filterPart + separator;
            }
        }

        // If we ended while still in an OR group  
        if (orGroup.length > 0) {
            filterStr += '(' + orGroup.join('|') + ')';
        }

        return filterStr;
    }

    static onSorting(sourceInfo: SourceInfo, needToUpdateActivateRoute: boolean = true, _router?: Router, _activatedRoute?: ActivatedRoute) {
        let sortingStr: string | undefined = undefined;
        if (sourceInfo.sortingState.column.trim() !== '' && sourceInfo.sortingState.order.trim() !== '') {
            sortingStr = this.getSortingStrFromSortingState(sourceInfo);
            sourceInfo.sortQuery = `&${ConstString.OrderByStr}=${sortingStr}`;
        }
        else {
            sourceInfo.sortQuery = '';
        }

        if (needToUpdateActivateRoute) {
            sourceInfo.pagination.pageNo = 1;
            this.updatePaginationQuery(sourceInfo);
            const queryParams: Params = { pageNo: sourceInfo.pagination.pageNo, orderBy: sortingStr };
            _router?.navigate(
                [],
                {
                    relativeTo: _activatedRoute,
                    queryParams,
                    queryParamsHandling: 'merge',
                }
            );
        }
        this.onRefresh(sourceInfo);
    }

    static getSortingStrFromSortingState(sourceInfo: SourceInfo): string {
        let sortingStr: string = '';
        sortingStr = `${sourceInfo.sortingState.column}${sourceInfo.sortingState.order}`;
        return sortingStr;
    }

    //#endregion filter/sorting operation on list ends

}