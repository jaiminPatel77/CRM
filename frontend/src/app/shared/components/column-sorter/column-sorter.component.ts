import { Component, EventEmitter, Input, Output } from '@angular/core';
import { SortingModel } from '../../models/table-conf';
import { ConstString } from '../../models/const-string';

@Component({
    selector: 'app-column-sorter',
    imports: [],
    templateUrl: './column-sorter.component.html',
    styleUrl: './column-sorter.component.scss'
})
export class ColumnSorterComponent {

  @Input("field") field?: string; //Key field for sort
  @Input("orderBy") orderBy?: SortingModel; //current sort object.
  @Output("onSort") sortOrderChange = new EventEmitter<SortingModel>(); // event when sorting is change.

  public get isAsc(): boolean {
    if (this.field && this.orderBy) {
      if (this.orderBy.column === this.field && this.orderBy.order === ConstString.asc) {
        return true;
      }
    }
    return false;
  }

  public get isDesc(): boolean {
    if (this.field && this.orderBy) {
      if (this.orderBy.column === this.field && this.orderBy.order === ConstString.desc) {
        return true;
      }
    }
    return false;
  }

  sort() {
    if (this.field && this.orderBy) {
      if (this.orderBy.column === this.field) {
        if (this.orderBy.order === ConstString.desc) {
          this.orderBy.order = ConstString.asc;
        } else {
          this.orderBy.order = ConstString.desc;
        }
      } else {
        this.orderBy.column = this.field;
        this.orderBy.order = ConstString.asc;
      }
      this.sortOrderChange.emit(this.orderBy);
    }
  }

}
