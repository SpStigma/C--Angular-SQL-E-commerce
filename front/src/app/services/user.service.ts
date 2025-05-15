// File: front/src/app/services/user.service.ts

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`;
  private jsonHeaders = new HttpHeaders({ 'Content-Type': 'application/json' });

  constructor(private http: HttpClient) {}

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/all`);
  }

  /**
   * Update user role via PUT /api/users/{id}/role
   * newRole body is sent as JSON string
   */
  updateRole(id: number, newRole: string): Observable<any> {
    const url = `${this.apiUrl}/${id}/role`;
    return this.http.put(url, JSON.stringify(newRole), { headers: this.jsonHeaders });
  }
}