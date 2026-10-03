import { Injectable, WritableSignal, inject, signal } from '@angular/core';
import { MenuItem } from '../models/menu';
import { MenuHelper } from '../models/menu-helper';
import { Location } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class NavigationService {

  isPopupMenu: WritableSignal<boolean> = signal<boolean>(false);
  isMobileView: WritableSignal<boolean> = signal<boolean>(false);
  hideSideNav: WritableSignal<boolean> = signal<boolean>(false);
  

  locationPath = ''; // location path without query param!

  private _location = inject(Location);

  constructor() {
    // set view as per window's width
    if (window.innerWidth < 768) {
      // mobile view
      this.isMobileView.set(true);
      this.hideSideNav.set(true);
      this.hideSideNav.set(true);
    } else {
      // web view
      this.isMobileView.set(false);
      this.hideSideNav.set(false);
      this.hideSideNav.set(false);
    }

  }

  toggleSideNav(): void {
    this.hideSideNav.update((value) => !value);
  }

  getSideBarMenu(): MenuItem[] {
    return MenuHelper.menus;
  }



  selectFromUrl(items: MenuItem[]) {
    this.locationPath = MenuHelper.getPathPartOfUrl(this._location.path());

    items.forEach(menu => {
      menu.isLinkActive.set(this.isLinkActive(menu));
    });

    const selectedItem = this.findItemByUrl(items);
    if (selectedItem) {
      selectedItem.expanded = true;
      if (selectedItem.parent) {
        selectedItem.parent.expanded = true;
      }
    }
  }



  /**
     * Find deepest item which link matches current URL path.
     * @param items array of items to search in.
     * @returns found item of undefined.
     */
  private findItemByUrl(items: MenuItem[]): MenuItem | undefined {
    let selectedItem: MenuItem | undefined;

    items.some(item => {
      if (item.children) {
        selectedItem = this.findItemByUrl(item.children);
      }
      if (!selectedItem && this.isSelectedInUrl(item)) {
        selectedItem = item;
      }
      return selectedItem;
    });

    return selectedItem;
  }


  private isSelectedInUrl(item: MenuItem): boolean {
    const exact: boolean = item.fullPathMatch || false;
    const link: string = item.routerLink ?? '';

    if (link) {
      const isSelectedInPath = exact
        ? MenuHelper.isUrlPathEqual(this.locationPath, link)
        : MenuHelper.isUrlPathContain(this.locationPath, link);
      return isSelectedInPath;
    } else {
      return false;
    }
  }


  isLinkActive(menuItem: MenuItem): boolean {
    if (menuItem.fullPathMatch) {
      return this.locationPath === menuItem.routerLink;
    } else {
      if (menuItem.routerLink) {
        return MenuHelper.isUrlPathContain(this.locationPath, menuItem.routerLink);
      }
      else {
        return false;
      }
    }
  }
  
}

