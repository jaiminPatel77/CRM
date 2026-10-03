import { EnumEntityType, EnumEntityEvents } from "./EntityInfo";

/**
 * Represent API error information 
*/
export class ApiError {
    entityCode?: EnumEntityType;
    eventCode?: EnumEntityEvents;
    statusCode?: number;
    statusText?: string;
    eventMessageId?: string
    errorMessage?: string;
    errorDetail?: string;
    url?: string | null;
}

/**
 * Represent API Response
*/
export class ApiResponse {
    eventCode?: EnumEntityType;
    entityCode?: EnumEntityEvents;
    eventMessageId?: string;
}

/**
 * Represent API Ok Response
*/
export class ApiOkResponse<T> extends ApiResponse
{
    data?: T
}

/**
 * Represent API Create Response with  with location for newly created resource URI
*/
export class ApiCreatedResponse<T> extends ApiOkResponse<T>
{
    location?: string
}