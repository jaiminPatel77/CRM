import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { PermissionDto, RoleDto } from '../../models/role-dto';
import { FormArray, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable, distinctUntilChanged, takeUntil } from 'rxjs';
import { BaseDetailComponent } from '@shared/components/base-detail/base-detail.component';
import { UserLookUpDto } from '../../models/user-dto';
import { UserService } from '../../../auth/services/user.service';
import { RoleService } from '../../../auth/services/role.service';
import { EnumPermissionFor } from '@shared/models/common-enums';
import { ApiError, ApiOkResponse } from '@shared/models/api-response';
import { DataStatusEnum } from '@shared/models/common-models';
import { instanceToInstance, plainToClass } from 'class-transformer';
import { LoadingComponent } from '@shared/components/loading/loading.component';
import { TranslateModule } from '@ngx-translate/core';
import { NgSelectModule } from '@ng-select/ng-select';
import { FocusFirstInvalidFieldDirective } from '@shared/directives/focus-first-invalid-field.directive';
import { ErrorDetailComponent } from '@shared/components/error-detail/error-detail.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-role-detail',
  imports: [ReactiveFormsModule, TranslateModule, NgSelectModule, FocusFirstInvalidFieldDirective, ErrorDetailComponent, LoadingComponent, CommonModule],
  templateUrl: './role-detail.component.html',
  styleUrl: './role-detail.component.scss'
})
export class RoleDetailComponent extends BaseDetailComponent<RoleDto> implements OnInit, OnDestroy {

  userList: UserLookUpDto[] = [];

  //Role permission current status!
  currentPermissions: PermissionDto[] = [];

  // inject services
  protected _userService = inject(UserService);
  private _roleService = inject(RoleService);

  constructor() {
    super();
    this.requiredPermissionType = EnumPermissionFor.ROLE;
  }

  //#region Init and destroy methods

  override ngOnInit() {
    super.ngOnInit();
    this.getRecord();
  }

  //#endregion Init and destroy methods

  //#region Load required data from server!

