import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { CustomerService } from '../../services/customer.service';
import { Customer } from '../../models/crm.models';

@Component({
  selector: 'crm-customer-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.scss'
})
export class CustomerListComponent implements OnInit {
  private _customerService = inject(CustomerService);
  private _fb = inject(FormBuilder);

  customers = signal<Customer[]>([]);
  isLoading = signal<boolean>(false);
  isModalOpen = signal<boolean>(false);
  editingCustomer = signal<Customer | null>(null);

  customerForm: FormGroup = this._fb.group({
    id: [0],
    name: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.email]],
    phone: [''],
    company: [''],
    address: [''],
    industry: [''],
    notes: ['']
  });

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading.set(true);
    this._customerService.getCustomers().subscribe({
      next: (res) => {
        this.customers.set(res.data || []);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  openCreateModal(): void {
    this.editingCustomer.set(null);
    this.customerForm.reset({ id: 0, name: '', email: '', phone: '', company: '', address: '', industry: '', notes: '' });
    this.isModalOpen.set(true);
  }

  openEditModal(customer: Customer): void {
    this.editingCustomer.set(customer);
    this.customerForm.patchValue({
      id: customer.id || 0,
      name: customer.name,
      email: customer.email || '',
      phone: customer.phone || '',
      company: customer.company || '',
      address: customer.address || '',
      industry: customer.industry || '',
      notes: customer.notes || ''
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveCustomer(): void {
    if (this.customerForm.invalid) return;
    const formValue = this.customerForm.value as Customer;
    this.isLoading.set(true);

    if (formValue.id && formValue.id > 0) {
      this._customerService.updateCustomer(formValue.id, formValue).subscribe({
        next: () => {
          this.closeModal();
          this.loadCustomers();
        },
        error: () => this.isLoading.set(false)
      });
    } else {
      this._customerService.createCustomer(formValue).subscribe({
        next: () => {
          this.closeModal();
          this.loadCustomers();
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  deleteCustomer(id: number | undefined): void {
    if (!id || !confirm('Are you sure you want to delete this customer?')) return;
    this.isLoading.set(true);
    this._customerService.deleteCustomer(id).subscribe({
      next: () => this.loadCustomers(),
      error: () => this.isLoading.set(false)
    });
  }
}
