// Define view models for auth workflows. e.g. login, forgot password, reset-password etc..


/**
 * Client type for login.
 */
export enum EnumClientType
{
    Web = 0, //Web browser.
    AndroidApp = 1, //Android mobile application.
    AppleApp = 2, //Ios mobile application.
}

/**
 * Base model for any login!
 */
export class LoginBaseModel {
    constructor(
        public clientType: EnumClientType = 0, //web client type
        public deviceId?: string,
        public rememberMe?: boolean) {
    }
  }

/**
 * Login page view model
 */
export class LoginViewModel extends LoginBaseModel {
    constructor(
        public email?: string,
        public userName?: string,
        public password?: string) {
            super();
    }
  }

  /**
   * Register new user view model.
   */
  export class RegistrationViewModel {
    constructor(
        public userName?: string,
        public email?: string,
        public password?: string,
        public confirmPassword?: string,
        public displayName?: string,
        public secretCode?: string,
    ) {
    }
  }

  /**
   * forgot password view model
   */
  export class ForgotPasswordViewModel {
    constructor(
        public email?: string,
        public returnUrl?: string) {
    }
  }

  /**
   * reset password view model
   */
  export class ResetPasswordViewModel {
    constructor(
        public email?: string,
        public password?: string,
        public confirmPassword?: string,
        public code?: string) {
    }
  }

  /**
   * Model for Google Captcha setting with default values.
   */
  export class CaptchaSetting {

    constructor(
        public siteKey?: string,
        public useGlobalDomain: boolean = false,
        public theme: 'light' | 'dark' = 'light',
        public size: 'compact' | 'normal' = 'normal',
        public lang: string = 'en',
        public type: 'audio' | 'image' = 'image',
        public response?: string,
        public isLoaded: boolean = false,        
        ) {
    }
  }

/**
 * Model for Request Access API/Form!
 */
  export class RequestAccessDto {

    constructor(
        public fullName?: string,
        public email?: string,
        public phoneNumber?: string,
        public reasonForAccess?: string,
        public secretCode?: string,
        public callbackUrl?: string,
        ) {
    }
  }

  /**
   * Model for Init Forgot password request!
   */
  export class ForgotPasswordRequest {

    constructor(
        public email?: string,
        public returnUrl?: string,
        public secretCode?: string,
        ) {
    }
  }

  /**
   * Model for Reset password request!
   */
  export class ResetPasswordDto {
    constructor(
        public email?: string,
        public password?: string,
        public code?: string | null,
        ) {
    }
  }