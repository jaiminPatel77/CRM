/**
 * Authentication API token response
 */
export interface JwtResponse {
  id?: string;
  access_token?: string;
  accessToken?: string; // Support camelCase
  refresh_token?: string;
  refreshToken?: string; // Support camelCase
  expiration?: number;
  expires_in?: number;
  expiresIn?: any; // Support camelCase (can be Date string or number)
}

/**
 * Auth token interface
 */
export interface AuthToken {

  /**
   * Original jwtResponse from server.
   */
  jwtResponse: JwtResponse | undefined;

  /**
   * Returns the token value
   * @returns string
   */
  token(): string | undefined;

  /**
  * Returns the refresh token value
  * @returns string
  */
  refreshToken(): string | undefined;


  /**
   * Is data expired
   * @returns {boolean}
   */
  isValid(): boolean | undefined;

  /**
   * Returns Token Exp. Date information.
   * @returns Date
   */
  getTokenExpDate(): Date | undefined;

  /**
   * return user id associated with current token.
   */
  userId(): number;

  /**
   * return tenant id associated with current token if available.
   */
  tenantId(): string | undefined;
}


/**
 * Wrapper for JWT token  which implement AuthToken interface
 */
export class AuthJWTToken implements AuthToken {

  private _validUpTo?: Date = undefined;
  private _decodedToken: any = null;

  constructor(public readonly jwtResponse: JwtResponse | undefined) {
    const token = this.token();
    if (token) {
      this._decodedToken = this.decodeJwt(token);
    }
  }

  private decodeJwt(token: string): any {
    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch (e) {
      return null;
    }
  }

  /**
   * Returns the refresh token value
   * @returns string
   */
  refreshToken(): string | undefined {
    return this.jwtResponse ? (this.jwtResponse.refresh_token || this.jwtResponse.refreshToken) : undefined;
  }

  /**
   * Returns the token value
   * @returns string
   */
  token(): string | undefined {
    return this.jwtResponse ? (this.jwtResponse.access_token || this.jwtResponse.accessToken) : undefined;
  }

  /**
   * Is data expired
   * @returns {boolean}
   */
  isValid(): boolean {
    return this.token() && (new Date() < this.getTokenExpDate()) ? true : false;
  }

  /**
   * Returns Token Exp. Date information.
   * @returns Date
   */
  getTokenExpDate(): Date {
    if (!this._validUpTo) {
      if (this._decodedToken && this._decodedToken.exp) {
        const date = new Date(0);
        date.setUTCSeconds(this._decodedToken.exp);
        this._validUpTo = date;
      } else if (this.jwtResponse && (this.jwtResponse.expiration || this.jwtResponse.expiresIn)) {
        // Fallback to legacy response property
        const exp = this.jwtResponse.expiration || this.jwtResponse.expiresIn;
        if (exp instanceof Date) {
          this._validUpTo = exp;
        } else if (typeof exp === 'string') {
          this._validUpTo = new Date(exp);
        } else {
          const date = new Date(0);
          date.setUTCSeconds(exp);
          this._validUpTo = date;
        }
      } else {
        this._validUpTo = new Date(0);
      }
    }
    return this._validUpTo;
  }


  /**
  * return user id associated with current token.
  * @returns string
  */
  userId(): number {
    if (this._decodedToken) {
      // Check standard claim names: nameid, sub, or .NET specific long types
      const id = this._decodedToken['id']
        || this._decodedToken['nameid']
        || this._decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
        || this._decodedToken['sub'];

      if (id) return Number(id);
    }

    // Fallback to legacy response property
    let idVal = this.jwtResponse ? this.jwtResponse.id : undefined;
    if (!idVal) {
      return 0;
    } else {
      return Number(idVal);
    }
  }

  /**
   * return tenant id associated with current token.
   */
  tenantId(): string | undefined {
    if (this._decodedToken) {
      return this._decodedToken['tenant_id'] || this._decodedToken['TenantId'] || undefined;
    }
    return undefined;
  }
}

export interface AuthTokenDto {
  token: string;
  refreshToken: string;
  expiresIn: number;
}