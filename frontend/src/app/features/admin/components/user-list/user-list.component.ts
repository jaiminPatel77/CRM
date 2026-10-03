import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { BaseListComponent } from '@shared/components/base-list/base-list.component';
import { UserListDto } from '../../models/user-dto';
import { UserService } from '../../../auth/services/user.service';
import { EnumPermissionFor, EnumUserStatus } from '@shared/models/common-enums';
import { TranslateModule } from '@ngx-translate/core';
import { LoadingComponent } from '@shared/components/loading/loading.component';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule, NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { takeUntil } from 'rxjs';
import { ApiError, ApiOkResponse } from '@shared/models/api-response';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';
import { FilterModel, SourceInfo } from '@shared/models/table-conf';
import { ColumnSorterComponent } from '@shared/components/column-sorter/column-sorter.component';
import { ConstString } from '@shared/models/const-string';

@Component({
  selector: 'app-user-list',
  imports: [TranslateModule, LoadingComponent, FormsModule, NgbPaginationModule, NgbTooltip, SvgIconDirective, ColumnSorterComponent],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent extends BaseListComponent<UserListDto> implements OnInit, OnDestroy {

  enumUserStatus = EnumUserStatus;

  // Search fields - searches both email and fullName
  emailField: string = 'email';
  fullNameField: string = 'fullName';
  searchTxt = signal<string>('');

  private _userService = inject(UserService);

  constructor() {
    super(inject(UserService));
    this.allowMultipleSelect = true;
    this.requiredPermissionType = EnumPermissionFor.USER;
  }

  //#region search filter starts

  override setFilterValuesToModel(sourceInfo: SourceInfo) {
    // Restore search text from either email or fullName filter
    let emailFilter = sourceInfo.filters.find(e => e.filterName === this.emailField);
    let nameFilter = sourceInfo.filters.find(e => e.filterName === this.fullNameField);
    if (emailFilter) {
      this.searchTxt.set(emailFilter.filterVal);
    } else if (nameFilter) {
      this.searchTxt.set(nameFilter.filterVal);
    }
  }

  onSearch() {
    const currentSearch = this.searchTxt().trim();
    if (currentSearch) {
      // Create filters for both email and fullName with OR operator
      const filters: FilterModel[] = [
        { filterName: this.emailField, filterVal: currentSearch, operator: ConstString.ContainsSplit, splitOp: ConstString.OrSplit },
        { filterName: this.fullNameField, filterVal: currentSearch, operator: ConstString.ContainsSplit, splitOp: '' }
      ];
      super.onFilterMultipleFields(filters, ConstString.AndSplit);
    } else {
      // Clear both filters
      this.sourceInfo.filters = this.sourceInfo.filters.filter(
        f => f.filterName !== this.emailField && f.filterName !== this.fullNameField
      );
      super.onClearFilter();
    }
  }

  override onClearFilter() {
    this.searchTxt.set('');
    super.onClearFilter();
  }

  //#endregion search filter ends

  //#region invite user starts

  onInviteUser(record: UserListDto) {
    if (record?.id) {
      record.$inviteUserLoading = true;
      this._userService.adminInviteUser(record.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result: ApiOkResponse<any>) => {
            (<UserListDto[]>this.sourceInfo.list).forEach(element => {
              if (element.id === record.id) {
                element.status = EnumUserStatus.Invited;
              }
            });
            record.$inviteUserLoading = false;
            if (result.eventMessageId) {
              this._userService.showSuccessIdToast(result.eventMessageId);
            }
          },
          error: (error: ApiError) => {
            this._userService.showApiErrorToast(error);
            record.$inviteUserLoading = false;
          }
        }
        );
    }
  }

  //#endregion invite user ends

}
