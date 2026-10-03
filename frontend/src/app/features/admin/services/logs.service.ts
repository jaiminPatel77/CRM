import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiOkResponse } from '@shared/models/api-response';
import { environment } from '../../../../environments/environment';

export interface LogFileInfo {
    name: string;
    sizeBytes: number;
    lastModified: Date;
}

export interface LogCleanupRequest {
    fileNames?: string[];
    olderThan?: Date;
}

@Injectable({
    providedIn: 'root'
})
export class LogsService {

    private _http = inject(HttpClient);
    private baseUrl: string;

    constructor() {
        this.baseUrl = `${environment.Setting.apiServiceUrl}/api/v1/logs`;
    }

    /**
     * Get log content for a specific date
     * @param date Optional date string (YYYY-MM-DD format)
     */
    getLogs(date?: string): Observable<ApiOkResponse<string>> {
        const url = date ? `${this.baseUrl}?date=${date}` : this.baseUrl;
        return this._http.get<ApiOkResponse<string>>(url);
    }

    /**
     * Get list of all log files
     */
    getLogFiles(): Observable<LogFileInfo[]> {
        const url = `${this.baseUrl}/files`;
        return this._http.get<ApiOkResponse<LogFileInfo[]>>(url).pipe(
            map(res => res.data || [])
        );
    }

    /**
     * Export selected log files as a zip
     * @param fileNames Array of log file names to export
     */
    exportLogs(fileNames: string[]): Observable<Blob> {
        const url = `${this.baseUrl}/export`;
        return this._http.post(url, { fileNames }, { responseType: 'blob' });
    }

    /**
     * Cleanup/delete log files
     * @param request Cleanup request with file names or date criteria
     */
    cleanupLogs(request: LogCleanupRequest): Observable<ApiOkResponse<string>> {
        return this._http.delete<ApiOkResponse<string>>(this.baseUrl, { body: request });
    }
}
