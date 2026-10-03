import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserService } from '../../../auth/services/user.service';
import { UserProfileDto } from '../../../admin/models/user-profile-dto';
import { ApiError, ApiOkResponse } from '@shared/models/api-response';
import { Subject, takeUntil } from 'rxjs';
import { NgbAlert } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-dashboard',
  imports: [TranslateModule, NgbAlert],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {

  private destroy$ = new Subject<boolean>();

  constructor(
    private _userService: UserService
  ) { }

  ngOnInit() {
    this.loadCurrentUser();
  }

  ngOnDestroy() {
    if (this.destroy$) {
      this.destroy$.next(true);
      this.destroy$.complete();
    }
  }

  loadCurrentUser() {
    console.log("currentUser:", this._userService.currentUser)
    this.getProfileData();
  }

  getProfileData() {
    if (this._userService.currentUser.id) {
      this._userService.getUserProfile(this._userService.currentUser.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response: ApiOkResponse<UserProfileDto>) => {
            console.log("response:", response?.data);
          },
          error: (error: ApiError) => {
            console.log("error:", error);
          }
        });
    }
  }

}
