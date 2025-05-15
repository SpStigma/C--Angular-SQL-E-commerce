import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { JwtHelperService } from '@auth0/angular-jwt';

@Injectable({ providedIn: 'root' })
export class AdminGuard implements CanActivate {
  private jwtHelper = new JwtHelperService();

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    const token = this.authService.getToken();
    if (!token || this.jwtHelper.isTokenExpired(token)) {
      this.router.navigate(['/login']);
      return false;
    }
    const decoded = this.jwtHelper.decodeToken(token);
    const role = decoded['role'];
    if (role === 'admin') {
      return true;
    }
    // Redirect non-admins
    this.router.navigate(['/']);
    return false;
  }
}