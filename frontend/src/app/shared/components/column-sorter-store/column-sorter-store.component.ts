import { Component, EventEmitter, Input, Output } from '@angular/core';
import { EnumTypeOfProperty, SortingModelForStore } from '../../models/table-conf';
import { ConstString } from '../../models/const-string';

@Component({
    selector: 'app-column-sorter-store',
    imports: [],
    templateUrl: './column-sorter-store.component.html',
    styleUrl: './column-sorter-store.component.scss'
})
export class ColumnSorterStoreComponent {

  @Input("field") field?: string; //Key field for sort
  @Input("typeOfproperty") typeOfproperty?: EnumTypeOfProperty; //Key typeOfproperty for sort
  @Input("orderBy") orderBy?: SortingModelForStore; //current sort object.
  @Output("onSort") sortOrderChange = new EventEmitter<SortingModelForStore>(); // event when sorting is change.

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
    if (this.field && this.typeOfproperty && this.orderBy) {
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
      this.orderBy.typeOfProperty = this.typeOfproperty;
      this.sortOrderChange.emit(this.orderBy);
    }
  }

}

