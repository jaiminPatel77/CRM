import { Component, OnDestroy, OnInit } from '@angular/core';
import { ConfirmDlgSetting } from '../../models/base-model';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'crm-confirm-dialog-popup',
    imports: [TranslateModule],
    templateUrl: './confirm-dialog-popup.component.html',
    styleUrl: './confirm-dialog-popup.component.scss'
})
export class ConfirmDialogPopupComponent implements OnInit, OnDestroy {

  private _isConfirmed = false;
  model: ConfirmDlgSetting;

  constructor(public activeModal: NgbActiveModal) {
    this.model = new ConfirmDlgSetting();
  }

  ngOnInit(): void {

  }

  ngOnDestroy(): void {

  }

  onCancel() {
    this._isConfirmed = false;
    this.activeModal.dismiss(this._isConfirmed);
  }

  onOk() {
    this._isConfirmed = true;
    this.activeModal.close(this._isConfirmed);
  }

}