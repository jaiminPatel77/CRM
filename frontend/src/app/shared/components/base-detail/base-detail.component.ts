import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { BaseDto } from '../../models/base-model';
import { BaseComponent } from '../base/base.component';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Observable, Subject, takeUntil } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { DataStatusEnum } from '../../models/common-models';
import { ApiError, ApiOkResponse } from '../../models/api-response';
import { deepCompare } from '../../models/helper';
import { plainToClassFromExist } from 'class-transformer';
import { ToastService } from '../../../core/services/toast.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-base-detail',
  imports: [],
  templateUrl: './base-detail.component.html',
  styleUrl: './base-detail.component.scss'
})
export class BaseDetailComponent<T extends BaseDto> extends BaseComponent implements OnInit, OnDestroy {
  submitted: boolean = false;
  model?: T;
  detailForm?: FormGroup;
  protected destroy$ = new Subject<boolean>();
  recordId: number;
  isNew: boolean = true;

  // inject services
  protected _router = inject(Router);
  protected _activatedRoute = inject(ActivatedRoute);
  protected _formBuilder = inject(FormBuilder);
  protected _toastService = inject(ToastService);
  protected _translateService = inject(TranslateService);
  protected _cdr = inject(ChangeDetectorRef);

  constructor() {
    super();
    this.recordId = 0;
  }

  //#region Init and destroy methods

  override ngOnInit() {
    super.ngOnInit();
    this.dataStatus = DataStatusEnum.Fetching;
    this.createForm();
    let id = this._activatedRoute.snapshot.paramMap.get("id");
    if (id) {
      this.recordId = Number(id);
      if (this.recordId) {
        this.isNew = false;
      }
    }
    this.setValueChangeEvent();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    if (this.destroy$) {
      this.destroy$.next(true);
      this.destroy$.complete();
    }
  }

  //#endregion Init and destroy methods


  //#region Must override methods

  createNewModelObject(): T {
    throw new Error('createNewModelObject must be override');
  }

  /**
   * prefer model object from value input in form controls 
   */
  transferDetailFormValuesToModel(model: T) {
    //Must override it!      
  }

  /**
   * Create detail form group!
   * Must override in detail component.
   */
  createForm() {
    //Must override it!
  }

  /**
   * One time value change event subscribe to form control events!
   */
  setValueChangeEvent() {
    //Must override it!
  }

  //#endregion Must override methods end

  /**
   * Method to save the record. Must be implemented by deriving classes if they use onSubmit default logic.
   * @param model 
   */
  saveRecord(model: T): Observable<ApiOkResponse<any>> {
    throw new Error('saveRecord must be implemented by derived class');
  }

  //#region can override methods

  /**
   * perform save data to server if all inputs are valid!
   */
  onSubmit(): void {
    if (this.model && this.validateDetailFormBeforeSubmit()) {
      this.submitted = true;
      this.transferDetailFormValuesToModel(this.model);

      this.saveRecord(this.model)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result: ApiOkResponse<any>) => {
            //console.log(JSON.stringify(result));
            this.showSuccessMsgToaster(result);
            this.onCancel();
          },
          error: (error: ApiError) => {
            this.submitted = false;
            //console.dir(error);
            this.showApiErrorToast(error);
          }
        });
    }
  }

  showSuccessMsgToaster(result: ApiOkResponse<any>) {
    let message = result.eventMessageId || 'SAVE_SUCCESSFULLY';
    let title = this._translateService.instant(message);
    this._toastService.successToast(title);
  }

  showApiErrorToast(apiError: ApiError) {
    let errorTitle: string;
    let errorDetail = apiError.errorDetail;
    if (apiError.eventMessageId) {
      errorTitle = this._translateService.instant(apiError.eventMessageId);
    } else if (apiError.errorMessage) {
      errorTitle = apiError.errorMessage;
    } else if (apiError.statusCode === 404) {
      errorTitle = this._translateService.instant('SERVICE_NOT_AVAILABLE_ERROR');
    } else if (apiError.statusCode === 401) {
      errorTitle = this._translateService.instant('USER_NOT_AUTHORIZED_ERROR');
    } else {
      errorTitle = this._translateService.instant('API_SERVICE_RETURN_UNKNOWN_ERROR');
    }

    this._toastService.errorToast(errorTitle, errorDetail);
  }

  /**
   * Go to list page. override if path is different
   */
  onCancel() {
    this._router.navigate(['..'], { relativeTo: this._activatedRoute });
  }

  /**
   * validate form controls before submit data to server!
   * by default only validate the detailForm .. in case if any custom validation needed override it.
   */
  validateDetailFormBeforeSubmit(): boolean {
    if (this.detailForm) {
      this.validateAllFormFields(this.detailForm);
      return this.detailForm.valid;
    }
    return false;
  }

  /**
   * Override it to return true if data is changed otherwise return false.
   * If model is not changed then it will not ask for navigation confirmation.
   */
  override get isModelChanged(): boolean {
    if (!this.submitted) {
      let defaultModel = this.createNewModelObject();
      let currentModel = plainToClassFromExist(defaultModel, this.model);
      this.transferDetailFormValuesToModel(currentModel);
      return !deepCompare(currentModel, this.model);
    } else {
      return false;
    }
  }

  //#endregion can override methods end  
}