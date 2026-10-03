import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CaptchaSetting, ForgotPasswordRequest } from '../../models/account-model';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { Router, RouterLink } from '@angular/router';
import { ApiError, ApiOkResponse } from '@shared/models/api-response';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../../../environments/environment';
import { TranslateModule } from '@ngx-translate/core';
import { NgxCaptchaModule } from 'ngx-captcha';
import { CommonModule } from '@angular/common';
import { CommonService } from '@shared/services/common.service';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink, TranslateModule, NgxCaptchaModule, CommonModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent implements OnInit, OnDestroy {

  submitted = signal<boolean>(false);
  isShown = signal<boolean>(false);
  model?: ForgotPasswordRequest;
  detailForm?: FormGroup;
  enableCaptcha: boolean = environment.Setting.enableCaptcha;
  captchaSetting?: CaptchaSetting;

  private destroy$ = new Subject<boolean>();

  private _commonService = inject(CommonService);
  private _fb = inject(FormBuilder);
  protected _router = inject(Router);
  private _authService = inject(AuthService);

  constructor() {
    this.createForm();
  }

  //#region Init and destroy methods
  ngOnInit() {
    this.model = this.createNewModelObject();
    this.setValueChangeEvent();
    this.setDetailFormValues();
  }

  ngOnDestroy(): void {
    if (this.destroy$) {
      this.destroy$.next(true);
      this.destroy$.complete();
    }
  }
  //#endregion Init and destroy methods

  //#region Form related methods

  /**
   * Return new instance of detail view model
   */
  createNewModelObject(): ForgotPasswordRequest {
    return new ForgotPasswordRequest();
  }

  /**
   * prefer model object from value input in form controls 
   */
  transferDetailFormValuesToModel(model: ForgotPasswordRequest) {
    const detailFormValue = this.detailForm?.value;
    model.email = detailFormValue.email;
    model.returnUrl = `${window.location.origin}${environment.Setting.rootURL}auth/set-password`;
    model.secretCode = this.enableCaptcha ? this.captchaSetting?.response : undefined;
  }

  /**
   * Create detail form and it's child controls.
   */
  createForm() {
    this.detailForm = this._fb.group({
      email: [null, [Validators.required, Validators.pattern(/^[\w-\.]+@([\w-]+\.)+[\w-]{2,}$/)]],
      recaptcha: [null, this.enableCaptcha ? [Validators.required] : []]
    });
  }

  /**
   * One time value change event subscribe to form control events!
   */
  setValueChangeEvent() {
  }

  /**
     * validate form controls before submit data to server!
     * by default only validate the detailForm .. in case if any custom validation needed override it.
     */
  validateDetailFormBeforeSubmit(): boolean {
    if (this.detailForm) {
      this.validateAllFormFields(this.detailForm);
      return this.detailForm.valid;
    }
    return false;
  }

  /**
  * Mark all control of a given FormGroup as Touched or dirty so that form will show appropriate error
  * @param {FormGroup} formGroup form group object
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

  /**
     * perform save data to server if all inputs are valid!
     */
  onSubmit(): void {
    this.detailForm?.markAllAsTouched();
    if (this.model && this.validateDetailFormBeforeSubmit()) {
      this.submitted.set(true);
      this.transferDetailFormValuesToModel(this.model);
      this._authService.forgotPassword(this.model)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result: ApiOkResponse<any>) => {
            return this._router.navigateByUrl("/auth/forgot-password-confirm");
          },
          error: (error: ApiError) => {
            this.submitted.set(false);
            this._commonService.toastrService.errorToast(error.errorMessage || "An error occurred");
          }
        }
        );
    }
  }

  setDetailFormValues() {
    if (this.model) {
      this.detailForm?.patchValue({
        email: this.model.email,
        secretCode: null,
      });
      this.captchaSetting = this._authService.getCaptchaSetting();
    }
  }

  //#region Form control properties
  //property for each form control!

  get email() {
    return this.detailForm?.get('email');
  }
  get recaptcha() {
    return this.detailForm?.get('recaptcha');
  }

  //#end region Form control properties

  /**
   * helper to check if control is invalid
   */
  isInvalid(control: AbstractControl | null | undefined): boolean {
    return !!(control && control.invalid && (control.dirty || control.touched || this.submitted()));
  }

  //#end region  Form related methods

  //#region Captcha methods
  handleReset(): void {
    if (this.captchaSetting) {
      this.captchaSetting.response = undefined;
      this.captchaSetting.isLoaded = false;
    }
  }

  handleSuccess(captchaResponse: string): void {
    if (this.captchaSetting) {
      this.captchaSetting.response = captchaResponse;
    }
  }

  handleLoad(): void {
    if (this.captchaSetting) {
      this.captchaSetting.isLoaded = true;
    }
  }

  handleExpire(): void {
    if (this.captchaSetting) {
      this.captchaSetting.response = undefined;
    }
  }
  //#endregion  Captcha methods

  goToLogin() {
    this._router.navigate(['auth/login']);
  }
}
