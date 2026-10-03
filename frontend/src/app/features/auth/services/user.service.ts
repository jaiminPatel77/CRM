import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ChangePasswordDto, ResetPasswordRequest, UserDto, UserLookUpDto, UserWithPermissionsDto } from '../../admin/models/user-dto';
import { Observable, map, of, catchError, throwError } from 'rxjs';
import { UserProfileDto } from '../../admin/models/user-profile-dto';
import { ApiOkResponse } from '@shared/models/api-response';
import { ApiError } from '../../../core/models/api-error';
import { EnumPermissionFor } from '@shared/models/common-enums';
import { AuthService } from './auth.service';
import { AuthToken } from '../models/token';
import { instanceToInstance } from 'class-transformer';
import { environment } from '../../../../environments/environment';
import { DataQueryResult } from '@shared/models/data-query-helper'; // Assuming this exists for list/lookup

import { UserPermissionService } from '@shared/services/user-permission.service';
import { BaseService } from '@shared/services/base.service'; // Assuming BaseService is imported from here

@Injectable({
  providedIn: 'root'
})
export class UserService extends BaseService<UserDto> {

  private _currentUserRequest?: Observable<UserWithPermissionsDto | undefined> = undefined;
  userInfo?: UserWithPermissionsDto;

  // services
  private _authService = inject(AuthService);
  private _http = inject(HttpClient);
  public _permissionService = inject(UserPermissionService); // Public for backward compatibility if components use it


  constructor() {
    super(`${environment.Setting.apiServiceUrl}/api/v1/Users`);

    //On user token changed event.  
    this._authService.onTokenChange().subscribe((data: AuthToken | undefined) => {
      if (!data?.isValid()) {
        //Make current user invalid!!
        this.userInfo = new UserWithPermissionsDto();
        this._permissionService.setCurrentUser(this.userInfo);
      }
    });
  }

  //#region Entity specific apis

  public get currentUser(): UserWithPermissionsDto {
    return this._permissionService.currentUser;
  }

  /**
   * get all records from server for entity lookup combo.
   */
  getLookUpList(): Observable<UserLookUpDto[]> {
    const url = `${this.baseUrl}/lookup-list`;
    return this._http.get<ApiOkResponse<UserLookUpDto[]>>(url).pipe(
      map(res => res.data || [])
    );
  }

  /**
   * record to be created/updated. if record.id == 0 it means new record otherwise update existing record.
   * @param {UserDto} record model
   */
  update(record: UserDto): Observable<ApiOkResponse<UserDto>> {
    if (record.id) {
      // Update
      return this._http.put<ApiOkResponse<UserDto>>(`${this.baseUrl}/${record.id}`, record).pipe(map(this.handleUpdateMap(record)));
    } else {
      // Create
      return this._http.post<ApiOkResponse<UserDto>>(`${this.baseUrl}`, record).pipe(map(this.handleUpdateMap(record)));
    }
  }

  private handleUpdateMap(record: UserDto) {
    return (res: ApiOkResponse<UserDto>) => {
      if (record.id == this.currentUser.id) {
        //Update the current user detail..
        instanceToInstance<UserDto>(record);
      }
      return res;
    };
  }

  /**
   * Make actual http call if logged in user detail is not loaded yet!
   * @param id userId for get record detail from db.
   */
  loadCurrentUser(id: number | undefined): Observable<UserWithPermissionsDto> {
    if (this.currentUser != undefined) {
      if (this.currentUser.id === id) {
        return of(this.currentUser);
      } else {
        //get user again.
        this._currentUserRequest = undefined;
      }
    } else if (this._currentUserRequest != undefined) {
      return <Observable<UserWithPermissionsDto>>this._currentUserRequest;
    }
    if (id) {
      let url = `${this.baseUrl}/user-with-permissions/${id}`;
      this._currentUserRequest = this._http.get<ApiOkResponse<UserWithPermissionsDto>>(url)
        .pipe(map(res => {
          if (res.data) {
            this.userInfo = instanceToInstance<UserWithPermissionsDto>(res.data);
          }

          if (this.userInfo) {
            this.userInfo?.permissionsList?.forEach(permission => {
              // Logic from original... seems to verify permissions
              // Original code had some weird logic: permissionFor = permission;
              // Leaving it as it was likely intended or placeholder
            });
          }

          if (this.userInfo) {
            this._permissionService.setCurrentUser(this.userInfo);
          }
          this._currentUserRequest = undefined;
          return <UserWithPermissionsDto>this.userInfo;
        }),
          catchError(error => {
            this._currentUserRequest = undefined;
            return throwError(() => error);
          })
        );
      return <Observable<UserWithPermissionsDto>>this._currentUserRequest;
    } else {
      this._currentUserRequest = undefined;
      return of(this.currentUser);
    }
  }

