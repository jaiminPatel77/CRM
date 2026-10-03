import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { BaseDetailComponent } from '@shared/components/base-detail/base-detail.component';
import { SMTPSetting, SettingDto } from '../../models/setting-dto';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { EnumPermissionFor } from '@shared/models/common-enums';
import { SettingService } from '../../services/setting.service';
import { takeUntil } from 'rxjs';
import { DataStatusEnum } from '@shared/models/common-models';
import { instanceToInstance } from 'class-transformer';
import { ApiError, ApiOkResponse } from '@shared/models/api-response';
import { TranslateModule } from '@ngx-translate/core';
import { FocusFirstInvalidFieldDirective } from '@shared/directives/focus-first-invalid-field.directive';
import { ErrorDetailComponent } from '@shared/components/error-detail/error-detail.component';
import { LoadingComponent } from '@shared/components/loading/loading.component';
import { CommonModule } from '@angular/common';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';


@Component({
  selector: 'app-mail-setting',
  imports: [ReactiveFormsModule, TranslateModule, FocusFirstInvalidFieldDirective, ErrorDetailComponent, LoadingComponent, SvgIconDirective, CommonModule, NgbTooltip],
  templateUrl: './mail-setting.component.html',
  styleUrl: './mail-setting.component.scss'
})
export class MailSettingComponent extends BaseDetailComponent<SettingDto<SMTPSetting>> implements OnInit, OnDestroy {

  isShownPwd: boolean = false;

  constructor(
    private _settingService: SettingService,
  ) {
    super();
    this.requiredPermissionType = EnumPermissionFor.SETTING;
  }

  //#region Init and destroy methods

  override ngOnInit() {
    super.ngOnInit();
    this.model = new SettingDto<SMTPSetting>();
    this.model.value = new SMTPSetting();
    this.getRecord();
  }
  //#endregion Init and destroy methods

  //#region Load required data from server!

  /**
   * get smtp setting record from server.
   */
  getRecord() {
    this._settingService.getSMTPSetting()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: ApiOkResponse<SettingDto<SMTPSetting>>) => {
          if (result.data) {
            this.model = instanceToInstance<SettingDto<SMTPSetting>>(result.data);
          }
          this.setDetailFormValues();
          this.dataStatus = DataStatusEnum.DataAvailable;
          this._cdr.detectChanges();
        },
        error: (error: ApiError) => {
          //Here we should show error component instead of toast message!
          this.dataStatus = DataStatusEnum.Error;
          this._cdr.detectChanges();
          this.apiError = error;
        }
      }
      );
  }

  //#endregion Load required data from server end!

  //#region Must override methods

  /**
   * Return new instance of detail view model
   */
  override createNewModelObject(): SettingDto<SMTPSetting> {
    let newModel = new SettingDto<SMTPSetting>();
    newModel.value = new SMTPSetting();
    return newModel;
  }

  /**
   * prefer model object from value input in form controls 
   */
  override transferDetailFormValuesToModel(model: SettingDto<SMTPSetting>) {
    const detailFormValue = this.detailForm?.value;
    const setting = model.value;
    if (setting) {
      setting.userName = detailFormValue.userName;
      setting.password = detailFormValue.password;
      setting.serverAddress = detailFormValue.serverAddress;
      setting.serverPort = detailFormValue.serverPort;
      setting.isSSL = detailFormValue.isSSL;
      setting.fromEmail = detailFormValue.fromEmail;
    }
  }

  /**
   * Create detail form and it's child controls.
   */
  override createForm() {
    this.detailForm = this._formBuilder.group({
      userName: [null, Validators.required],
      password: [null, Validators.required],
      serverAddress: [null, Validators.required],
      serverPort: [null, [Validators.required, Validators.pattern(/^[0-9]+$/)]],
      isSSL: [null, Validators.required],
      fromEmail: [null, [Validators.required, Validators.pattern(/^[\w-\.]+@([\w-]+\.)+[\w-]{2,}$/)]],
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

  //#endregion Must override methods end  

  //#region Form related methods

  /**
   * transfer model values to form control
   */
  setDetailFormValues() {
    let setting = this.model?.value;
    this.detailForm?.patchValue({
      userName: setting?.userName,
      password: setting?.password,
      serverAddress: setting?.serverAddress,
      serverPort: setting?.serverPort,
      isSSL: setting?.isSSL,
      fromEmail: setting?.fromEmail,
    });
  }

  //#region Form control properties

  //property for each form control!
  get userName() {
    return this.detailForm?.get('userName');
  }
  get password() {
    return this.detailForm?.get('password');
  }
  get serverAddress() {
    return this.detailForm?.get('serverAddress');
  }
  get serverPort() {
    return this.detailForm?.get('serverPort');
  }
  get isSSL() {
    return this.detailForm?.get('isSSL');
  }
  get fromEmail() {
    return this.detailForm?.get('fromEmail');
  }
  //#endregion Form control properties


  //#region can override methods

  /**
   * perform submit data to server  if all inputs are valid!
   */
  override onSubmit(): void {
    if (this.model && this.validateDetailFormBeforeSubmit()) {
      this.submitted = true;
      this.transferDetailFormValuesToModel(this.model);
      this._settingService.putSMTPSetting(this.model)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result: ApiOkResponse<SettingDto<SMTPSetting>>) => {
            this.submitted = false;
            if (result.eventMessageId) {
              this._settingService.showSuccessIdToast(result.eventMessageId);
            } else {
              this._settingService.showSuccessToast('SMTP settings saved successfully!');
            }
            // Reload the saved data
            this.getRecord();
          },
          error: (error: ApiError) => {
            this.submitted = false;
            this._settingService.showApiErrorToast(error);
          }
        }
        );
    }
  }

  override onCancel() {
    this._router.navigateByUrl("dashboard");
  }

  //#endregion can override methods end!  


  //#endregion  Form related methods  

}
