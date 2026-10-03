import { Component, OnDestroy, OnInit, Directive, inject, effect } from '@angular/core';
import { AbstractControl, FormControl, FormGroup } from '@angular/forms';
import { ApiError } from '../../models/api-response';
import { EvaluatedPermission } from '../../models/base-model';
import { EnumPermissionFor, EnumUserType } from '../../models/common-enums';
import { UserWithPermissionsDto } from '../../../features/admin/models/user-dto';
import { UserPermissionService } from '../../services/user-permission.service';
import { DateTimeHelper } from '../../models/date-time-helper';
import { PermissionDto } from '../../../features/admin/models/role-dto';
import { DataStatusEnum } from '../../models/common-models';

@Directive()
export class BaseComponent implements OnInit, OnDestroy {

  dataStatus: DataStatusEnum;
  dataStatusEnum = DataStatusEnum;
  apiError?: ApiError;
  currentUser?: UserWithPermissionsDto;
  requiredPermissionType?: EnumPermissionFor;
  evaluatedPermission: EvaluatedPermission;
  permissionService = inject(UserPermissionService);

  dateFormat = DateTimeHelper.dateFormat;
  timeFormat = DateTimeHelper.timeFormat;
  enumUserType = EnumUserType;

  constructor() {
    this.dataStatus = DataStatusEnum.None;
    this.evaluatedPermission = this.permissionService.AllAllowedPermission; //by default all allowed!

    // Handle current user change event to load correct permission reactively
    effect(() => {
      const user = this.permissionService.currentUserChanged();
      this.currentUser = user;
      this.updateEvaluatedPermission();
    });
  }

  ngOnInit() {
    this.updateEvaluatedPermission();
  }

  private updateEvaluatedPermission() {
    if (this.requiredPermissionType && this.currentUser) {
      const associatedPermission = this.currentUser.permissions 
        ? this.currentUser.permissions[EnumPermissionFor[this.requiredPermissionType] as any] 
        : undefined;
      
      let permissionDto: PermissionDto;
      if (associatedPermission) {
        permissionDto = associatedPermission;
      } else {
        permissionDto = new PermissionDto();
      }
      this.evaluatedPermission = this.permissionService.getEvaluatedPermission(permissionDto);
    }
  }

  ngOnDestroy(): void {
  }

  /**
   * Override it to return true if data is changed otherwise return false.
   * If model is not changed then it will not ask for navigation confirmation.
   */
  get isModelChanged(): boolean {
    return false;
  }

  /**
   * return true if control is dirty or touched and it's status is invalid.
   * @param {AbstractControl} control any form control object
   */
  isInvalid(control: AbstractControl | null | undefined): boolean {
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  /**
     * Mark all control of a given FormGroup as Touched or dirty so that form will show appropriate error
     * @param {FormGroup} formGroup formgroup object
     */
  validateAllFormFields(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(field => {
      const control = formGroup.get(field);
      if (control instanceof FormControl) {
        control.markAsTouched({ onlySelf: true });
      } else if (control instanceof FormGroup) {
        this.validateAllFormFields(control);
      }
    });
  }

}
