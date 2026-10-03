import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { BaseDetailComponent } from '@shared/components/base-detail/base-detail.component';
import { UserDto } from '../../models/user-dto';
import { RoleLookUpDto } from '../../models/role-dto';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../../auth/services/user.service';
import { EnumPermissionFor } from '@shared/models/common-enums';
import { Observable, takeUntil } from 'rxjs';
import { DataStatusEnum } from '@shared/models/common-models';
import { ApiError, ApiOkResponse } from '@shared/models/api-response';
import { RoleService } from '../../../auth/services/role.service';
import { instanceToInstance } from 'class-transformer';
import { TranslateModule } from '@ngx-translate/core';
import { NgSelectModule } from '@ng-select/ng-select';
import { FocusFirstInvalidFieldDirective } from '@shared/directives/focus-first-invalid-field.directive';
import { LoadingComponent } from '@shared/components/loading/loading.component';
import { ErrorDetailComponent } from '@shared/components/error-detail/error-detail.component';
import { CommonModule } from '@angular/common';
import { PascalCaseToSpacePipe } from '@shared/pipes/pascal-case-to-space.pipe';

@Component({
  selector: 'app-user-detail',
  imports: [ReactiveFormsModule, TranslateModule, NgSelectModule, FocusFirstInvalidFieldDirective, ErrorDetailComponent, LoadingComponent, CommonModule, PascalCaseToSpacePipe],
  templateUrl: './user-detail.component.html',
  styleUrl: './user-detail.component.scss'
})
export class UserDetailComponent extends BaseDetailComponent<UserDto> implements OnInit, OnDestroy {

  roleList: RoleLookUpDto[] = [];

  // inject services
  protected _roleService = inject(RoleService);
  private _userService = inject(UserService);

  constructor() {
    super();
    this.requiredPermissionType = EnumPermissionFor.USER;
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
      this.getRoleList();
    } else {
      this._userService.getRecord(this.recordId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result: ApiOkResponse<UserDto>) => {
            if (result.data) {
              this.model = instanceToInstance<UserDto>(result.data);
            }
            this.getRoleList();
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

  getRoleList() {
    this._roleService.getLookUpList()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: RoleLookUpDto[]) => {
          this.roleList = instanceToInstance<RoleLookUpDto[]>(result);
          this.setDetailFormValues();
          this.dataStatus = DataStatusEnum.DataAvailable;
           this._cdr.detectChanges();
        },
        error: (error: ApiError) => {
          this.dataStatus = DataStatusEnum.DataAvailable;
           this._cdr.detectChanges();
          this._roleService.showApiErrorToast(error);
        }
      }
      );
  }

  override saveRecord(model: UserDto): Observable<ApiOkResponse<any>> {
    return this._userService.update(model);
  }

  //#endregion Load required data from server end!

  //#region Form related methods

  //#region Must override methods

  /**
   * Return new instance of detail view model
   */
  override createNewModelObject(): UserDto {
    let newModel = new UserDto();
    return newModel;
  }

  /**
   * prefer model object from value input in form controls 
   */
  override transferDetailFormValuesToModel(model: UserDto) {
    const detailFormValue = this.detailForm?.value;
    model.email = detailFormValue.email;
    model.fullName = detailFormValue.fullName;
    model.title = detailFormValue.title;
    model.phoneNumber = detailFormValue.phoneNumber;
    model.roles = detailFormValue.roles?.map((r: RoleLookUpDto) => r.name) || [];
  }

  /**
   * Create detail form and it's child controls.
   */
  override createForm() {
    this.detailForm = this._formBuilder.group({
      email: [null, [Validators.required, Validators.pattern(/^[\w-\.]+@([\w-]+\.)+[\w-]{2,}$/)]],
      fullName: [null, [Validators.required]],
      title: [null, [Validators.required]],
      phoneNumber: [null, [Validators.pattern(/^\+?[0-9]{10}(?:x.+)?$/)]],
      roles: [null]
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
    let selectedRoles: RoleLookUpDto[] = [];
    if (this.model) {
      this.model.roles?.forEach(roleName => {
        const roleRec = this.roleList.find(c => c.name === roleName);
        if (roleRec) {
          selectedRoles.push(instanceToInstance<RoleLookUpDto>(roleRec));
        }
      });
      this.detailForm?.patchValue({
        email: this.model.email,
        fullName: this.model.fullName,
        title: this.model.title,
        phoneNumber: this.model.phoneNumber,
        roles: selectedRoles
      });
    }
  }

  //#region Form control properties

  //property for each form control!
  get email() {
    return this.detailForm?.get('email');
  }
  get fullName() {
    return this.detailForm?.get('fullName');
  }
  get title() {
    return this.detailForm?.get('title');
  }
  get phoneNumber() {
    return this.detailForm?.get('phoneNumber');
  }
  get roles() {
    return this.detailForm?.get('roles');
  }
  //#endregion Form control properties

  //#endregion  Form related methods

}
