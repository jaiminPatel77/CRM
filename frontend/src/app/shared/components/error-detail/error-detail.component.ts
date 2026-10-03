import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { ApiError } from '../../models/api-response';
import { CommonService } from '../../services/common.service';

import { NgbAlert } from '@ng-bootstrap/ng-bootstrap';
import { SvgIconDirective } from '../../directives/svg-icon.directive';

@Component({
    selector: 'crm-error-detail',
    imports: [NgbAlert, SvgIconDirective],
    templateUrl: './error-detail.component.html',
    styleUrl: './error-detail.component.scss'
})
export class ErrorDetailComponent implements OnInit, OnDestroy {

  @Input('errorDetail') errorMsgDetail?: ApiError;
  errorTitle?: string;

  constructor(private _commonService: CommonService) { }

  ngOnInit(): void {
    this.showErrorDetail();
  }

  ngOnDestroy(): void {

  }

  showErrorDetail() {
    if (this.errorMsgDetail) {
      if (this.errorMsgDetail.eventMessageId) {
        this.errorTitle = this._commonService.translateService.instant(this.errorMsgDetail.eventMessageId);
      } else if (this.errorMsgDetail.errorMessage) {
        this.errorTitle = this.errorMsgDetail.errorMessage;
      } else if (this.errorMsgDetail.statusCode === 404) {
        this.errorTitle = this._commonService.translateService.instant('SERVICE_NOT_AVAILABLE_ERROR');
      } else if (this.errorMsgDetail.statusCode === 401) {
        this.errorTitle = this._commonService.translateService.instant('USER_NOT_AUTHORISED_ERROR');
      } else {
        this.errorTitle = this._commonService.translateService.instant('API_SERVICE_RETURN_UNKNOWN_ERROR');
      }
    }
  }

}

