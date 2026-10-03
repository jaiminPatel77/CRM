import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgbTooltipModule, NgbPaginationModule, NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap';
import { LogsService, LogFileInfo, LogCleanupRequest } from '../../services/logs.service';
import { LoadingComponent } from '@shared/components/loading/loading.component';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';
import { ToastService } from '@shared/services/toast.service';

@Component({
    selector: 'app-logs-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TranslateModule,
        NgbTooltipModule,
        NgbPaginationModule,
        NgbDatepickerModule,
        LoadingComponent,
        SvgIconDirective
    ],
    templateUrl: './logs-list.component.html',
    styleUrl: './logs-list.component.scss'
})
export class LogsListComponent implements OnInit {

    private _logsService = inject(LogsService);
    private _toastService = inject(ToastService);

    logFiles: LogFileInfo[] = [];
    selectedFiles: Set<string> = new Set();
    isLoading = signal<boolean>(true);
    isExporting = signal<boolean>(false);
    logContent = '';
    selectedDate: string = '';
    viewingLog = false;

    ngOnInit(): void {
        this.loadLogFiles();
    }

    loadLogFiles(): void {
        this.isLoading.set(true);
        this._logsService.getLogFiles().subscribe({
            next: (files) => {
                this.logFiles = files;
                this.isLoading.set(false);
            },
            error: (err) => {
                this._toastService.errorToast('Failed to load log files');
                this.isLoading.set(false);
            }
        });
    }

    viewLog(file: LogFileInfo): void {
        // Extract date from filename (log-YYYYMMDD.txt)
        const dateMatch = file.name.match(/log-(\d{8})\.txt/);
        if (dateMatch) {
            const dateStr = dateMatch[1];
            const formattedDate = `${dateStr.substring(0, 4)}-${dateStr.substring(4, 6)}-${dateStr.substring(6, 8)}`;
            this.isLoading.set(true);
            this._logsService.getLogs(formattedDate).subscribe({
                next: (res) => {
                    this.logContent = res.data || 'No content';
                    this.viewingLog = true;
                    this.isLoading.set(false);
                },
                error: () => {
                    this._toastService.errorToast('Failed to load log content');
                    this.isLoading.set(false);
                }
            });
        }
    }

    closeLogViewer(): void {
        this.viewingLog = false;
        this.logContent = '';
    }

    toggleSelection(file: LogFileInfo): void {
        if (this.selectedFiles.has(file.name)) {
            this.selectedFiles.delete(file.name);
        } else {
            this.selectedFiles.add(file.name);
        }
    }

    isSelected(file: LogFileInfo): boolean {
        return this.selectedFiles.has(file.name);
    }

    selectAll(): void {
        if (this.selectedFiles.size === this.logFiles.length) {
            this.selectedFiles.clear();
        } else {
            this.logFiles.forEach(f => this.selectedFiles.add(f.name));
        }
    }

    exportSelected(): void {
        if (this.selectedFiles.size === 0) {
            this._toastService.warningToast('Please select files to export');
            return;
        }

        this.isExporting.set(true);
        this._logsService.exportLogs(Array.from(this.selectedFiles)).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `logs-export-${new Date().toISOString().slice(0, 10)}.zip`;
                a.click();
                window.URL.revokeObjectURL(url);
                this.isExporting.set(false);
                this._toastService.successToast('Logs exported successfully');
            },
            error: () => {
                this._toastService.errorToast('Failed to export logs');
                this.isExporting.set(false);
            }
        });
    }

    deleteSelected(): void {
        if (this.selectedFiles.size === 0) {
            this._toastService.warningToast('Please select files to delete');
            return;
        }

        if (!confirm(`Are you sure you want to delete ${this.selectedFiles.size} log file(s)?`)) {
            return;
        }

        const request: LogCleanupRequest = {
            fileNames: Array.from(this.selectedFiles)
        };

        this._logsService.cleanupLogs(request).subscribe({
            next: () => {
                this._toastService.successToast('Log files deleted successfully');
                this.selectedFiles.clear();
                this.loadLogFiles();
            },
            error: () => {
                this._toastService.errorToast('Failed to delete log files');
            }
        });
    }

    formatFileSize(bytes: number): string {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    refresh(): void {
        this.loadLogFiles();
    }
}
