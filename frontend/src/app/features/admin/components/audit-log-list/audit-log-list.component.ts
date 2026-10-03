import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgbTooltipModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { AuditLogService, AuditLogDto, AuditLogQueryParams } from '../../services/audit-log.service';
import { LoadingComponent } from '@shared/components/loading/loading.component';
import { SvgIconDirective } from '@shared/directives/svg-icon.directive';
import { ToastService } from '@shared/services/toast.service';

@Component({
    selector: 'app-audit-log-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TranslateModule,
        NgbTooltipModule,
        NgbPaginationModule,
        LoadingComponent,
        SvgIconDirective
    ],
    templateUrl: './audit-log-list.component.html',
    styleUrl: './audit-log-list.component.scss'
})
export class AuditLogListComponent implements OnInit {

    private _auditLogService = inject(AuditLogService);
    private _toastService = inject(ToastService);

    auditLogs: AuditLogDto[] = [];
    isLoading = signal<boolean>(true);

    // Pagination
    page = 1;
    pageSize = 20;
    totalRecords = 0;

    // Filtering
    searchFilter = '';

    ngOnInit(): void {
        this.loadAuditLogs();
    }

    loadAuditLogs(): void {
        this.isLoading.set(true);
        let filter: string | undefined = undefined;
        if (this.searchFilter?.trim()) {
            const search = this.searchFilter.trim();
            filter = `(UserId=*${search}*)`;
        }
        const query: AuditLogQueryParams = {
            page: this.page,
            pageSize: this.pageSize,
            filter: filter || undefined
        };

        this._auditLogService.getAuditLogs(query).subscribe({
            next: (result) => {
                this.auditLogs = result.items;
                this.totalRecords = result.count;
                this.isLoading.set(false);
            },
            error: () => {
                this._toastService.errorToast('Failed to load audit logs');
                this.isLoading.set(false);
            }
        });
    }

    onSearch(): void {
        this.page = 1;
        this.loadAuditLogs();
    }

    onPageChange(): void {
        this.loadAuditLogs();
    }

    clearFilter(): void {
        this.searchFilter = '';
        this.page = 1;
        this.loadAuditLogs();
    }

    refresh(): void {
        this.loadAuditLogs();
    }

    formatChangedData(data: string | undefined): string {
        if (!data) return '-';
        try {
            const parsed = JSON.parse(data);
            return JSON.stringify(parsed, null, 2);
        } catch {
            return data;
        }
    }
}
