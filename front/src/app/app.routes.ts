import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { HomeComponent } from './pages/home/home.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { ProductsComponent } from './pages/products/products.component';
import { ProductDetailComponent } from './pages/product-detail/product-detail.component';
import { CartComponent } from './pages/cart/cart.component';
import { OrdersComponent } from './pages/orders/orders.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { TermsComponent }   from './pages/terms/terms.component';
import { PrivacyComponent } from './pages/privacy/privacy.component';
import { CookiesComponent } from './pages/cookies/cookies.component';

// Guards
import { AuthGuard } from './gards/auth.guard';
import { AdminGuard } from './gards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'products', component: ProductsComponent },
  { path: 'product/:id', component: ProductDetailComponent },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
  { path: 'terms', component: TermsComponent },
  { path: 'privacy', component: PrivacyComponent },
  { path: 'cookies', component: CookiesComponent },
  { path: '**', redirectTo: 'home' },

  // Routes protégées (utilisateur connecté)
  {
    path: 'cart',
    component: CartComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'orders',
    component: OrdersComponent,
    canActivate: [AuthGuard]
  },

  // Section admin (seulement pour les admins)
  {
    path: 'admin',
    canActivate: [AuthGuard, AdminGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/admin/admin.component').then(m => m.AdminComponent)
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./pages/admin/products/admin-products/admin-products.component')
            .then(m => m.AdminProductsComponent)
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./pages/admin/orders/admin-orders/admin-orders.component')
            .then(m => m.AdminOrdersComponent)
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./pages/admin/users/admin-users/admin-users.component')
            .then(m => m.AdminUsersComponent)
      },
      {
        path: 'add-product',
        loadComponent: () =>
          import('./pages/admin/add-product/add-product.component')
            .then(m => m.AddProductComponent)
      }
    ]
  },

  // Fallback
  { path: '**', redirectTo: 'home' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}