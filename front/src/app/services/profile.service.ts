import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Profile {
  username: string;
  email: string;
}

export interface UpdateProfileDto {
  username?: string;
  email?: string;
  currentPassword?: string;
  newPassword?: string;
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private apiUrl = 'http://localhost:5292/api/users';

  constructor(private http: HttpClient) {}

  getProfile(): Observable<Profile> {
    return this.http.get<Profile>(`${this.apiUrl}/profile`);
  }

  updateProfile(data: UpdateProfileDto): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/profile`, data);
  }

  deleteAccount(): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/profile`);
  }
}