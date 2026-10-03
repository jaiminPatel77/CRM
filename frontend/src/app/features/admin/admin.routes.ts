import { Routes } from '@angular/router';
import { authGuard } from '../../models/route-gaurds/auth.guard';
import { UserListComponent } from './components/user-list/user-list.component';
import { EnumPermissionFor, EnumPermissions } from '@shared/models/common-enums';
import { UserDetailComponent } from './components/user-detail/user-detail.component';
import { RoleDetailComponent } from './components/role-detail/role-detail.component';
import { MailSettingComponent } from './components/mail-setting/mail-setting.component';
import { RoleListComponent } from './components/role-list/role-list.component';
import { LogsListComponent } from './components/logs-list/logs-list.component';
import { BackupListComponent } from './components/backup-list/backup-list.component';
import { AuditLogListComponent } from './components/audit-log-list/audit-log-list.component';

export const adminRoutes: Routes = [
    {
        path: 'users',
        canActivate: [authGuard],
        component: UserListComponent,
        data: {
            permission: EnumPermissions.ViewAccess,
            permissionFor: EnumPermissionFor.USER
        }
    },
    {
        path: 'users/:id',
        canActivate: [authGuard],
        component: UserDetailComponent,
        data: {
            permission: EnumPermissions.ViewAccess,
            permissionFor: EnumPermissionFor.USER
        }
    },
    {
        path: 'roles',
        canActivate: [authGuard],
        component: RoleListComponent,
        data: {
            permission: EnumPermissions.ViewAccess,
            permissionFor: EnumPermissionFor.ROLE
        }
    },
    {
        path: 'roles/:id',
        canActivate: [authGuard],
        component: RoleDetailComponent,
        data: {
            permission: EnumPermissions.ViewAccess,
            permissionFor: EnumPermissionFor.ROLE
        }
    },
    {
        path: 'mail-setting',
        canActivate: [authGuard],
        component: MailSettingComponent,
        data: {
            permission: EnumPermissions.ViewAccess,
            permissionFor: EnumPermissionFor.SETTING
        }
    },
    {
        path: 'logs',
        canActivate: [authGuard],
        component: LogsListComponent,
        data: {
            permission: EnumPermissions.ViewAccess,
            permissionFor: EnumPermissionFor.SETTING
        }
    },
    {
        path: 'backups',
        canActivate: [authGuard],
        component: BackupListComponent,
        data: {
            permission: EnumPermissions.ViewAccess,
            permissionFor: EnumPermissionFor.SETTING
        }
    },
    {
        path: 'audit-logs',
        canActivate: [authGuard],
        component: AuditLogListComponent,
        data: {
            permission: EnumPermissions.ViewAccess,
            permissionFor: EnumPermissionFor.SETTING
        }
    }
]