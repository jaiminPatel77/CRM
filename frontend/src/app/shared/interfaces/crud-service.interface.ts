import { Observable } from 'rxjs';

export interface ICrudService<T> {
    baseUrl: string;
    deleteRecord(id: number): Observable<any>;
    // Optional methods standardizing CRUD if needed
    getRecord?(id: number): Observable<any>;
    updateRecord?(id: number | undefined, record: T): Observable<any>;
}
