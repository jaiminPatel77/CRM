import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MenuItem } from '@shared/models/menu';
import { MenuHelper } from '@shared/models/menu-helper';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { NgbCollapseModule, NgbNavModule } from '@ng-bootstrap/ng-bootstrap';
import { NavigationService } from '@shared/services/navigation.service';
import { AuthService } from '@auth/services/auth.service';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';


@Component({
  selector: 'crm-side-bar',
  imports: [RouterLink, RouterLinkActive, CommonModule, TranslateModule, NgbNavModule],
  templateUrl: './side-bar.component.html',
  styleUrl: './side-bar.component.scss'
})
export class SideBarComponent implements OnInit {
  menuList: MenuItem[] = [];

  // inject services
  private _router = inject(Router);
  private _navigationService = inject(NavigationService);

  ngOnInit(): void {
    this.menuList = MenuHelper.menus;
  }

  onMenuClick(menu: MenuItem) {
    if (menu?.children && menu?.children.length > 0) {
      menu.expanded = !menu.expanded;
    }
  }


}