  getRecord() {
    if (this.isNew) {
      this.model = this.createNewModelObject();
      this.getUserList();
    } else {
      this._roleService.getRecord(this.recordId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result: ApiOkResponse<RoleDto>) => {
            if (result.data) {
              this.model = instanceToInstance<RoleDto>(result.data);
            }
            this.getUserList();
          },
          error: (error: ApiError) => {
            this.dataStatus = DataStatusEnum.Error;
             this._cdr.detectChanges();
            this.apiError = error;
          }
        }
        );
    }
  }

  getUserList() {
    this._userService.getLookUpList()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: UserLookUpDto[]) => {
          this.userList = instanceToInstance<UserLookUpDto[]>(result);
          this.setDetailFormValues();
          this.dataStatus = DataStatusEnum.DataAvailable;
           this._cdr.detectChanges();

        },
        error: (error: ApiError) => {
          this.dataStatus = DataStatusEnum.DataAvailable;
           this._cdr.detectChanges();
          this._userService.showApiErrorToast(error);
        }
      }
      );
  }

  override saveRecord(model: RoleDto): Observable<ApiOkResponse<any>> {
    if (this.isNew) {
      return this._roleService.createRecord(model);
    } else {
      return this._roleService.updateRecord(model.id, model);
    }
  }

  //#endregion Load required data from server end!

  //#region Form related methods

  //#region Must override methods

  /**
   * Return new instance of detail view model
   */
  override createNewModelObject(): RoleDto {
    let newModel = new RoleDto();
    return newModel;
  }

  /**
   * prefer model object from value input in form controls 
   */
  override transferDetailFormValuesToModel(model: RoleDto) {
    const detailFormValue = this.detailForm?.value;
    model.name = detailFormValue.name;
    model.description = detailFormValue.description;
    model.users = detailFormValue.users;
    model.permissions = this.currentPermissions;
  }

  /**
   * Create detail form and it's child controls.
   */
  override createForm() {
    this.detailForm = this._formBuilder.group({
      name: [null, [Validators.required]],
      description: [null, [Validators.required]],
      users: [null],
      rolePermissions: this._formBuilder.array([]), //permission array!
    });
  }

  /**
   * One time value change event subscribe to form control events!
   */
  override setValueChangeEvent() {
  }

  //#endregion Must override methods end  

  /**
   * transfer model values to form control
   */
  setDetailFormValues() {
    //set the selected role values..
    let selectedUsers: UserLookUpDto[] = [];
    if (this.model) {
      this.model.users.forEach(role => {
        const userRec = this.userList.find(c => c.id === role.id);
        if (userRec) {
          selectedUsers.push(instanceToInstance<UserLookUpDto>(userRec));
        }
      });
      this.detailForm?.patchValue({
        name: this.model.name,
        description: this.model.description,
        users: selectedUsers
      });
      if (this.model.permissions) {
        this.model.permissions.forEach((permission: PermissionDto) => {
          let currentPermissionStatus = plainToClass(PermissionDto, permission);
          let permissionGroup = this.createPermissionGroup(currentPermissionStatus);
          this.currentPermissions.push(currentPermissionStatus);
          this.rolePermissions.push(permissionGroup);
        });
      }
    }
  }

  //#region Form control properties

  //property for each form control!
  get name() {
    return this.detailForm?.get('name');
  }
  get description() {
    return this.detailForm?.get('description');
  }
  get users() {
    return this.detailForm?.get('users');
  }
  get rolePermissions(): FormArray {
    return this.detailForm?.get('rolePermissions') as FormArray;
  }

  //#endregion Form control properties

  //#endregion  Form related methods

  //#region component specific methods

  createPermissionGroup(permissionDto: PermissionDto): FormGroup {
    let permissionGroup = this._formBuilder.group({
      textId: `PERMISSION_${EnumPermissionFor[permissionDto.permissionFor]}`,
      viewAccess: permissionDto.viewAccess,
      updateAccess: permissionDto.updateAccess,
      createAccess: permissionDto.createAccess,
      deleteAccess: permissionDto.deleteAccess,
      allAccess: permissionDto.allAccess
    });
    //bind associated event on value change..
    permissionGroup?.get('viewAccess')?.valueChanges.pipe(distinctUntilChanged(), takeUntil(this.destroy$)).subscribe(
      newValue => {
        //set only if new value        
        if (permissionDto.viewAccess != newValue) {
          permissionDto.viewAccess = newValue || false;
          permissionGroup.patchValue(this.getUpdatedPermission(permissionDto));
        }
      }
    );
    permissionGroup?.get('updateAccess')?.valueChanges.pipe(distinctUntilChanged(), takeUntil(this.destroy$)).subscribe(
      newValue => {
        //set only if new value
        if (permissionDto.updateAccess != newValue) {
          permissionDto.updateAccess = newValue || false;
          permissionGroup.patchValue(this.getUpdatedPermission(permissionDto));
        }
      }
    );
    permissionGroup?.get('createAccess')?.valueChanges.pipe(distinctUntilChanged(), takeUntil(this.destroy$)).subscribe(
      newValue => {
        //set only if new value
        if (permissionDto.createAccess != newValue) {
          permissionDto.createAccess = newValue || false;
          //also patch affect to other checkbox values!
          permissionGroup.patchValue(this.getUpdatedPermission(permissionDto));
        }
      }
    );
    permissionGroup?.get('deleteAccess')?.valueChanges.pipe(distinctUntilChanged(), takeUntil(this.destroy$)).subscribe(
      newValue => {
        //set only if new value
        if (permissionDto.deleteAccess != newValue) {
          permissionDto.deleteAccess = newValue || false;
          //also patch affect to other checkbox values!
          permissionGroup.patchValue(this.getUpdatedPermission(permissionDto));
        }
      }
    );
    permissionGroup?.get('allAccess')?.valueChanges.pipe(distinctUntilChanged(), takeUntil(this.destroy$)).subscribe(
      newValue => {
        //set only if new value
        if (permissionDto.allAccess != newValue) {
          permissionDto.allAccess = newValue || false;
          //also patch affect to other checkbox values!
          permissionGroup.patchValue(this.getUpdatedPermission(permissionDto));
        }
      }
    );
    return permissionGroup;
  }

  getUpdatedPermission(permissionDto: PermissionDto): any {
    return {
      viewAccess: permissionDto.viewAccess,
      updateAccess: permissionDto.updateAccess,
      createAccess: permissionDto.createAccess,
      deleteAccess: permissionDto.deleteAccess,
      allAccess: permissionDto.allAccess,
    };
  }
  //#endregion component specific methods

}
