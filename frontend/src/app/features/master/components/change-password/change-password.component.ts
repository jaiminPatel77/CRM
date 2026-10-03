import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntil } from 'rxjs';
import { ChangePasswordDto } from '@features/admin/models/user-dto';
import { UserService } from '@auth/services/user.service';
import { ApiError, ApiOkResponse } from '@shared/models/api-response';
import { NgbTooltip, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { FocusFirstInvalidFieldDirective } from '@shared/directives/focus-first-invalid-field.directive';
import { BaseDetailPopupComponent } from '@shared/components/base-detail-popup/base-detail-popup.component';
import { CommonService } from '@shared/services/common.service';
import { EnumPermissionFor } from '@shared/models/common-enums';
import { DataStatusEnum } from '@shared/models/common-models';
import { Validation } from '@shared/models/validator';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-change-password',
  imports: [ReactiveFormsModule, TranslateModule, FocusFirstInvalidFieldDirective, NgbTooltip, SvgIconDirective, CommonModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss'
})
export class ChangePasswordComponent extends BaseDetailPopupComponent<ChangePasswordDto> implements OnInit, OnDestroy {

  isOldPwdShown: boolean = false;
  isNewPwdShown: boolean = false;
  isConfirmPwdShown: boolean = false;

  private _userService = inject(UserService);
  private _commonService = inject(CommonService);

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
      this.setDetailFormValues();
      this.dataStatus = DataStatusEnum.DataAvailable;
    } else {
      this.dataStatus = DataStatusEnum.DataAvailable;
    }
  }

  //#endregion Load required data from server end!

  //#region Form related methods

  //#region Must override methods

  /**
   * Return new instance of detail view model
   */
  override createNewModelObject(): ChangePasswordDto {
    let newModel = new ChangePasswordDto();
    return newModel;
  }

  /**
   * prefer model object from value input in form controls 
   */
  override transferDetailFormValuesToModel(model: ChangePasswordDto) {
    const detailFormValue = this.detailForm?.value;
    if (this._userService.currentUser && this._userService.currentUser.id) {
      model.userId = this._userService.currentUser.id;
    }
    model.oldPassword = detailFormValue.oldPassword;
    model.newPassword = detailFormValue.newPassword;
    model.$confirmPassword = detailFormValue.confirmPassword;
  }

  /**
   * Create detail form and it's child controls.
   */
  override createForm() {
    this.detailForm = this._formBuilder.group({
      oldPassword: [null, [Validators.required]],
      newPassword: [null, [Validators.required, Validators.pattern(/(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[!@#\$%\^&\*()]).{8,}/)]],
      confirmPassword: [null, [Validators.required]],
    },
      {
        validators: [Validation.match('newPassword', 'confirmPassword')]
      });
  }

  /**
   * One time value change event subscribe to form control events!
   */
  override setValueChangeEvent() {
    //keep subscribe still not destroy!
    // this.name.valueChanges.pipe(debounceTime(500), distinctUntilChanged(), takeUntil(this.destroy$)).subscribe(
    //   newValue => {
    //     console.log("name value changed!!");
    //   }
    // );
  }

  /**
     * perform save data to server if all inputs are valid!
     */
  override onSubmit(): void {
    if (this.model && this.validateDetailFormBeforeSubmit()) {
      this.submitted = true;
      this.transferDetailFormValuesToModel(this.model);
      this._userService.userChangePassword(this.model)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result: ApiOkResponse<ChangePasswordDto>) => {
            if (result?.eventMessageId) {
              this._commonService.toastrService.successToast(result.eventMessageId);
              this.activeModal.close(true);
            }
          },
          error: (error: ApiError) => {
            this.submitted = false;
            this._commonService.toastrService.errorToast(error.errorMessage || 'An error occurred');
          }
        }
        );
    }
  }

  //#end region Must override methods end

  /**
   * transfer model values to form control
   */
  setDetailFormValues() {
    if (this.model) {
      this.detailForm?.patchValue({
        oldPassword: this.model.oldPassword,
        newPassword: this.model.newPassword,
        confirmPassword: this.model.$confirmPassword
      });
    }
  }

  //#region Form control properties
  //property for each form control!

  get oldPassword() {
    return this.detailForm?.get('oldPassword');
  }
  get newPassword() {
    return this.detailForm?.get('newPassword');
  }
  get confirmPassword() {
    return this.detailForm?.get('confirmPassword');
  }

  //#end region Form control properties

  //#end region  Form related methods
}
