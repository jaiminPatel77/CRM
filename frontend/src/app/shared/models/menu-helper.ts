import { MenuItem } from "./menu";

export class MenuHelper {

    /**
     * Define default menu relation for application.
     */
    public static readonly menus: MenuItem[] = [
        new MenuItem('dashboard', 'SIDE_BAR_DASHBOARD', '/dashboard', 'crm_icon', false, []),
        new MenuItem('admin', 'SIDE_BAR_ADMIN', '/admin', 'crm_icon', false, [
            new MenuItem('roles', 'SIDE_BAR_ROLES', '/admin/roles', 'crm_icon', false, []),
            new MenuItem('users', 'SIDE_BAR_USERS', '/admin/users', 'crm_icon', false, []),
            new MenuItem('mail-setting', 'SIDE_BAR_MAIL_SETTING', '/admin/mail-setting', 'crm_icon', false, []),
            new MenuItem('logs', 'SIDE_BAR_LOGS', '/admin/logs', 'crm_icon', false, []),
            new MenuItem('backups', 'SIDE_BAR_BACKUPS', '/admin/backups', 'crm_icon', false, []),
            new MenuItem('audit-logs', 'SIDE_BAR_AUDIT_LOGS', '/admin/audit-logs', 'crm_icon', false, [])
        ], false)
    ];

    /**
     * Url Path helper
     */
    public static isUrlPathEqual(path: string, link: string) {
        const locationPath = MenuHelper.getPathPartOfUrl(path);
        return link === locationPath;
    }

    public static isUrlPathContain(path: string, link: string) {
        const locationPath = MenuHelper.getPathPartOfUrl(path);
        const endOfUrlSegmentRegExp = /\/|^$/;
        return locationPath.startsWith(link) &&
            locationPath.slice(link.length).charAt(0).search(endOfUrlSegmentRegExp) !== -1;
    }

    public static getPathPartOfUrl(url: string): string {
        const matchedUrl = /.*?(?=[?;#]|$)/.exec(url);
        return matchedUrl?.[0] ?? '';
    }

    public static getFragmentPartOfUrl(url: string): string {
        const matched = /#(.+)/.exec(url);
        return matched ? matched[1] : '';
    }

}