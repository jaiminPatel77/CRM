

//Define role related Model dto !

import { BaseDto, BaseDtoWithCommonFields } from "@shared/models/base-model";
import { EnumRoleType, EnumPermissionFor, EnumPermissions } from "@shared/models/common-enums";
import { UserLookUpDto } from "./user-dto";

/**
* Base Dto class with common fields!  
*/
export class RoleLookUpDto extends BaseDto {
    name?: string;
    roleType?: EnumRoleType;

}

/**
 * Model class for Role list/Table view!
 */
export class RoleListDto extends BaseDtoWithCommonFields {

    name?: string;
    description?: string;
    roleType?: EnumRoleType;

}

/**
 * Permission dto
 */
export class PermissionDto extends BaseDto {

    permissionFor!: EnumPermissionFor
    permissions!: EnumPermissions

    //#region specific permission access helper functions

    public textId(): string {
        return "PERMISSION_" + EnumPermissionFor[this.permissionFor];
    }

    public get viewAccess(): boolean {
        return (this.permissions & EnumPermissions.ViewAccess) == EnumPermissions.ViewAccess;
    }

    public set viewAccess(val: boolean) {
        if (val) {
            this.permissions = this.permissions | EnumPermissions.ViewAccess;
        } else {
            this.permissions = EnumPermissions.None;
        }
    }

    public get updateAccess(): boolean {
        return (this.permissions & EnumPermissions.UpdateAccess) == EnumPermissions.UpdateAccess;
    }

    public set updateAccess(val: boolean) {
        if (val) {
            this.permissions = this.permissions | EnumPermissions.ViewUpdateAccess;
        } else {
            this.permissions = this.permissions & (EnumPermissions.ViewAccess);
        }
    }

    public get createAccess(): boolean {
        return (this.permissions & EnumPermissions.CreateAccess) == EnumPermissions.CreateAccess;
    }

    public set createAccess(val: boolean) {
        if (val) {
            this.permissions = this.permissions | EnumPermissions.ViewUpdateCreateAccess;
        } else {
            this.permissions = this.permissions & (EnumPermissions.ViewUpdateAccess);
        }
    }

    public get deleteAccess(): boolean {
        return (this.permissions & EnumPermissions.DeleteAccess) == EnumPermissions.DeleteAccess;
    }

    public set deleteAccess(val: boolean) {
        if (val) {
            this.permissions = this.permissions | EnumPermissions.CURDAccess;
        } else {
            this.permissions = this.permissions & (EnumPermissions.ViewUpdateCreateAccess);
        }
    }

    public get allAccess(): boolean {
        return (this.permissions & EnumPermissions.CURDAccess) == EnumPermissions.CURDAccess;
    }

    public set allAccess(val: boolean) {
        if (val) {
            this.permissions = this.permissions | EnumPermissions.CURDAccess;
        } else {
            this.permissions = EnumPermissions.None;
        }
    }

    public get approveAccess(): boolean {
        return (this.permissions & EnumPermissions.ApproveAccess) == EnumPermissions.ApproveAccess;
    }

    public set approveAccess(val: boolean) {
        if (val) {
            this.permissions = this.permissions | EnumPermissions.ApproveAccess;
        } else {
            this.permissions = this.permissions & (EnumPermissions.CURDAccess);
        }
    }

    //#endregion
}

/**
 * Model class for Role Detail view!
 */
export class RoleDto extends BaseDtoWithCommonFields {

    permissions: PermissionDto[] = [];
    name?: string;
    description?: string;
    roleType?: EnumRoleType;
    users: UserLookUpDto[] = []



}