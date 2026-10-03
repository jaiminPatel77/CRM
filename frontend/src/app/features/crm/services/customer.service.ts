import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Customer } from '../models/crm.models';
import { ApiOkResponse } from '@shared/models/api-response';
import { BaseService } from '@shared/services/base.service';

@Injectable({
  providedIn: 'root'
})
export class CustomerService extends BaseService<Customer> {
  private _http = inject(HttpClient);

  constructor() {
    super(BaseService.ApiUrls.Customer);
  }

  getCustomers(): Observable<ApiOkResponse<Customer[]>> {
    return this._http.get<ApiOkResponse<Customer[]>>(this.baseUrl);
  }

  getCustomerById(id: number): Observable<ApiOkResponse<Customer>> {
    return this._http.get<ApiOkResponse<Customer>>(`${this.baseUrl}/${id}`);
  }

  createCustomer(customer: Customer): Observable<ApiOkResponse<Customer>> {
    return this._http.post<ApiOkResponse<Customer>>(this.baseUrl, customer);
  }

  updateCustomer(id: number, customer: Customer): Observable<ApiOkResponse<Customer>> {
    return this._http.put<ApiOkResponse<Customer>>(`${this.baseUrl}/${id}`, customer);
  }

  deleteCustomer(id: number): Observable<ApiOkResponse<boolean>> {
    return this._http.delete<ApiOkResponse<boolean>>(`${this.baseUrl}/${id}`);
  }
}
