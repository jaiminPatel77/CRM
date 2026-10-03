import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Lead } from '../models/crm.models';
import { ApiOkResponse } from '@shared/models/api-response';
import { BaseService } from '@shared/services/base.service';

@Injectable({
  providedIn: 'root'
})
export class LeadService extends BaseService<Lead> {
  private _http = inject(HttpClient);

  constructor() {
    super(BaseService.ApiUrls.Lead);
  }

  getLeads(): Observable<ApiOkResponse<Lead[]>> {
    return this._http.get<ApiOkResponse<Lead[]>>(this.baseUrl);
  }

  getLeadById(id: number): Observable<ApiOkResponse<Lead>> {
    return this._http.get<ApiOkResponse<Lead>>(`${this.baseUrl}/${id}`);
  }

  createLead(lead: Lead): Observable<ApiOkResponse<Lead>> {
    return this._http.post<ApiOkResponse<Lead>>(this.baseUrl, lead);
  }

  updateLead(id: number, lead: Lead): Observable<ApiOkResponse<Lead>> {
    return this._http.put<ApiOkResponse<Lead>>(`${this.baseUrl}/${id}`, lead);
  }

  deleteLead(id: number): Observable<ApiOkResponse<boolean>> {
    return this._http.delete<ApiOkResponse<boolean>>(`${this.baseUrl}/${id}`);
  }
}
