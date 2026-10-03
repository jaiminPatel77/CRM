import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ActivityService } from '../../services/activity.service';
import { Activity, EnumActivityType } from '../../models/crm.models';

@Component({
  selector: 'crm-activity-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './activity-list.component.html',
  styleUrl: './activity-list.component.scss'
})
export class ActivityListComponent implements OnInit {
  private _activityService = inject(ActivityService);
  private _fb = inject(FormBuilder);

  enumActivityType = EnumActivityType;
  activities = signal<Activity[]>([]);
  isLoading = signal<boolean>(false);
  isModalOpen = signal<boolean>(false);
  editingActivity = signal<Activity | null>(null);

  activityForm: FormGroup = this._fb.group({
    id: [0],
    subject: ['', [Validators.required, Validators.maxLength(200)]],
    type: [EnumActivityType.Task, [Validators.required]],
    dueDate: [null],
    isCompleted: [false],
    description: ['']
  });

  ngOnInit(): void {
    this.loadActivities();
  }

  loadActivities(): void {
    this.isLoading.set(true);
    this._activityService.getActivities().subscribe({
      next: (res) => {
        this.activities.set(res.data || []);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getTypeBadgeClass(type: EnumActivityType): string {
    switch (type) {
      case EnumActivityType.Call: return 'bg-primary';
      case EnumActivityType.Meeting: return 'bg-purple text-white bg-dark';
      case EnumActivityType.Email: return 'bg-info text-dark';
      case EnumActivityType.Task: return 'bg-warning text-dark';
      case EnumActivityType.Note: return 'bg-secondary';
      default: return 'bg-secondary';
    }
  }

  getTypeLabel(type: EnumActivityType): string {
    return EnumActivityType[type] || 'Task';
  }

  openCreateModal(): void {
    this.editingActivity.set(null);
    this.activityForm.reset({
      id: 0,
      subject: '',
      type: EnumActivityType.Task,
      dueDate: null,
      isCompleted: false,
      description: ''
    });
    this.isModalOpen.set(true);
  }

  openEditModal(activity: Activity): void {
    this.editingActivity.set(activity);
    this.activityForm.patchValue({
      id: activity.id || 0,
      subject: activity.subject,
      type: activity.type,
      dueDate: activity.dueDate ? activity.dueDate.substring(0, 10) : null,
      isCompleted: activity.isCompleted,
      description: activity.description || ''
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  toggleCompleted(activity: Activity): void {
    const updated = { ...activity, isCompleted: !activity.isCompleted };
    this._activityService.updateActivity(activity.id!, updated).subscribe({
      next: () => this.loadActivities()
    });
  }

  saveActivity(): void {
    if (this.activityForm.invalid) return;
    const formValue = this.activityForm.value as Activity;
    formValue.type = Number(formValue.type);
    this.isLoading.set(true);

    if (formValue.id && formValue.id > 0) {
      this._activityService.updateActivity(formValue.id, formValue).subscribe({
        next: () => {
          this.closeModal();
          this.loadActivities();
        },
        error: () => this.isLoading.set(false)
      });
    } else {
      this._activityService.createActivity(formValue).subscribe({
        next: () => {
          this.closeModal();
          this.loadActivities();
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  deleteActivity(id: number | undefined): void {
    if (!id || !confirm('Are you sure you want to delete this activity?')) return;
    this.isLoading.set(true);
    this._activityService.deleteActivity(id).subscribe({
      next: () => this.loadActivities(),
      error: () => this.isLoading.set(false)
    });
  }
}
