import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { BaseListComponent } from '@shared/components/base-list/base-list.component';
import { RoleListDto } from '../../models/role-dto';
import { RoleService } from '../../../auth/services/role.service';
import { EnumPermissionFor } from '@shared/models/common-enums';
import { LoadingComponent } from '@shared/components/loading/loading.component';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule, NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { FilterModel, SourceInfo } from '@shared/models/table-conf';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';
import { ColumnSorterComponent } from '@shared/components/column-sorter/column-sorter.component';
import { ConstString } from '@shared/models/const-string';

@Component({
  selector: 'app-role-list',
  imports: [TranslateModule, LoadingComponent, FormsModule, NgbPaginationModule, NgbTooltip, SvgIconDirective, ColumnSorterComponent],
  templateUrl: './role-list.component.html',
  styleUrl: './role-list.component.scss'
})
export class RoleListComponent extends BaseListComponent<RoleListDto> implements OnInit, OnDestroy {

  // Search fields - searches both name and description
  nameField: string = 'name';
  descriptionField: string = 'description';
  searchTxt = signal<string>('');

  private _roleService = inject(RoleService);

  constructor() {
    super(inject(RoleService));
    this.allowMultipleSelect = true;
    this.requiredPermissionType = EnumPermissionFor.ROLE;
  }

  //#region search filter starts

  override setFilterValuesToModel(sourceInfo: SourceInfo) {
    // Restore search text from either name or description filter
    let nameFilter = sourceInfo.filters.find(e => e.filterName === this.nameField);
    let descFilter = sourceInfo.filters.find(e => e.filterName === this.descriptionField);
    if (nameFilter) {
      this.searchTxt.set(nameFilter.filterVal);
    } else if (descFilter) {
      this.searchTxt.set(descFilter.filterVal);
    }
  }

  onSearch() {
    const currentSearch = this.searchTxt().trim();
    if (currentSearch) {
      // Create filters for both name and description with OR operator
      const filters: FilterModel[] = [
        { filterName: this.nameField, filterVal: currentSearch, operator: ConstString.ContainsSplit, splitOp: ConstString.OrSplit },
        { filterName: this.descriptionField, filterVal: currentSearch, operator: ConstString.ContainsSplit, splitOp: '' }
      ];
      super.onFilterMultipleFields(filters, ConstString.AndSplit);
    } else {
      // Clear both filters
      this.sourceInfo.filters = this.sourceInfo.filters.filter(
        f => f.filterName !== this.nameField && f.filterName !== this.descriptionField
      );
      super.onClearFilter();
    }
  }

  override onClearFilter() {
    this.searchTxt.set('');
    super.onClearFilter();
  }

  //#endregion search filter ends

}
