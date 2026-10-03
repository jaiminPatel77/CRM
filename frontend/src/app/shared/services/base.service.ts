import { Inject, Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { UserPermissionService } from './user-permission.service';
import { Observable, map } from 'rxjs';
import { ApiCreatedResponse, ApiError, ApiOkResponse } from '../models/api-response';
import { CommonService } from './common.service';
import { DataQueryResult } from '../models/data-query-helper';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BaseService<A extends object = object> {

  public static readonly API_V1: string = `${environment.Setting.apiServiceUrl}/api/v1`;

  public static readonly ApiUrls = {
    Auth: `${BaseService.API_V1}/auth`,
    Account: `${BaseService.API_V1}/account`,
    ExternalAuth: `${BaseService.API_V1}/externalauth`,
    Role: `${BaseService.API_V1}/role`,
    User: `${BaseService.API_V1}/user`,
    Setting: `${BaseService.API_V1}/setting`,
    Project: `${BaseService.API_V1}/project`,
    Task: `${BaseService.API_V1}/task`,
    AuditLog: `${BaseService.API_V1}/auditlog`
  } as const;

  private readonly _commonService = inject(CommonService);

  constructor(
    @Inject('baseUrl') public readonly baseUrl: string
  ) {}

  //#region Access to common services

  public get http(): HttpClient {
    return this._commonService.http;
  }

  public get modal(): NgbModal {
    return this._commonService.modalService;
  }

  public get permissionService(): UserPermissionService {
    return this._commonService.permissionService;
  }

  //#endregion

  //#region Generic HTTP wrappers

  /**
   * Make actual HTTP GET call and return success/error response of given type.
   */
  getResponse<T>(baseUrl: string, subUrl: string): Observable<ApiOkResponse<T>> {
    return this._commonService.extractOkResponse(
      this._commonService.http.get<ApiOkResponse<T>>(baseUrl + subUrl)
    );
  }

  /**
   * Make actual HTTP POST call and return success/error response of given type.
   */
  postResponse<T, U>(baseUrl: string, subUrl: string, data: T): Observable<ApiOkResponse<U>> {
    return this._commonService.extractCreateResponse(
      this._commonService.http.post<ApiOkResponse<U>>(baseUrl + subUrl, data)
    );
  }

  /**
   * Make actual HTTP POST call for creating new record and return created response.
   */
  createResponse<T, U>(baseUrl: string, subUrl: string, data: T): Observable<ApiCreatedResponse<U>> {
    return this._commonService.extractCreateResponse(
      this._commonService.http.post<ApiCreatedResponse<U>>(baseUrl + subUrl, data)
    );
  }

  /**
   * Make actual HTTP PUT call and return success/error response of given type.
   */
  putResponse<T, U>(baseUrl: string, subUrl: string, data: T): Observable<ApiOkResponse<U>> {
    return this._commonService.extractOkResponse(
      this._commonService.http.put<ApiOkResponse<U>>(baseUrl + subUrl, data)
    );
  }

  /**
   * Make actual HTTP PATCH call and return success/error response of given type.
   */
  patchResponse<T, U>(baseUrl: string, subUrl: string, data: T): Observable<ApiOkResponse<U>> {
    return this._commonService.extractOkResponse(
      this._commonService.http.patch<ApiOkResponse<U>>(baseUrl + subUrl, data)
    );
  }

  /**
   * Make actual HTTP DELETE call and return success/error response.
   */
  deleteResponse<T = unknown>(baseUrl: string, subUrl: string): Observable<ApiOkResponse<T>> {
    return this._commonService.extractOkResponse(
      this._commonService.http.delete<ApiOkResponse<T>>(baseUrl + subUrl)
    );
  }

  //#endregion

  //#region CRUD operations

  /**
   * Get a single record by ID.
   */
  getRecord(id: number): Observable<ApiOkResponse<A>> {
    const url = `${this.baseUrl}/${id}`;
    return this._commonService.extractOkResponse(
      this._commonService.http.get<ApiOkResponse<A>>(url)
    );
  }

  /**
   * Get a single record by ID.
   */
  getRecordById(id: number): Observable<ApiOkResponse<A>> {
    const url = `${this.baseUrl}/${id}`;
    return this._commonService.extractOkResponse(
      this._commonService.http.get<ApiOkResponse<A>>(url)
    );
  }

  /**
   * Create or update a record based on ID presence.
   */
  updateRecord<U>(id: number | undefined, record: A): Observable<ApiOkResponse<U>> {
    if (!id) {
      return this.createResponse<A, U>(this.baseUrl, '', record);
    }
    return this.putResponse<A, U>(this.baseUrl, `/${id}`, record);
  }

  /**
   * Delete a record by ID.
   */
  deleteRecord<T = unknown>(id: number): Observable<ApiOkResponse<T>> {
    const url = `${this.baseUrl}/${id}`;
    return this._commonService.extractOkResponse(
      this._commonService.http.delete<ApiOkResponse<T>>(url)
    );
  }

  /**
   * Enable or disable a record.
   */
  enableDisableRecord<T = unknown>(id: number, isDisable: boolean): Observable<ApiOkResponse<T>> {
    const url = `${this.baseUrl}/enable-disable/${id}/${isDisable}`;
    return this._commonService.extractOkResponse(
      this._commonService.http.post<ApiOkResponse<T>>(url, {})
    );
  }

  /**
   * Get paginated list of records.
   */
  getList<T>(): Observable<T[]> {
    const url = `${this.baseUrl}`;
    return this._commonService.extractOkResponse(
      this._commonService.http.get<ApiOkResponse<DataQueryResult<T>>>(url)
    ).pipe(map(res => res.data?.items ?? []));
  }

  /**
   * Get all records for lookup/dropdown purposes.
   */
  lookUpList<T>(): Observable<T[]> {
    const url = `${this.baseUrl}/lookup-list`;
    return this._commonService.extractOkResponse(
      this._commonService.http.get<ApiOkResponse<DataQueryResult<T>>>(url)
    ).pipe(map(res => res.data?.items ?? []));
  }

  //#endregion

  //#region Toast notifications

  /**
   * Translate and display success message.
   */
  showSuccessIdToast(titleTextId: string, body?: string): void {
    const title = this._commonService.translateService.instant(titleTextId);
    this.showSuccessToast(title, body);
  }

  /**
   * Display success toast.
   */
  showSuccessToast(title: string, body?: string): void {
    if (body) {
      this._commonService.toastrService.successToast(body, title);
    } else {
      this._commonService.toastrService.successToast(title);
    }
  }

  /**
   * Display API error toast.
   */
  showApiErrorToast(apiError: ApiError): void {
    let errorTitle: string;
    const errorDetail = apiError.errorDetail;

    if (apiError.eventMessageId) {
      errorTitle = this._commonService.translateService.instant(apiError.eventMessageId);
    } else if (apiError.errorMessage) {
      errorTitle = apiError.errorMessage;
    } else if (apiError.statusCode === 404) {
      errorTitle = this._commonService.translateService.instant('SERVICE_NOT_AVAILABLE_ERROR');
    } else if (apiError.statusCode === 401) {
      errorTitle = this._commonService.translateService.instant('USER_NOT_AUTHORIZED_ERROR');
    } else {
      errorTitle = this._commonService.translateService.instant('API_SERVICE_RETURN_UNKNOWN_ERROR');
    }

    this.showErrorToast(errorTitle, errorDetail);
  }

  /**
   * Display error toast.
   */
  showErrorToast(title: string, body?: string): void {
    if (body) {
      this._commonService.toastrService.errorToast(body, title);
    } else {
      this._commonService.toastrService.errorToast(title);
    }
  }

  //#endregion
}
