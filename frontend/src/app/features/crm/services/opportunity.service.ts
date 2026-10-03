import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Opportunity } from '../models/crm.models';
import { ApiOkResponse } from '@shared/models/api-response';
import { BaseService } from '@shared/services/base.service';

@Injectable({
  providedIn: 'root'
})
export class OpportunityService extends BaseService<Opportunity> {
  private _http = inject(HttpClient);

  constructor() {
    super(BaseService.ApiUrls.Opportunity);
  }

  getOpportunities(): Observable<ApiOkResponse<Opportunity[]>> {
    return this._http.get<ApiOkResponse<Opportunity[]>>(this.baseUrl);
  }

  getOpportunityById(id: number): Observable<ApiOkResponse<Opportunity>> {
    return this._http.get<ApiOkResponse<Opportunity>>(`${this.baseUrl}/${id}`);
  }

  createOpportunity(opportunity: Opportunity): Observable<ApiOkResponse<Opportunity>> {
    return this._http.post<ApiOkResponse<Opportunity>>(this.baseUrl, opportunity);
  }

  updateOpportunity(id: number, opportunity: Opportunity): Observable<ApiOkResponse<Opportunity>> {
    return this._http.put<ApiOkResponse<Opportunity>>(`${this.baseUrl}/${id}`, opportunity);
  }

  deleteOpportunity(id: number): Observable<ApiOkResponse<boolean>> {
    return this._http.delete<ApiOkResponse<boolean>>(`${this.baseUrl}/${id}`);
  }
}
