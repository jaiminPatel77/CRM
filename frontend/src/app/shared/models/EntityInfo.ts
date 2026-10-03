export enum EnumEntityType
{
    Unknown = 0, //Not applicable or unknown!
    USER = 1,
    ROLE = 2,
    SETTING = 3,
}

export enum EnumEntityEvents
{
        // Information related events
        COMMON_LIST_ALL_ITEMS = 1000,
        COMMON_LIST_FILTER_ITEMS = 1001,
        COMMON_GET_ITEM = 1002,
        COMMON_CREATE_ITEM = 1003,
        COMMON_UPDATE_ITEM = 1004,
        COMMON_DELETE_ITEM = 1005,

        // Error one start from here!
        COMMON_GET_ITEM_NOTFOUND = 4000,
        COMMON_UPDATE_ITEM_NOTFOUND = 4001,
        COMMON_DELETE_ITEM_NOTFOUND = 4002,

        // Any common CURD operation failed because of any exception. refer exception for detail.
        COMMON_LIST_EXCEPTION = 4003,
        COMMON_GET_EXCEPTION = 4004,
        COMMON_POST_EXCEPTION = 4005,
        COMMON_PUT_EXCEPTION = 4006,
        COMMON_DELETE_EXCEPTION = 4007,


        // Special Operations for user xxxyyyy - format
        // 001 = entity type
        // 0001 = special operation
        USER_LOGIN = 10001,
        USER_REGISTER = 10002,
        USER_RENEW_TOKEN = 10003,
        USER_FORGOT_PASSWORD = 10004,
        USER_RESET_PASSWORD = 10005,
        

        USER_LOGIN_FAILED = 10501, //error start here
        USER_RENEW_TOKEN_FAILED = 10502,
        USER_REGISTER_FAILED = 10503,
        USER_FORGOT_PASSWORD_FAILED = 10504,
        USER_RESET_PASSWORD_FAILED = 10505,        
}