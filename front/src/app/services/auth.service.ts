import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface LoginPayload {
  username: string;
  password: string;
}

export interface RegisterPayload {
  username: string;
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  /**
   * Enregistre un nouvel utilisateur
   */
  register(payload: RegisterPayload): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, payload);
  }

  /**
   * Authentifie l'utilisateur et stocke le token JWT
   */
  login(payload: LoginPayload): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(`${this.baseUrl}/login`, payload)
      .pipe(
        tap(response => {
          localStorage.setItem('token', response.token);
        })
      );
  }

  /**
   * Supprime le token pour déconnecter l'utilisateur
   */
  logout(): void {
    localStorage.removeItem('token');
  }

  /**
   * Indique si l'utilisateur est authentifié
   */
  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
  }

  /**
   * Récupère le token JWT stocké
   */
  getToken(): string | null {
    return localStorage.getItem('token');
  }
}
