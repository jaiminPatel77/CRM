import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CaptchaSetting, ForgotPasswordRequest, RequestAccessDto, ResetPasswordDto } from '../models/account-model';
import { Observable } from 'rxjs';
import { ApiOkResponse } from '@shared/models/api-response';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private _http = inject(HttpClient);
  public baseUrl: string;

  constructor() {
    this.baseUrl = `${environment.Setting.apiServiceUrl}/api/v1/Account`;
  }

  /**
   * Request Access for  new user.
   * @param {RequestAccessDto} viewModel request access detail view model.
   */
  requestAccess(viewModel: RequestAccessDto): Observable<ApiOkResponse<any>> {
    const url = `${this.baseUrl}/request-access`;
    viewModel.callbackUrl = "test";
    return this._http.post<ApiOkResponse<any>>(url, viewModel);
  }

  /**
   * api to send e-mail for forgot password!
   * @param {ForgotPasswordRequest} viewModel model for new password.
   */
  forgotPassword(viewModel: ForgotPasswordRequest): Observable<ApiOkResponse<any>> {
    const url = `${this.baseUrl}/forgot-password`;
    viewModel.returnUrl = `${window.location.origin}${environment._environmentSetting?.rootURL}auth/set-password`;
    return this._http.post<ApiOkResponse<any>>(url, viewModel);
  }

  /**
   * api to change/set the password for a given user!
   * @param {ResetPasswordDto} viewModel model for new password.
   */
  resetPassword(viewModel: ResetPasswordDto): Observable<ApiOkResponse<any>> {
    const url = `${this.baseUrl}/reset-password`;
    return this._http.post<ApiOkResponse<any>>(url, viewModel);
  }

  //#region Google captcha related functions
  getCaptchaSetting(): CaptchaSetting {
    let captchaSetting = new CaptchaSetting();
    captchaSetting.siteKey = environment.Setting.captchaKey;
    return captchaSetting;
  }
  //#endregion Google captcha related functions

}
