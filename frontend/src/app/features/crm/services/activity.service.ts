import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Activity } from '../models/crm.models';
import { ApiOkResponse } from '@shared/models/api-response';
import { BaseService } from '@shared/services/base.service';

@Injectable({
  providedIn: 'root'
})
export class ActivityService extends BaseService<Activity> {
  private _http = inject(HttpClient);

  constructor() {
    super(BaseService.ApiUrls.Activity);
  }

  getActivities(): Observable<ApiOkResponse<Activity[]>> {
    return this._http.get<ApiOkResponse<Activity[]>>(this.baseUrl);
  }

  getActivityById(id: number): Observable<ApiOkResponse<Activity>> {
    return this._http.get<ApiOkResponse<Activity>>(`${this.baseUrl}/${id}`);
  }

  createActivity(activity: Activity): Observable<ApiOkResponse<Activity>> {
    return this._http.post<ApiOkResponse<Activity>>(this.baseUrl, activity);
  }

  updateActivity(id: number, activity: Activity): Observable<ApiOkResponse<Activity>> {
    return this._http.put<ApiOkResponse<Activity>>(`${this.baseUrl}/${id}`, activity);
  }

  deleteActivity(id: number): Observable<ApiOkResponse<boolean>> {
    return this._http.delete<ApiOkResponse<boolean>>(`${this.baseUrl}/${id}`);
  }
}
