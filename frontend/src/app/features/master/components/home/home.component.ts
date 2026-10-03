import { Component, OnDestroy, WritableSignal, inject } from '@angular/core';
import { NavigationCancel, NavigationEnd, NavigationStart, Router, RouterOutlet } from '@angular/router';
import { Subject, filter, takeUntil } from 'rxjs';
import { NavigationService } from '@shared/services/navigation.service';
import { AuthService } from '@auth/services/auth.service';
import { FooterComponent } from '../footer/footer.component';
import { HeaderComponent } from '../header/header.component';
import { SideBarComponent } from '../side-bar/side-bar.component';
import { CommonModule } from '@angular/common';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmDialogPopupComponent } from '@shared/components/confirm-dialog-popup/confirm-dialog-popup.component';
import { CommonService } from '@shared/services/common.service';
@Component({
  selector: 'app-home',
  imports: [RouterOutlet, FooterComponent, HeaderComponent, SideBarComponent, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnDestroy {

  loadingRouteConfig?: boolean;
  destroy$: Subject<boolean>;

  hideSidebarMenu: WritableSignal<boolean>;

  readonly DEFAULT_TIME_OUT = 60; //1 min as default.
  remainingCountDown: number = this.DEFAULT_TIME_OUT; // 1 min
  countdownId?: NodeJS.Timeout;
  confirmDialogPopupModalRef?: NgbModalRef;

  // inject services
  public _modalService = inject(NgbModal);
  public _commonService = inject(CommonService);
  private _authService = inject(AuthService);

  constructor(
    private _router: Router,
    public _navService: NavigationService) {
    this.destroy$ = new Subject<boolean>();

    this._router.events
      .pipe(
        takeUntil(this.destroy$),
        filter(event => event instanceof NavigationStart
        ),
      )
      .subscribe(() => {
        this.loadingRouteConfig = true;
      });

    this._router.events
      .pipe(
        takeUntil(this.destroy$),
        filter(event => event instanceof NavigationEnd
          || event instanceof NavigationCancel
        ),
      )
      .subscribe(() => {
        this.loadingRouteConfig = false;
        this._navService.selectFromUrl(this._navService.getSideBarMenu());
      });

    this.hideSidebarMenu = this._navService.hideSideNav;

    this._authService.sessionExpired$.pipe(takeUntil(this.destroy$)).subscribe(
      (result: boolean) => {
        if (result) {
          this.onSessionExpired();
        }
      }
    );
  }

  ngOnDestroy(): void {
    clearInterval(this.countdownId);
    if (this.destroy$) {
      this.destroy$.next(true);
      this.destroy$.complete();
    }
  }

  onSessionExpired() {
    this.confirmDialogPopupModalRef = this._modalService.open(ConfirmDialogPopupComponent);
    if (this.confirmDialogPopupModalRef) {
      let confirmDlg = this.confirmDialogPopupModalRef.componentInstance;
      if (confirmDlg) {
        confirmDlg.model.titleTextId = 'SESSION_WARNING_TITLE';
        confirmDlg.model.okBtnTextId = 'SESSION_WARNING_OK_TEXT';
        confirmDlg.model.cancelBtnTextId = 'SESSION_WARNING_CANCEL_TEXT';
        this.countDownBegins();
        this.confirmDialogPopupModalRef.result.then((result) => {
          if (result) {
            clearInterval(this.countdownId);
            this.remainingCountDown = this.DEFAULT_TIME_OUT;
            this._authService.setActualIdleTimer();
          }
          else {
            clearInterval(this.countdownId);
            this.logout();
          }
        }, (reason) => {
          clearInterval(this.countdownId);
          this.logout();
        });
      }
    }
  }

  countDownBegins() {
    this.setDialogueMsg();
    this.countdownId = setInterval(() => {
      this.remainingCountDown--;
      this.setDialogueMsg();
      if (this.remainingCountDown <= 0) {
        this.confirmDialogPopupModalRef?.dismiss();
      }
    }, 1000); //interval of 1 second
  }

  setDialogueMsg() {
    if (this.confirmDialogPopupModalRef) {
      this.confirmDialogPopupModalRef.componentInstance.model.messageTextId = `${this._commonService.translateService.instant('SESSION_EXPIRE')} ${this._commonService.translateService.instant('LOGOUT_IN')} ${this.remainingCountDown} ${this._commonService.translateService.instant('SECONDS')} ${this._commonService.translateService.instant('STAY_SIGNED_IN')}`;
    }
  }

  logout() {
    this._authService.logout();
  }

}

