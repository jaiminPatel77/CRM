import { Injectable, inject, signal, computed } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { BaseService } from '@shared/services/base.service';
import { LoginViewModel, CaptchaSetting, ForgotPasswordRequest, ResetPasswordViewModel } from '../models/account-model';
import { AuthJWTToken, AuthToken, JwtResponse } from '../models/token';
import { BehaviorSubject, Observable, filter, map, mergeMap, of, share, timer, Subject } from 'rxjs';
import { Router } from '@angular/router';
import { ApiOkResponse } from '@shared/models/api-response';
import { environment } from '@env/environment';
import { HttpRequest } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AuthService extends BaseService<LoginViewModel> {

  //#region Signals for reactive state

  // Session expired signal - replaces EventEmitter
  private _sessionExpired = signal<boolean>(false);
  readonly sessionExpired$ = toObservable(this._sessionExpired);

  // Token signal for reactive updates
  protected _token = signal<AuthToken | undefined>(undefined);
  protected _token$ = toObservable(this._token);

  // Computed signal for authentication status
  readonly isLoggedIn = computed(() => this._token()?.isValid() ?? false);

  //#endregion

  //#region Private fields

  private readonly STORAGE_KEYS = {
    getSession: 'getSessionStorage',
    session: 'sessionStorage',
    removeSession: 'removeSessionStorage',
    refreshTokenStarted: 'refreshTokenStartedStorage',
    refreshToken: 'refreshTokenStorage',
    shareTimeout: 'shareTimeoutStorage',
    authToken: 'crm_auth_token'
  } as const;

  protected key = this.STORAGE_KEYS.authToken;
  private _remember?: boolean;
  private _jwtResponse?: JwtResponse;
  private _checkSession?: boolean;
  private _authJWTToken?: AuthJWTToken;
  private _tokenRefreshTimer?: ReturnType<typeof setTimeout>;
  private _refreshTokenStarted = false;
  private _ignoreUrls: string[] = [];
  private readonly DEFAULT_TIME_OUT = 20 * 60;
  private _idleTimerOut = this.DEFAULT_TIME_OUT;
  private _idleTimer?: ReturnType<typeof setTimeout>;

  // Subject for authentication changes
  private _authenticationChange$ = new Subject<boolean | undefined>();

  //#endregion

  //#region Public properties

  redirectUrl = '/dashboard';
  issueUrl?: string;
  isSessionPopupShown = false;

  // Expose token as signal
  readonly token = this._token.asReadonly();

  // Expose isAuthenticated as observable
  readonly isAuthenticated$ = this._token$.pipe(
    filter((token): token is AuthToken => !!token),
    map((token) => token.isValid()),
    share()
  );

  //#endregion

  private readonly _router = inject(Router);

  constructor() {
    super(BaseService.ApiUrls.Auth);
    this._initTokenInfoAndEvent();
    this._ignoreUrls = ['/Auth/logout'];
  }

  //#region JWT token handling methods

  /**
   * @ignore
   * One time init from ctor for token from storage and event binding for storage
   */
  private _initTokenInfoAndEvent() {
    this._remember = false;
    this.jwtResponse = localStorage.getItem(this.key) ?? undefined;
    if (this._jwtResponse) {
      this._remember = true;
    } else {
      this.askSessionStorageDetailFromOtherTabs();
      this._checkSession = true;
    }
    this.publishToken();
    this.startStorageEventListener();
  }

  /**
   * @ignore
   * set jwtResponse from JSON string store in session/local store.
   */
  private set jwtResponse(rawValue: string | undefined) {
    if (rawValue) {
      this._jwtResponse = JSON.parse(rawValue);
    } else {
      this._jwtResponse = undefined;
    }
  }

  /**
   * Set token detail as per JwtResponse and fire publish event for the same.
   * @param token JwtResponse from login api.
   * @param remember true will remember token detail in localStorage otherwise in sessionStorage.
   */
  setToken(token: JwtResponse, remember: boolean | undefined): boolean {
    this._jwtResponse = token;
    const strVal = JSON.stringify(token);
    this._remember = remember;
    if (remember) {
      localStorage.setItem(this.key, strVal);
    } else {
      sessionStorage.setItem(this.key, strVal);
    }

    if (this._refreshTokenStarted) {
      this._refreshTokenStarted = false;
      localStorage.setItem(this.STORAGE_KEYS.refreshToken, strVal);
      localStorage.removeItem(this.STORAGE_KEYS.refreshToken);
    }
    this._checkSession = false;
    this.publishToken();
    return true;
  }

  /**
   * return the current AuthToken detail. null if yet not Authenticated!
   */
  public get getCurrentToken(): AuthToken | undefined {
    return this._authJWTToken;
  }

  /**
   * clear token information from memory and local/session storage. also fire session storage event to notify other tab!
   * and navigate to login page!
   */
  clearToken() {
    localStorage.setItem(this.STORAGE_KEYS.removeSession, Date.now().toString());
    localStorage.removeItem(this.STORAGE_KEYS.removeSession);
    this._clearToken();
  }

  /**
   * @ignore clear token detail from local and session storage. and publish event for the same.
   */
  private _clearToken() {
    localStorage.removeItem(this.key);
    sessionStorage.removeItem(this.key);
    this._jwtResponse = undefined;
    this._remember = false;
    this.publishToken();
    this._router.navigate(['/auth']);
  }

  /**
   * publish new token value.
   */
  protected publishToken() {
    this._authJWTToken = new AuthJWTToken(this._jwtResponse);
    this.setRefreshTokenTimer();
    this._token.set(this._authJWTToken);
    this._authenticationChange$.next(this._authJWTToken?.isValid());
  }

  /**
   * set the timer to refresh token before access_token get expired.
   * before three Minute ... the time callback will refresh token from server.
   */
  protected setRefreshTokenTimer() {
    if (this._tokenRefreshTimer) {
      clearTimeout(this._tokenRefreshTimer);
      this._tokenRefreshTimer = undefined;
    }

    if (this._authJWTToken && this._authJWTToken.isValid()) {
      const threeMinute = 180;
      let timeOut = this._authJWTToken?.jwtResponse?.expires_in;
      if (timeOut) {
        if (timeOut > threeMinute) {
          timeOut = timeOut - threeMinute;
        }
        timeOut = timeOut * 1000;
        this._tokenRefreshTimer = setTimeout(() => {
          this.refreshToken();
        }, timeOut);
      }
    }
  }

  /**
   * Call refreshTokenFromServer if not already start by other tab if any.
   * if success then it will set new token otherwise app will redirected to login page!
   */
  protected refreshToken() {
    if (!this._refreshTokenStarted) {
      this._refreshTokenStarted = true;
      localStorage.setItem(this.STORAGE_KEYS.refreshTokenStarted, Date.now().toString());
      localStorage.removeItem(this.STORAGE_KEYS.refreshTokenStarted);
      this.refreshTokenFromServer().subscribe({
        next: () => {},
        error: (error) => this.logger.error('Token refresh failed', error)
      });
    }
  }

  /**
   * true if authenticated and have valid token.
   */
  isAuthenticated(): Observable<boolean> {
    if (this._checkSession) {
      return timer(500).pipe(
        map(() => {
          this._checkSession = false;
          this.jwtResponse = sessionStorage.getItem(this.key) ?? undefined;
          if (this._jwtResponse) {
            this.publishToken();
          }
          return this._authJWTToken?.isValid() ?? false;
        })
      );
    } else {
      const isValid = this._authJWTToken?.isValid() ?? false;
      if (!isValid && this._authJWTToken?.refreshToken()) {
        return this.refreshTokenAtStartUp();
      }
      return of(isValid);
    }
  }

  /**
   * refresh the token at startup if saved access_token is exp. and refresh_token is there!
   * make actual http call for refresh token as applicable.
   */
  protected refreshTokenAtStartUp(): Observable<boolean> {
    if (!this._refreshTokenStarted) {
      this._refreshTokenStarted = true;
      localStorage.setItem(this.STORAGE_KEYS.refreshTokenStarted, Date.now().toString());
      localStorage.removeItem(this.STORAGE_KEYS.refreshTokenStarted);
      return this.refreshTokenFromServer().pipe(
        map(() => true)
      );
    }
    return of(false);
  }

  /**
   * event for Authentication change
   */
  onAuthenticationChange(): Observable<boolean | undefined> {
    return this.onTokenChange().pipe(
      map((token: AuthToken | undefined) => token?.isValid())
    );
  }

  /**
   * event for Token value change.
   */
  onTokenChange(): Observable<AuthToken | undefined> {
    return this._token$.pipe(
      filter((value): value is AuthToken => !!value),
      share()
    );
  }

  //#endregion

  //#region Sharing token/session between tabs

  /**
   * @ignore ask session storage detail from other tab if any.
   */
  private askSessionStorageDetailFromOtherTabs(): void {
    localStorage.setItem(this.STORAGE_KEYS.getSession, Date.now().toString());
    localStorage.removeItem(this.STORAGE_KEYS.getSession);
  }

  /**
   * @ignore start listen to Storage event..
   */
  private startStorageEventListener(): void {
    window.addEventListener('storage', this.storageEventListener.bind(this));
  }

  /**
   * actual storage event listener function to share session between tabs!
   */
  private storageEventListener(event: StorageEvent): void {
    if (event.storageArea === localStorage) {
      if (event.key === this.STORAGE_KEYS.getSession) {
        if (sessionStorage && sessionStorage.length) {
          const sessionStorageString = JSON.stringify(sessionStorage);
          localStorage.setItem(this.STORAGE_KEYS.session, sessionStorageString);
          localStorage.removeItem(this.STORAGE_KEYS.session);
        }
      } else if (event.key === this.STORAGE_KEYS.session && event.newValue) {
        const data = JSON.parse(event.newValue);
        for (const key in data) {
          sessionStorage.setItem(key, data[key]);
        }
      } else if (event.key === this.STORAGE_KEYS.removeSession && event.newValue) {
        if (this._jwtResponse) {
          this._clearToken();
        }
      } else if (event.key === this.STORAGE_KEYS.refreshTokenStarted && event.newValue) {
        if (!this._refreshTokenStarted) {
          this._refreshTokenStarted = true;
        }
      } else if (event.key === this.STORAGE_KEYS.refreshToken && event.newValue) {
        if (this._refreshTokenStarted) {
          this._refreshTokenStarted = false;
          this.jwtResponse = event.newValue;
        }
      }
    }
  }

  //#endregion

  //#region Auth related view/Page apis

  /**
   * login user with given user/email and password.
   */
  login(viewModel: LoginViewModel): Observable<ApiOkResponse<unknown>> {
    const url = '/login';
    viewModel.userName = viewModel.email;
    return this.postResponse<LoginViewModel, unknown>(this.baseUrl, url, viewModel).pipe(
      map((res) => this.processResultToken(res, viewModel.rememberMe))
    );
  }

  /**
   * make http call to refreshToken from server!
   */
  protected refreshTokenFromServer(): Observable<ApiOkResponse<unknown>> {
    const url = '/renew-token';
    return this.postResponse<JwtResponse, unknown>(this.baseUrl, url, this._jwtResponse!).pipe(
      map((res) => this.processResultToken(res, this._remember))
    );
  }

  /**
   * forgot password request
   */
  forgotPassword(model: ForgotPasswordRequest): Observable<ApiOkResponse<unknown>> {
    const url = '/forgot-password';
    return this.postResponse(this.baseUrl, url, model);
  }

  /**
   * reset password request
   */
  resetPassword(model: ResetPasswordViewModel): Observable<ApiOkResponse<unknown>> {
    const url = '/reset-password';
    // Map view model to what the API expects if necessary, 
    // but here we just pass the model as the component uses this model.
    const request = {
      email: model.email,
      token: model.code,
      newPassword: model.password
    };
    return this.postResponse(this.baseUrl, url, request);
  }

  /**
   * register new user
   */
  register(model: { email: string; password: string; fullName: string }): Observable<ApiOkResponse<unknown>> {
    const url = '/register';
    return this.postResponse(this.baseUrl, url, model).pipe(
      map((res) => this.processResultToken(res, false))
    );
  }

  /**
   * Google captcha related functions
   */
  getCaptchaSetting(): CaptchaSetting {
    const captchaSetting = new CaptchaSetting();
    captchaSetting.siteKey = environment.Setting.captchaKey;
    return captchaSetting;
  }

  /**
   * logout the currently logged in user and clear associated token information!
   * and navigate to login page.
   */
  logout(): void {
    const url = '/logout';
    if (this._jwtResponse) {
      this.postResponse<JwtResponse, unknown>(this.baseUrl, url, this._jwtResponse).subscribe({
        next: () => this.clearToken(),
        error: (error) => {
          this.logger.error('Logout failed', error);
          this.clearToken();
        }
      });
    }
  }

  /**
   * @param result JwtResponse information.
   * @param rememberMe indicate whether to remember the token or not.
   */
  private processResultToken(result: ApiOkResponse<unknown>, rememberMe: boolean | undefined): ApiOkResponse<unknown> {
    if (result.data) {
      const response = result.data as JwtResponse;
      this.setToken(response, rememberMe);
    }
    return result;
  }

  //#endregion

  //#region Idle timeout related

  setIdleTimer(req: HttpRequest<unknown>): void {
    if (this.isSessionPopupShown) {
      const isIgnore = req ? this.ignoreAutoApiCall(req) : true;
      if (!isIgnore) {
        this.setActualIdleTimer();
      }
    }
  }

  ignoreAutoApiCall(req: HttpRequest<unknown>): boolean {
    if (req.url && this._ignoreUrls) {
      return this._ignoreUrls.some((apiUrl) => req.url.includes(apiUrl));
    }
    return false;
  }

  setActualIdleTimer(doNotNotify = false): void {
    let timeOut = this._idleTimerOut;
    if (timeOut) {
      const oneMinute = 60;
      if (timeOut > oneMinute) {
        timeOut = (timeOut - oneMinute) * 1000;
      } else {
        timeOut = timeOut * 1000;
      }

      this.clearActualIdleTimer();
      this._idleTimer = setTimeout(() => {
        this._sessionExpired.set(true);
      }, timeOut);

      if (!doNotNotify) {
        localStorage.setItem(this.STORAGE_KEYS.shareTimeout, Date.now().toString());
        localStorage.removeItem(this.STORAGE_KEYS.shareTimeout);
      }
    }
  }

  clearActualIdleTimer(): void {
    if (this._idleTimer) {
      clearTimeout(this._idleTimer);
      this._idleTimer = undefined;
    }
  }

  //#endregion

  //#region Logger helper

  private logger = {
    error: (message: string, error: unknown): void => {
      console.error(`[AuthService] ${message}`, error);
    }
  };

  //#endregion
}
