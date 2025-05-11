import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = environment.apiUrl + '/users';

  constructor(private http: HttpClient) {}

  login(user: { username: string, email: string, password: string }) {
    return this.http.post(`${environment.apiUrl}/users/login`, user);
  }

  getRole(): string {
    const token = localStorage.getItem('token');
    if (!token) return '';
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload['role'] || '';
  }

  getUsername(): string {
    const token = localStorage.getItem('token');
    if (!token) return '';
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload['sub'] || '';
  }

  logout(): void {
    localStorage.removeItem('token');
  }
}
