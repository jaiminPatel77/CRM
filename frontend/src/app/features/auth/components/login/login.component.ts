
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject, switchMap, takeUntil } from 'rxjs';
import { ApiOkResponse, ApiError } from '@shared/models/api-response';
import { LoginViewModel } from '../../models/account-model';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { TranslateModule } from '@ngx-translate/core';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';
import { BaseComponent } from '@shared/components/base/base.component';
import { CommonModule } from '@angular/common';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { CommonService } from '@shared/services/common.service';

import { environment } from '@env/environment';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, TranslateModule, SvgIconDirective, CommonModule, NgbTooltip],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent extends BaseComponent implements OnInit, OnDestroy {

  submitted = signal<boolean>(false);
  isShown = signal<boolean>(false);
  model?: LoginViewModel;
  detailForm?: FormGroup;

  private destroy$ = new Subject<boolean>();

  private _formBuilder = inject(FormBuilder);
  private _authService = inject(AuthService);
  protected _router = inject(Router);
  private _commonService = inject(CommonService);
  private _userService = inject(UserService);

  constructor() {
    super();
  }

  override ngOnInit() {
    super.ngOnInit();
    this.model = new LoginViewModel();
    this.model.deviceId = (new Date()).toUTCString();
    this.createForm();
    this.setValueChangeEvent();
    this.setDetailFormValues();

    if (!environment.production) {
      this.detailForm?.patchValue({
        email: 'admin@crm.local',
        password: 'Test@123'
      });
      this.detailForm?.markAllAsDirty();
    }
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    if (this.destroy$) {
      this.destroy$.next(true);
      this.destroy$.complete();
    }
  }

  /**
   * perform submit data to server  if all inputs are valid!
   */
  onSubmit(): void {
    this.detailForm?.markAllAsTouched();
    if (this.validateDetailFormBeforeSubmit()) {
      this.submitted.set(true);
      this.transferDetailFormValuesToModel();
      if (this.model) {
        this._authService.login(this.model)
          .pipe(
            takeUntil(this.destroy$),
            switchMap((result: ApiOkResponse<any>) => {
              // Login successful, token is set. Now load profile.
              if (result?.eventMessageId) {
                this._commonService.toastrService.successToast(result?.eventMessageId, "Success");
              }
              console.log(result);
              // Get current user ID from the newly set token
              const validUserId = this._authService.getCurrentToken?.userId();
              console.log(validUserId);
              if (validUserId) {
                return this._userService.loadCurrentUser(validUserId);
              } else {
                // Should not happen if token is valid, but handle gracefully
                throw new Error("User ID not found in token");
              }
            })
          )
          .subscribe({
            next: (userProfile) => {
              // Profile loaded, NOW redirect
              this.submitted.set(false);
              const redirect = this._authService.redirectUrl ? this._authService.redirectUrl : '/dashboard';
              this._router.navigateByUrl(redirect);
            },
            error: (error: ApiError | any) => {
              this.submitted.set(false);
              // Handle specific API errors or generic ones
              const msg = error?.errorMessage || error?.message || "An error occurred during login processing";
              this._commonService.toastrService.errorToast(msg);
            }
          });
      }
    }
  }

  //#region Form related methods

  /**
   * Create detail form and it's child controls.
   */
  createForm() {
    this.detailForm = this._formBuilder.group({
      email: [null, [Validators.required, Validators.pattern(/^[\w-\.]+@([\w-]+\.)+[\w-]{2,}$/)]],
      password: [null, [Validators.required]],
      rememberMe: [false]
    });
  }

  /**
   * transfer model values to form control
   */
  setDetailFormValues() {
    if (this.model) {
      this.detailForm?.patchValue({
        email: this.model.email,
        password: this.model.password,
        rememberMe: this.model.rememberMe ?? false,
      });
    }
  }

  /**
   * prefer model object from value input in form controls
   */
  transferDetailFormValuesToModel() {
    if (this.model && this.detailForm) {
      const detailFormValue = this.detailForm.value;
      this.model.email = detailFormValue.email;
      this.model.password = detailFormValue.password;
      this.model.rememberMe = detailFormValue.rememberMe;
    }

  }


  //#region Form control change events

  /**
   * One time value change event subscribe to form control events!
   */
  setValueChangeEvent() {

  }
  //#endregion


  /**
   * validate form controls before submit data to server!
   */
  validateDetailFormBeforeSubmit(): boolean {
    this.detailForm?.markAsTouched();
    return this.detailForm?.valid ?? true;
  }

  //#region Form control properties

  //property for each form control!
  get email() {
    return this.detailForm?.get('email');
  }
  get password() {
    return this.detailForm?.get('password');
  }
  get rememberMe() {
    return this.detailForm?.get('rememberMe');
  }
  //#endregion 

  //#endregion  
}