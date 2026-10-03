import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SMTPSetting, SettingDto } from '../models/setting-dto';
import { Observable, map } from 'rxjs';
import { ApiOkResponse } from '@shared/models/api-response';
import { environment } from '../../../../environments/environment';
import { BaseService } from '@shared/services/base.service';

@Injectable({
  providedIn: 'root'
})
export class SettingService extends BaseService<SettingDto<any>> {

  private _httpLocal = inject(HttpClient);

  constructor() {
    super(`${environment.Setting.apiServiceUrl}/api/v1/Settings`);
  }

  //#region SMTP setting API

  /**
   * fetch SMTP setting from server.
   */
  getSMTPSetting(): Observable<ApiOkResponse<SettingDto<SMTPSetting>>> {
    const url = `${this.baseUrl}/smpt-setting`;
    return this.http.get<ApiOkResponse<SettingDto<any>>>(url).pipe(
      map((response: ApiOkResponse<SettingDto<any>>) => {
        // Backend returns { value: "json string" }, convert to object
        if (response.data?.value && typeof response.data.value === 'string') {
          try {
            response.data.value = JSON.parse(response.data.value);
          } catch (e) {
            response.data.value = new SMTPSetting();
          }
        }
        return response as ApiOkResponse<SettingDto<SMTPSetting>>;
      })
    );
  }

  /**
   * update the SMTP setting
   * @param model model for SMTP setting
   */
  putSMTPSetting(model: SettingDto<SMTPSetting>): Observable<ApiOkResponse<SettingDto<SMTPSetting>>> {
    const url = `${this.baseUrl}/smpt-setting`;
    // Backend expects { Value: "json string" }, not the full model
    const payload = { value: JSON.stringify(model.value) };
    return this.http.put<ApiOkResponse<SettingDto<SMTPSetting>>>(url, payload);
  }

  /**
   * Get setting by key
   * @param key The setting key
   */
  getByKey<T>(key: string): Observable<ApiOkResponse<SettingDto<T>>> {
    const url = `${this.baseUrl}/key/${encodeURIComponent(key)}`;
    return this.http.get<ApiOkResponse<SettingDto<T>>>(url);
  }

  //#endregion SMTP setting API
}
