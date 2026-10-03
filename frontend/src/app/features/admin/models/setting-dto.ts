import { BaseDtoWithCommonFields } from "@shared/models/base-model";
import { EnumSettingType, EnumSettingCategory, EnumSettingStatus } from "@shared/models/common-enums";

/**
 * Dto model class for user detail page!
 */
export class SettingDto<T> extends BaseDtoWithCommonFields {

    settingType?: EnumSettingType;
    settingCategory?: EnumSettingCategory;
    storageFormat?: number;
    isEncrypted?: boolean;
    status?: EnumSettingStatus;
    value?: T;

}
/**
 * SMTP setting dto
 */
export class SMTPSetting {
    userName?: string;
    password?: string;
    serverAddress?: string;
    serverPort?: number;
    isSSL?: boolean;
    fromEmail?: string;

}