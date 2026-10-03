import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { ApiCreatedResponse, ApiError, ApiOkResponse } from '../models/api-response';
import { Observable, catchError, throwError } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { UserPermissionService } from './user-permission.service';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class CommonService {

  public toastrService = inject(ToastService);
  public translateService = inject(TranslateService);
  public http = inject(HttpClient);
  public modalService = inject(NgbModal);
  public permissionService = inject(UserPermissionService);

  /**
   * extract Ok response of any web api.
   * @param {Observable<ApiOkResponse<T>>} response web api response.
   * @returns {Observable<ApiOkResponse<T>>} return ApiOkResponse.
   */
  extractOkResponse<T>(response: Observable<ApiOkResponse<T>>): Observable<ApiOkResponse<T>> {
    return response.pipe(
      catchError(this.handleError)
    );
  }

  /**
   * extract create response of any web api.
   * @param {Observable<ApiCreatedResponse<T>>} response web api response.
   * @returns {Observable<ApiCreatedResponse<T>>} return ApiCreatedResponse.
   */
  extractCreateResponse<T>(response: Observable<ApiCreatedResponse<T>>): Observable<ApiCreatedResponse<T>> {
    return response.pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Handle wel api error response.
   * @param {HttpErrorResponse} error error response from web api.
   * @returns {Observable<ApiError>} ApiError information.
   */
  handleError(error: HttpErrorResponse): Observable<any> {
    let formattedError = CommonService.formatError(error);
    return throwError(() => formattedError);
  }

  /**
  * Format web api response in proper ApiError object form.
  *  @param {HttpErrorResponse} error error response from web api.
  *  @returns {ApiError} format error information in as ApiError.
  * 
  */
  static formatError(error: HttpErrorResponse): ApiError {
    if (error.error instanceof ErrorEvent) {

      let errMsg = error.message ? error.message : `Api Error ${error?.error?.message.toString()}`;
      let apiError = new ApiError();
      apiError.errorMessage = errMsg;
      apiError.statusCode = 400;
      apiError.statusText = "Bad request";
      return apiError;

    }
    else if (error instanceof ApiError) {
      //throw the same object!
      return error;
    }
    else {

      //response from api
      let apiError = new ApiError();
      apiError.url = error.url;
      apiError.statusCode = error.status;
      if (error.statusText) {
        apiError.statusText = error.statusText;
      }
      //if error is return as Object.. let read more info!
      if (error.status === 0) {
        //Do nothing for service not available..
      }
      else if (error.error instanceof Object) {
        //Entity code sent ?
        if (error.error.entityCode) {
          apiError.entityCode = error.error.entityCode;
        }
        if (error.error.eventCode) {
          apiError.eventCode = error.error.eventCode;
        }
        //copy as message or object as required.
        if (error.error.errorDetail) {

          //JSON.stringify(error.error.errorDetail)
          apiError.errorDetail = error.error.errorDetail;;
        }
        //set the error message
        if (error.error.eventMessageId) {
          apiError.eventMessageId = error.error.eventMessageId;
        }
        else {
          const errors = [];
          for (const key in error.error) {
            const errorInfo = error.error[key];
            if (errorInfo instanceof Array) {
              for (const element of errorInfo) {
                errors.push(element);
              }
            } else {
              errors.push(errorInfo);
            }
          }
          apiError.errorDetail = errors.join();
        }
      }
      else {
        //response is not an Object
        if (apiError.statusCode === 403) {
          apiError.errorMessage = "You do not have enough permissions.";
        }
        else {
          apiError.errorMessage = error.error;
        }
      }
      //Check error type! and throw accordingly,
      if (apiError.statusCode === 0) {
        apiError.statusText = "Service not available";
        apiError.errorMessage = "Unable to connect to api service(s)!";

        return apiError;
      } else {
        return apiError;
      }
    }
  }
}
