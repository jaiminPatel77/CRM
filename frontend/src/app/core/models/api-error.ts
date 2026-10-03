export class ApiError {
    entityCode?: any; // Replace with EnumEntityType if needed, or keep as any for Core decoupling
    eventCode?: any;
    statusCode?: number;
    statusText?: string;
    eventMessageId?: string
    errorMessage?: string;
    errorDetail?: string;
    url?: string | null;
}
