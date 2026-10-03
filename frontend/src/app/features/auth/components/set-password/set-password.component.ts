import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { ResetPasswordViewModel } from '../../models/account-model';
import { AuthService } from '../../services/auth.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, ApiOkResponse } from '@shared/models/api-response';
import { TranslateModule } from '@ngx-translate/core';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';
import { Validation } from '@shared/models/validator';
import { CommonModule } from '@angular/common';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { CommonService } from '@shared/services/common.service';

@Component({
  selector: 'app-set-password',
  imports: [ReactiveFormsModule, RouterLink, TranslateModule, SvgIconDirective, CommonModule, NgbTooltip],
  templateUrl: './set-password.component.html',
  styleUrl: './set-password.component.scss'
})
export class SetPasswordComponent implements OnInit, OnDestroy {

  submitted = signal<boolean>(false);
  isShownPwd = signal<boolean>(false);
  isShownConfirmPwd = signal<boolean>(false);

  model!: ResetPasswordViewModel;
  detailForm?: FormGroup;

  private destroy$ = new Subject<boolean>();

  private _formBuilder = inject(FormBuilder);
  private _authService = inject(AuthService);
  protected _router = inject(Router);
  private _activatedRoute = inject(ActivatedRoute);
  private _commonService = inject(CommonService);

  constructor() {
    this.createForm();
  }

  //#region Init and destroy methods
  ngOnInit() {
    this.model = this.createNewModelObject();
    this.setValueChangeEvent();

    this._activatedRoute.queryParamMap.subscribe(qparams => {
      if (this.model) {
        this.model.code = qparams.get("code") ?? undefined;
      }
    });
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
  createNewModelObject(): ResetPasswordViewModel {
    return new ResetPasswordViewModel();
  }

  /**
   * prefer model object from value input in form controls 
   */
  transferDetailFormValuesToModel(model: ResetPasswordViewModel) {
    const detailFormValue = this.detailForm?.value;
    model.email = detailFormValue.email;
    model.password = detailFormValue.password;
  }

  /**
   * Create detail form and it's child controls.
   */
  createForm() {
    this.detailForm = this._formBuilder.group({
      email: [null, [Validators.required, Validators.pattern(/^[\w-\.]+@([\w-]+\.)+[\w-]{2,}$/)]],
      password: [null, [Validators.required, Validators.pattern(/(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[!@#\$%\^&\*()]).{8,}/)]],
      confirmPassword: [null, [Validators.required]]
    }, {
      validators: [Validation.match('password', 'confirmPassword')]
    }
    );
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

  /**
     * perform save data to server if all inputs are valid!
     */
  onSubmit(): void {
    if (this.model && this.validateDetailFormBeforeSubmit()) {
      this.submitted.set(true);
      this.transferDetailFormValuesToModel(this.model);
      this._authService.resetPassword(this.model)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res: ApiOkResponse<any>) => {
            this._commonService.toastrService.successToast("Password reset successfully!", "Success");
            return this._router.navigateByUrl("/auth/login");
          },
          error: (error: ApiError) => {
            this.submitted.set(false);
            this._commonService.toastrService.errorToast(error.errorMessage || "An error occurred");
          }
        }
        );
    }
  }

  /**
   * return true if control is dirty or touched and it's status is invalid.
   * @param {AbstractControl} control any form control object
   */
  isInvalid(control: AbstractControl | null | undefined): boolean {
    return !!(control && control.invalid && (control.dirty || control.touched || this.submitted()));
  }

  //#region Form control properties
  //property for each form control!

  get email() {
    return this.detailForm?.get('email');
  }
  get password() {
    return this.detailForm?.get('password');
  }
  get confirmPassword() {
    return this.detailForm?.get('confirmPassword');
  }

  //#end region Form control properties

  goToLogin() {
    this._router.navigate(['auth/login']);
  }
}
