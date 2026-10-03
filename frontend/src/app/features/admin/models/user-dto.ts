import { BaseDto, BaseLookUpDto, BaseDtoWithCommonFields } from "@shared/models/base-model";
import { EnumUserType, EnumUserStatus, EnumPermissionFor } from "@shared/models/common-enums";
import { RoleLookUpDto, PermissionDto } from "./role-dto";

/**
 * User lookup model for role or other combo!
 */
export class UserLookUpDto extends BaseDto {

  title?: string;
  email?: string;
  fullName?: string;
  phoneNumber?: string;
  isImageAvailable?: boolean;
  isDisabled?: boolean;
  userType?: EnumUserType

}



/**
 * Model class for User list/Table view!
 */
export class UserListDto extends BaseDtoWithCommonFields {

  title?: string;
  email?: string;
  fullName?: string;
  phoneNumber?: string;
  isImageAvailable?: boolean;
  isDisabled?: boolean;
  userType?: EnumUserType;
  status?: EnumUserStatus;
  lastLogin?: Date;
  lastLogout?: Date;
  lastEnableDisable?: Date;
  isLockedOut?: boolean;

  $inviteUserLoading?: boolean;

}



/**
 * Dto model class for user detail page!
 */
export class UserDto extends BaseDto {
  email?: string;
  fullName?: string;
  title?: string;
  phoneNumber?: string;
  isImageAvailable?: boolean;
  isDisabled?: boolean;
  userType?: EnumUserType;
  status?: EnumUserStatus;
  lastLogin?: Date;
  lastLogout?: Date;
  lastEnableDisable?: Date;
  roles?: string[] = [];
  image?: string;
}

/**
 * Define type for Dictionary for each Entity type string value as key and associated permission information!
 */
export type PermissionType = { [key in keyof typeof EnumPermissionFor]: PermissionDto };

/**
 * User with permission detail.. used for current user!
 */
export class UserWithPermissionsDto extends UserDto {
  public permissionsList: PermissionDto[] = [];
  constructor(
    public permissions?: PermissionType
  ) {
    super();
    let noPermission: any = {};
    this.permissions = noPermission;
  }
}

/**
 * Reset password request.
 */
export class ResetPasswordRequest {
  constructor(
    public userId: number,
    public authUrl: string,
  ) { }
}

/**
   * User change password Dto
   */
export class ChangePasswordDto extends BaseDto {
  userId?: number;
  oldPassword?: string;
  newPassword?: string;
  $confirmPassword?: string;
}


