import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiOkResponse } from '@shared/models/api-response';
import { environment } from '../../../../environments/environment';

export interface BackupInfo {
    name: string;
    sizeBytes: number;
    lastModified: Date;
}

export interface BackupCleanupRequest {
    fileNames?: string[];
    olderThan?: Date;
}

@Injectable({
    providedIn: 'root'
})
export class BackupService {

    private _http = inject(HttpClient);
    private baseUrl: string;

    constructor() {
        this.baseUrl = `${environment.Setting.apiServiceUrl}/api/v1/backups`;
    }

    /**
     * Get list of all backups
     */
    getBackups(): Observable<BackupInfo[]> {
        return this._http.get<ApiOkResponse<BackupInfo[]>>(this.baseUrl).pipe(
            map(res => res.data || [])
        );
    }

    /**
     * Download a backup file
     * @param fileName Name of the backup file
     */
    downloadBackup(fileName: string): Observable<Blob> {
        const url = `${this.baseUrl}/${encodeURIComponent(fileName)}/download`;
        return this._http.get(url, { responseType: 'blob' });
    }

    /**
     * Delete a single backup file
     * @param fileName Name of the backup file to delete
     */
    deleteBackup(fileName: string): Observable<ApiOkResponse<string>> {
        const url = `${this.baseUrl}/${encodeURIComponent(fileName)}`;
        return this._http.delete<ApiOkResponse<string>>(url);
    }

    /**
     * Cleanup/delete multiple backup files
     * @param request Cleanup request with file names or date criteria
     */
    cleanBackups(request: BackupCleanupRequest): Observable<ApiOkResponse<string>> {
        return this._http.delete<ApiOkResponse<string>>(this.baseUrl, { body: request });
    }

    /**
     * Create a new database backup
     */
    createBackup(): Observable<ApiOkResponse<string>> {
        return this._http.post<ApiOkResponse<string>>(this.baseUrl, {});
    }

    /**
     * Restore database from a backup file
     * WARNING: This is a destructive operation!
     * @param fileName Name of the backup file to restore from
     */
    restoreBackup(fileName: string): Observable<ApiOkResponse<string>> {
        const url = `${this.baseUrl}/${encodeURIComponent(fileName)}/restore`;
        return this._http.post<ApiOkResponse<string>>(url, {});
    }
}
