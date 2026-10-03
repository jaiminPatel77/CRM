//Define base model details

import { IBaseList } from "./common-models";

/**
 * Base class for all Model Dto Which define common PK for all entity!
 */
export class BaseDto implements IBaseList {
    id?: number;
    $selected?: boolean;
}

/**
 * Base Look Up class for all drop downs.
 */
export class BaseLookUpDto {
    id?: number;
    name?: string;
}

/**
 * Base Dto class with common fields!  
 */
export class BaseDtoWithCommonFields extends BaseDto {
    createdOn?: Date;
    createdById?: number;
    modifiedOn?: Date;
    modifiedById?: number;
    disabled?: boolean;
    enabledDisabledOn?: Date;
}

/**
 * Base Dto class with common fields and address fields!  
 */
export class BaseDtoWithAddress extends BaseDtoWithCommonFields {

    addressLine1?: string;
    addressLine2?: string;
    city?: string;
    state?: string;
    country?: string;
    postalCode?: string;
    lat?: number;
    lng?: number;
    email?: string;
    phone?: string;
    mobile?: string;
}

/**
 * Base Dto class with name , description and common fields!  
 */
export class BaseDtoWithNameDescription extends BaseDtoWithCommonFields {
    name?: string ;
    description?: string ;
}

/**
 * Setting for confirm model dialog.
 */
export class ConfirmDlgSetting {
    constructor(
        public titleTextId: string = "TITLE_WARNING",
        public messageTextId: string = "DELETE_CONFIRM",
        public okBtnTextId: string = "BUTTON_OK",
        public cancelBtnTextId: string = "BUTTON_CANCEL",
    ) { }
}

/**
 * common object to hold evaluated permission for a particular view/component.
 */
export class EvaluatedPermission {
    constructor(
        public viewAccess: boolean = false,
        public updateAccess: boolean = false,
        public createAccess: boolean = false,
        public deleteAccess: boolean = false,
        public approveAccess: boolean = false,
    ) { }
}



