import { ICrudService } from "../interfaces/crud-service.interface";
import { Subject } from "rxjs";
import { DataStatusEnum } from "./common-models";
import { BaseService } from "../services/base.service";

export enum EnumConditionalOperator {
    Equal = 1, //Eq
    NotEqual = 2, //Ne
    GreaterThan = 3, //Gt
    GreaterThanOrEqual = 4, //Ge
    LessThan = 5, //Lt
    LessThanOrEqual = 6, //Le
    Contains = 7, // Contains
    NotContains = 8, // NotContains
    StartsWith = 9, //StartWith
    EndsWith = 10, //EndWith
    Custom = 11 //Custom filter.
}

export interface FilterModel {
    filterName: string;
    filterVal: string;
    operator: string;
    splitOp: string
}

export class Pagination {
    constructor(
        public pageNo: number = 1,
        public pageSize: number = 10,
        public totalRecords: number = 0
    ) { }
}

export interface SortingModel {
    column: string;
    order: string;
}

export enum EnumTypeOfProperty {
    Date = 1,
    Number = 2,
    String = 3,
    Boolean = 4
}

export interface SortingModelForStore {
    column: string;
    order: string;
    typeOfProperty: EnumTypeOfProperty;
}

export interface FilterStrWithOperatorModel {
    filterStr: string;
    splitOp: string
}

export class SourceInfo {
    _service?: ICrudService<any>;
    list: any[] = [];
    apiName: string = '';
    filters: FilterModel[] = [];
    filterChanged: Subject<any> = new Subject<any>();
    pagination: Pagination = new Pagination();
    gridDataStatus: DataStatusEnum = DataStatusEnum.Fetching;
    sortingState: SortingModel = { column: '', order: '' };
    sortQuery: string = '';
    filterQuery: string = '';
    paginationQuery: string = '';
    afterLoadedData: Subject<any> = new Subject<any>();
    afterLoadedError: Subject<any> = new Subject<any>();
}