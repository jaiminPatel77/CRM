import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { AuthService } from '@auth/services/auth.service';
import { Router, RouterLink } from '@angular/router';
import { NgbModal, NgbDropdownModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { UserService } from '@auth/services/user.service';
import { NavigationService } from '@shared/services/navigation.service';
import { ChangePasswordComponent } from '../change-password/change-password.component';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';
import { TranslateModule } from '@ngx-translate/core';
import { ThemeSwitcherComponent } from '@shared/components/theme-switcher/theme-switcher.component';

@Component({
  selector: 'crm-header',
  imports: [SvgIconDirective, TranslateModule, NgbTooltipModule, NgbDropdownModule, RouterLink, ThemeSwitcherComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})

export class HeaderComponent implements OnInit, OnDestroy {

  isUserInfoPopUpShown: boolean = false;
  isMobileMenuPopUp: boolean = false;

  private _authService = inject(AuthService);
  public _userService = inject(UserService);

  constructor(
    public _navService: NavigationService,
    private _router: Router,
    private _modalService: NgbModal
  ) { }

  ngOnInit() {

  }

  ngOnDestroy() {

  }

  clickEvent() {
    this._navService.toggleSideNav();
  }

  goToProfile() {
    this._router.navigate(['/profile']);
    this.isUserInfoPopUpShown = false;
  }

  goToChangePassword() {
    this.isUserInfoPopUpShown = false;
    const modalRef = this._modalService.open(ChangePasswordComponent);
    let changePasswordPopup: ChangePasswordComponent = modalRef.componentInstance;
    modalRef.result.then((result) => {
      return true;
    }, (reason) => {
      return false;
    });
  }

  logout() {
    this._authService.logout();
  }

  toggleMobileMenuInfoPopUp() {
    this.isMobileMenuPopUp = !this.isMobileMenuPopUp;
  }

  goToUserProfile() {
    this._router.navigate(['/profile']);
  }

  toggleSidebar() {
    this._navService.toggleSideNav();
  }

}