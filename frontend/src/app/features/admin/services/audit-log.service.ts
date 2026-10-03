import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiOkResponse } from '@shared/models/api-response';
import { environment } from '../../../../environments/environment';

export interface AuditLogDto {
    id: number;
    userId?: string;
    type: string;
    tableName: string;
    dateTime: Date;
    oldValues?: string;
    newValues?: string;
    affectedColumns?: string;
    primaryKey: string;
}

export interface AuditLogQueryParams {
    page?: number;
    pageSize?: number;
    filter?: string;
    orderBy?: string;
}

export interface PagingResult<T> {
    count: number;
    items: T[];
}

// Gridify's actual response format
interface GridifyPaging<T> {
    count: number;
    data: T[];
}

@Injectable({
    providedIn: 'root'
})
export class AuditLogService {

    private _http = inject(HttpClient);
    private baseUrl: string;

    constructor() {
        this.baseUrl = `${environment.Setting.apiServiceUrl}/api/v1/AuditLogs`;
    }

    /**
     * Get paginated audit logs
     * @param query Query parameters for pagination and filtering
     */
    getAuditLogs(query?: AuditLogQueryParams): Observable<PagingResult<AuditLogDto>> {
        let url = this.baseUrl;
        if (query) {
            const params = new URLSearchParams();
            if (query.page) params.append('page', query.page.toString());
            if (query.pageSize) params.append('pageSize', query.pageSize.toString());
            if (query.filter) params.append('filter', query.filter);
            if (query.orderBy) params.append('orderBy', query.orderBy);
            const queryString = params.toString();
            if (queryString) url += `?${queryString}`;
        }
        return this._http.get<ApiOkResponse<GridifyPaging<AuditLogDto>>>(url).pipe(
            map(res => ({
                count: res.data?.count || 0,
                items: res.data?.data || []
            }))
        );
    }
}
