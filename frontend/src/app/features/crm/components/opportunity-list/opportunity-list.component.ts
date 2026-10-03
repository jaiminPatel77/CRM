import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { OpportunityService } from '../../services/opportunity.service';
import { Opportunity, EnumOpportunityStage } from '../../models/crm.models';

@Component({
  selector: 'crm-opportunity-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './opportunity-list.component.html',
  styleUrl: './opportunity-list.component.scss'
})
export class OpportunityListComponent implements OnInit {
  private _opportunityService = inject(OpportunityService);
  private _fb = inject(FormBuilder);

  enumOpportunityStage = EnumOpportunityStage;
  opportunities = signal<Opportunity[]>([]);
  isLoading = signal<boolean>(false);
  isModalOpen = signal<boolean>(false);
  editingOpportunity = signal<Opportunity | null>(null);

  opportunityForm: FormGroup = this._fb.group({
    id: [0],
    title: ['', [Validators.required, Validators.maxLength(150)]],
    amount: [0, [Validators.required, Validators.min(0)]],
    stage: [EnumOpportunityStage.Qualification, [Validators.required]],
    probability: [10, [Validators.required, Validators.min(0), Validators.max(100)]],
    expectedCloseDate: [null],
    notes: ['']
  });

  ngOnInit(): void {
    this.loadOpportunities();
  }

  loadOpportunities(): void {
    this.isLoading.set(true);
    this._opportunityService.getOpportunities().subscribe({
      next: (res) => {
        this.opportunities.set(res.data || []);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getStageBadgeClass(stage: EnumOpportunityStage): string {
    switch (stage) {
      case EnumOpportunityStage.Qualification: return 'bg-info text-dark';
      case EnumOpportunityStage.Proposal: return 'bg-warning text-dark';
      case EnumOpportunityStage.Negotiation: return 'bg-primary';
      case EnumOpportunityStage.ClosedWon: return 'bg-success';
      case EnumOpportunityStage.ClosedLost: return 'bg-danger';
      default: return 'bg-secondary';
    }
  }

  getStageLabel(stage: EnumOpportunityStage): string {
    return EnumOpportunityStage[stage] || 'Qualification';
  }

  openCreateModal(): void {
    this.editingOpportunity.set(null);
    this.opportunityForm.reset({
      id: 0,
      title: '',
      amount: 0,
      stage: EnumOpportunityStage.Qualification,
      probability: 10,
      expectedCloseDate: null,
      notes: ''
    });
    this.isModalOpen.set(true);
  }

  openEditModal(opportunity: Opportunity): void {
    this.editingOpportunity.set(opportunity);
    this.opportunityForm.patchValue({
      id: opportunity.id || 0,
      title: opportunity.title,
      amount: opportunity.amount,
      stage: opportunity.stage,
      probability: opportunity.probability,
      expectedCloseDate: opportunity.expectedCloseDate ? opportunity.expectedCloseDate.substring(0, 10) : null,
      notes: opportunity.notes || ''
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveOpportunity(): void {
    if (this.opportunityForm.invalid) return;
    const formValue = this.opportunityForm.value as Opportunity;
    formValue.stage = Number(formValue.stage);
    this.isLoading.set(true);

    if (formValue.id && formValue.id > 0) {
      this._opportunityService.updateOpportunity(formValue.id, formValue).subscribe({
        next: () => {
          this.closeModal();
          this.loadOpportunities();
        },
        error: () => this.isLoading.set(false)
      });
    } else {
      this._opportunityService.createOpportunity(formValue).subscribe({
        next: () => {
          this.closeModal();
          this.loadOpportunities();
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  deleteOpportunity(id: number | undefined): void {
    if (!id || !confirm('Are you sure you want to delete this opportunity?')) return;
    this.isLoading.set(true);
    this._opportunityService.deleteOpportunity(id).subscribe({
      next: () => this.loadOpportunities(),
      error: () => this.isLoading.set(false)
    });
  }
}
