import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { BackupService, BackupInfo, BackupCleanupRequest } from '../../services/backup.service';
import { LoadingComponent } from '@shared/components/loading/loading.component';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';
import { ToastService } from '@shared/services/toast.service';

@Component({
    selector: 'app-backup-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TranslateModule,
        NgbTooltipModule,
        LoadingComponent,
        SvgIconDirective
    ],
    templateUrl: './backup-list.component.html',
    styleUrl: './backup-list.component.scss'
})
export class BackupListComponent implements OnInit {

    private _backupService = inject(BackupService);
    private _toastService = inject(ToastService);

    backups: BackupInfo[] = [];
    selectedFiles: Set<string> = new Set();
    isLoading = signal<boolean>(true);
    isCreatingBackup = signal<boolean>(false);
    isRestoring = signal<boolean>(false);

    ngOnInit(): void {
        this.loadBackups();
    }

    loadBackups(): void {
        this.isLoading.set(true);
        this._backupService.getBackups().subscribe({
            next: (backups) => {
                this.backups = backups;
                this.isLoading.set(false);
            },
            error: () => {
                this._toastService.errorToast('Failed to load backups');
                this.isLoading.set(false);
            }
        });
    }

    createBackup(): void {
        if (this.isCreatingBackup()) return;

        this.isCreatingBackup.set(true);
        this._backupService.createBackup().subscribe({
            next: () => {
                this._toastService.successToast('Backup created successfully');
                this.isCreatingBackup.set(false);
                this.loadBackups();
            },
            error: () => {
                this._toastService.errorToast('Failed to create backup');
                this.isCreatingBackup.set(false);
            }
        });
    }

    restoreBackup(backup: BackupInfo): void {
        const confirmMessage = `⚠️ WARNING: This will REPLACE the current database with the backup "${backup.name}".\n\nThis action cannot be undone!\n\nAre you absolutely sure you want to continue?`;

        if (!confirm(confirmMessage)) {
            return;
        }

        // Double confirmation for safety
        if (!confirm('Please confirm again: Are you SURE you want to restore the database?')) {
            return;
        }

        this.isRestoring.set(true);
        this._backupService.restoreBackup(backup.name).subscribe({
            next: () => {
                this._toastService.successToast('Database restored successfully');
                this.isRestoring.set(false);
            },
            error: () => {
                this._toastService.errorToast('Failed to restore database');
                this.isRestoring.set(false);
            }
        });
    }

    downloadBackup(backup: BackupInfo): void {
        this._backupService.downloadBackup(backup.name).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = backup.name;
                a.click();
                window.URL.revokeObjectURL(url);
                this._toastService.successToast('Backup downloaded');
            },
            error: () => {
                this._toastService.errorToast('Failed to download backup');
            }
        });
    }

    deleteBackup(backup: BackupInfo): void {
        if (!confirm(`Are you sure you want to delete ${backup.name}?`)) {
            return;
        }

        this._backupService.deleteBackup(backup.name).subscribe({
            next: () => {
                this._toastService.successToast('Backup deleted');
                this.loadBackups();
            },
            error: () => {
                this._toastService.errorToast('Failed to delete backup');
            }
        });
    }

    toggleSelection(backup: BackupInfo): void {
        if (this.selectedFiles.has(backup.name)) {
            this.selectedFiles.delete(backup.name);
        } else {
            this.selectedFiles.add(backup.name);
        }
    }

    isSelected(backup: BackupInfo): boolean {
        return this.selectedFiles.has(backup.name);
    }

    selectAll(): void {
        if (this.selectedFiles.size === this.backups.length) {
            this.selectedFiles.clear();
        } else {
            this.backups.forEach(b => this.selectedFiles.add(b.name));
        }
    }

    deleteSelected(): void {
        if (this.selectedFiles.size === 0) {
            this._toastService.warningToast('Please select backups to delete');
            return;
        }

        if (!confirm(`Are you sure you want to delete ${this.selectedFiles.size} backup(s)?`)) {
            return;
        }

        const request: BackupCleanupRequest = {
            fileNames: Array.from(this.selectedFiles)
        };

        this._backupService.cleanBackups(request).subscribe({
            next: () => {
                this._toastService.successToast('Backups deleted successfully');
                this.selectedFiles.clear();
                this.loadBackups();
            },
            error: () => {
                this._toastService.errorToast('Failed to delete backups');
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
        this.loadBackups();
    }
}
