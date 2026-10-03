import { Injectable, inject } from '@angular/core';
import { BaseService } from '@shared/services/base.service';
import { PermissionDto, RoleDto, RoleListDto, RoleLookUpDto } from '../../admin/models/role-dto';
import { Observable, map } from 'rxjs';
import { ApiOkResponse } from '@shared/models/api-response';
import { BaseLookUpDto } from '@shared/models/base-model';
import { environment } from '../../../../environments/environment';
import { DataQueryResult } from '@shared/models/data-query-helper';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class RoleService extends BaseService<RoleDto> {

  private _http = inject(HttpClient);

  constructor() {
    super(`${environment.Setting.apiServiceUrl}/api/v1/Roles`);
  }

  /**
   * get all records from server for entity lookup combo.
   */
  getLookUpList(): Observable<RoleLookUpDto[]> {
    const url = `${this.baseUrl}/lookup-list`;
    return this._http.get<ApiOkResponse<RoleLookUpDto[]>>(url).pipe(
      map((res: ApiOkResponse<RoleLookUpDto[]>) => res.data || [])
    );
  }

  override deleteRecord(id: number): Observable<any> {
    const url = `${this.baseUrl}/${id}`;
    return this._http.delete<ApiOkResponse<any>>(url);
  }

  override getRecord(id: number): Observable<ApiOkResponse<RoleDto>> {
    const url = `${this.baseUrl}/${id}`;
    return this._http.get<ApiOkResponse<RoleDto>>(url);
  }

  createRecord(record: RoleDto): Observable<ApiOkResponse<RoleDto>> {
    return this._http.post<ApiOkResponse<RoleDto>>(this.baseUrl, record);
  }

  override updateRecord<U>(id: number | undefined, record: RoleDto): Observable<ApiOkResponse<U>> {
    const url = `${this.baseUrl}/${id}`;
    return this._http.put<ApiOkResponse<U>>(url, record);
  }

  /**
   * Get all available permissions
   */
  getPermissions(): Observable<string[]> {
    const url = `${this.baseUrl}/permissions`;
    return this._http.get<ApiOkResponse<string[]>>(url).pipe(
      map(res => res.data || [])
    );
  }

  /**
   * Update role permissions
   * @param roleId The role ID
   * @param permissions Array of permission strings to assign
   */
  updatePermissions(roleId: number, permissions: string[]): Observable<ApiOkResponse<boolean>> {
    const url = `${this.baseUrl}/${roleId}/permissions`;
    return this._http.put<ApiOkResponse<boolean>>(url, permissions);
  }
}
