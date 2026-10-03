import { Injectable, signal } from '@angular/core';
import { UserWithPermissionsDto } from '../../features/admin/models/user-dto';
import { EvaluatedPermission } from '../models/base-model';
import { EnumPermissionFor, EnumPermissions, EnumUserType } from '../models/common-enums';
import { PermissionDto } from '../../features/admin/models/role-dto';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserPermissionService {

  /**
   * Signal for getting current user change notification.
   */
  public currentUserChanged = signal<UserWithPermissionsDto>(new UserWithPermissionsDto());

  public get currentUser(): UserWithPermissionsDto {
    return this.currentUserChanged();
  }

  AllAllowedPermission: EvaluatedPermission; // Object which has values indicate all permission are allowed.

  private _isGlobalAdministrator: boolean = false;

  constructor() {
    this._onUserChange();
    this.AllAllowedPermission = new EvaluatedPermission(true, true, true, true, true);
  }

  /**
   * Set current user and notify subscribers via signal.
   * @param user non-null current user instance.
   */
  setCurrentUser(user: UserWithPermissionsDto) {
    if (user && this.currentUser != user) {
      this.currentUserChanged.set(user);
      this._onUserChange();
    } else {
      console.dir("hmmm.. user is already set!");
    }
  }

  /**
   * perform initialization on user change.
   */
  private _onUserChange() {
    if (this.currentUser.userType == EnumUserType.GlobalAdministrator
      || this.currentUser.userType == EnumUserType.EnterpriseAdministrator) {
      this._isGlobalAdministrator = true;
    } else {
      this._isGlobalAdministrator = false;
    }
  }

  /**
   * Indicate that current user is global administrator or not.
   */
  public get isGlobalAdministrator(): boolean {
    return this._isGlobalAdministrator;
  }

  /**
   * Return evaluated permission which is good for angular data binding.
   * @param permissionDto permissionDto for a particular view/component
   */
  public getEvaluatedPermission(permissionDto: PermissionDto): EvaluatedPermission {
    if (this.isGlobalAdministrator) {
      return this.AllAllowedPermission;
    } else {
      let evaluatedPermission = new EvaluatedPermission(
        permissionDto.viewAccess,
        permissionDto.updateAccess,
        permissionDto.createAccess,
        permissionDto.deleteAccess,
        permissionDto.approveAccess
      );
      return evaluatedPermission;
    }
  }

  /**
   * Check whether asked permission is allowed for logged in user or not as Observable.
   * @param permissionFor entity type or permission type to which permission need to check for.
   * @param permission  actual required permission of a give type.
   */
  isGranted(permissionFor: EnumPermissionFor, permission: EnumPermissions): Observable<boolean> {
    let isGranted: boolean = this.isAllowed(permissionFor, permission);
    return of(isGranted);
  }

  /**
   * Check whether asked permission is allowed for logged in user or not as bool value.
   * @param permissionFor entity type or permission type to which permission need to check for.
   * @param permission actual required permission of a give type.
   */
  isAllowed(permissionFor: EnumPermissionFor, permission: EnumPermissions): boolean {
    let allowed: boolean = false;
    if (this.isGlobalAdministrator) {
      allowed = true;
    }
    else if (this.currentUser && this.currentUser.permissions) {
      let associatedPermission = this.currentUser.permissions[EnumPermissionFor[permissionFor] as any];
      allowed = (associatedPermission && ((associatedPermission.permissions & permission) == permission));
    }
    return allowed;
  }
}