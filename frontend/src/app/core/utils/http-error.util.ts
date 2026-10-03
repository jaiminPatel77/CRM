import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from '../models/api-error';

export class HttpErrorUtil {

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
            return error; // Already formatted
        }
        else {
            let apiError = new ApiError();
            apiError.url = error.url;
            apiError.statusCode = error.status;
            if (error.statusText) {
                apiError.statusText = error.statusText;
            }

            if (error.status === 0) {
                // Service not available
            }
            else if (error.error instanceof Object) {
                if (error.error.entityCode) {
                    apiError.entityCode = error.error.entityCode;
                }
                if (error.error.eventCode) {
                    apiError.eventCode = error.error.eventCode;
                }
                if (error.error.errorDetail) {
                    apiError.errorDetail = error.error.errorDetail;
                }
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
                    apiError.errorDetail = errors.join(', ');
                }
            }
            else {
                // response is not an Object
                if (apiError.statusCode === 403) {
                    apiError.errorMessage = "You do not have enough permissions.";
                }
                else {
                    apiError.errorMessage = error.error;
                }
            }

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
