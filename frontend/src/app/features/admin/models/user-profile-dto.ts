

//Define User Profile related Model dto !

import { BaseDto } from "@shared/models/base-model";

export class UserProfileDto extends BaseDto {
    title?: string;
    email?: string;
    fullName?: string;
    phoneNumber?: string;
    isImageAvailable?: boolean;
}