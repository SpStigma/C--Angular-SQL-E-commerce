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
}
