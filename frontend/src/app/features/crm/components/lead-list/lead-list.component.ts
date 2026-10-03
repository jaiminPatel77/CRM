import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { LeadService } from '../../services/lead.service';
import { Lead, EnumLeadStatus } from '../../models/crm.models';

@Component({
  selector: 'crm-lead-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './lead-list.component.html',
  styleUrl: './lead-list.component.scss'
})
export class LeadListComponent implements OnInit {
  private _leadService = inject(LeadService);
  private _fb = inject(FormBuilder);

  enumLeadStatus = EnumLeadStatus;
  leads = signal<Lead[]>([]);
  isLoading = signal<boolean>(false);
  isModalOpen = signal<boolean>(false);
  editingLead = signal<Lead | null>(null);

  leadForm: FormGroup = this._fb.group({
    id: [0],
    title: ['', [Validators.required, Validators.maxLength(150)]],
    firstName: [''],
    lastName: [''],
    email: ['', [Validators.email]],
    phone: [''],
    company: [''],
    estimatedValue: [null],
    status: [EnumLeadStatus.New, [Validators.required]],
    source: [''],
    notes: ['']
  });

  ngOnInit(): void {
    this.loadLeads();
  }

  loadLeads(): void {
    this.isLoading.set(true);
    this._leadService.getLeads().subscribe({
      next: (res) => {
        this.leads.set(res.data || []);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getStatusBadgeClass(status: EnumLeadStatus): string {
    switch (status) {
      case EnumLeadStatus.New: return 'bg-info text-dark';
      case EnumLeadStatus.Contacted: return 'bg-warning text-dark';
      case EnumLeadStatus.Qualified: return 'bg-success';
      case EnumLeadStatus.Unqualified: return 'bg-danger';
      case EnumLeadStatus.Converted: return 'bg-primary';
      default: return 'bg-secondary';
    }
  }

  getStatusLabel(status: EnumLeadStatus): string {
    return EnumLeadStatus[status] || 'New';
  }

  openCreateModal(): void {
    this.editingLead.set(null);
    this.leadForm.reset({
      id: 0,
      title: '',
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      company: '',
      estimatedValue: null,
      status: EnumLeadStatus.New,
      source: '',
      notes: ''
    });
    this.isModalOpen.set(true);
  }

  openEditModal(lead: Lead): void {
    this.editingLead.set(lead);
    this.leadForm.patchValue({
      id: lead.id || 0,
      title: lead.title,
      firstName: lead.firstName || '',
      lastName: lead.lastName || '',
      email: lead.email || '',
      phone: lead.phone || '',
      company: lead.company || '',
      estimatedValue: lead.estimatedValue,
      status: lead.status,
      source: lead.source || '',
      notes: lead.notes || ''
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveLead(): void {
    if (this.leadForm.invalid) return;
    const formValue = this.leadForm.value as Lead;
    formValue.status = Number(formValue.status);
    this.isLoading.set(true);

    if (formValue.id && formValue.id > 0) {
      this._leadService.updateLead(formValue.id, formValue).subscribe({
        next: () => {
          this.closeModal();
          this.loadLeads();
        },
        error: () => this.isLoading.set(false)
      });
    } else {
      this._leadService.createLead(formValue).subscribe({
        next: () => {
          this.closeModal();
          this.loadLeads();
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  deleteLead(id: number | undefined): void {
    if (!id || !confirm('Are you sure you want to delete this lead?')) return;
    this.isLoading.set(true);
    this._leadService.deleteLead(id).subscribe({
      next: () => this.loadLeads(),
      error: () => this.isLoading.set(false)
    });
  }
}
