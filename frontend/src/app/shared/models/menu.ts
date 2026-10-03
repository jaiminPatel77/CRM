import { WritableSignal, inject, signal } from "@angular/core";
import { EnumPermissionFor, EnumPermissions } from "./common-enums";

/**
 * Menu Item options example
 * @stacked-example(Menu Link Parameters, menu/menu-link-params.component)
 */
export class MenuItem {

    

    /**
     * Name/id of Menu item.
     * @type {string}
     */
    name: string;
    /**
     * Item Title
     * @type {string}
     */
    title: string;
    /**
     * Item relative routerLink
     * @type {string}
     */
    routerLink?: string;

    /**
     * svg Icon name
     * @type {string}
     */
    svgIconName?: string;

    /**
     * Expanded by default
     * @type {boolean}
     */
    expanded?: boolean;

    /**
     * Children items
     * @type {List<MenuItem>}
     */
    children?: MenuItem[];

    /**
     * Hidden Item
     * @type {boolean}
     */
    hidden?: boolean;

    /**
     * Item is selected when partly or fully equal to the current url
     * @type {boolean}
     */
    fullPathMatch?: boolean;

    /**
     * Whether the item is just a group (non-clickable)
     * Not implement yet.
     * @type {boolean}
     */
    group?: boolean;

    /** Whether the item selected is true or false
     *@type {boolean}
     */
    selected?: boolean;

    /** Whether the current user is having permission to view menu
     *@type {EnumPermissions}
     */
    permission?: EnumPermissions;

    /** Whether the current user is having permission to view menu -permission name
     *@type {EnumPermissionFor}
     */
    permissionFor?: EnumPermissionFor;

    /** Parent MenuItem
     *@type {MenuItem}
     */
    parent?: MenuItem;

    /** isLinkActive if it currently active menu and if its any child is active
     *@type {WritableSignal<boolean>}
     */
    isLinkActive : WritableSignal<boolean> = signal<boolean>(false);

  

    constructor(
        name: string,
        title: string,
        routerLink: string | undefined,
        svgIconName: string,
        expanded: boolean,
        children: MenuItem[],
        fullPathMatch: boolean = true,
    ) {
        this.name = name;
        this.title = title;
        this.routerLink = routerLink;
        this.children = children;
        if (children?.length) {
            children.forEach(element => {
                element.parent = this;
            });
        }
        this.svgIconName = svgIconName;
        this.expanded = expanded;
        this.fullPathMatch = fullPathMatch;
    }

   

    public get hasChild(): boolean {
        if (this.children && this.children.length > 0) {
            return true;
        }
        return false;
    }
    
}