  /**
   * api to send invitation e-mail to user to set his first time password.
   * @param {ResetPasswordRequest} viewModel model for send set password request.
   */
  adminInviteUser(userId: number): Observable<ApiOkResponse<any>> {
    if (this.currentUser.id == userId) {
      let apiError = new ApiError();
      apiError.eventMessageId = "USER_SELF_INVITE_ERROR";
      apiError.statusCode = 200;
      apiError.statusText = "Bad Request";
      return throwError(() => apiError);
    }
    const url = `${this.baseUrl}/admin-invite-user`;
    const authUrl = `${window.location.origin}${environment._environmentSetting?.rootURL}auth/set-password`;
    let viewModel: ResetPasswordRequest = new ResetPasswordRequest(userId, authUrl);
    return this._http.post<ApiOkResponse<any>>(url, viewModel);
  }

  /**
   * api to send e-mail to reset his password.
   * @param {ResetPasswordRequest} viewModel model for send set password request.
   */
  adminResetPassword(userId: number): Observable<ApiOkResponse<any>> {
    const url = `${this.baseUrl}/admin-reset-password`;
    const authUrl = `${window.location.origin}${environment._environmentSetting?.rootURL}auth/set-password`;
    let viewModel: ResetPasswordRequest = new ResetPasswordRequest(userId, authUrl);
    return this._http.post<ApiOkResponse<any>>(url, viewModel);
  }

  /**
     * ChangePasswordRequest viewModel model for send set new password request.
     */
  userChangePassword(data: ChangePasswordDto): Observable<ApiOkResponse<ChangePasswordDto>> {
    let url = `${this.baseUrl}/change-password`;
    return this._http.patch<ApiOkResponse<ChangePasswordDto>>(url, data);
  }

  /**
   * Update profile details.
   */
  submitUserProfile(data: UserProfileDto): Observable<ApiOkResponse<UserProfileDto>> {
    let url = `${this.baseUrl}/user-profile`;
    return this._http.patch<ApiOkResponse<UserProfileDto>>(url, data);
  }

  /**
     * Get profile details.
     */
  getUserProfile(id: number): Observable<ApiOkResponse<UserProfileDto>> {
    let url = `${this.baseUrl}/user-profile/${id}`;
    return this._http.get<ApiOkResponse<UserProfileDto>>(url);
  }

  override deleteRecord(id: number): Observable<any> {
    const url = `${this.baseUrl}/${id}`;
    return this._http.delete<ApiOkResponse<any>>(url);
  }

  override getRecord(id: number): Observable<ApiOkResponse<UserDto>> {
    const url = `${this.baseUrl}/${id}`;
    return this._http.get<ApiOkResponse<UserDto>>(url);
  }

  createRecord(record: UserDto): Observable<ApiOkResponse<UserDto>> {
    return this._http.post<ApiOkResponse<UserDto>>(this.baseUrl, record);
  }

  override updateRecord<U>(id: number | undefined, record: UserDto): Observable<ApiOkResponse<U>> {
    const url = `${this.baseUrl}/${id}`;
    return this._http.put<ApiOkResponse<U>>(url, record);
  }

  /**
   * Assign roles to a user
   * @param userId The user ID
   * @param roles Array of role names to assign
   */
  assignRoles(userId: number, roles: string[]): Observable<ApiOkResponse<boolean>> {
    const url = `${this.baseUrl}/${userId}/roles`;
    return this._http.post<ApiOkResponse<boolean>>(url, { id: userId, roles: roles });
  }

  //#endregion entity specific apis end

}

