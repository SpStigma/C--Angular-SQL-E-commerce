import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { ProductsComponent } from './pages/products/products.component';
import { CartComponent } from './pages/cart/cart.component';
import { OrdersComponent } from './pages/orders/orders.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'products', component: ProductsComponent },
  { path: 'cart', component: CartComponent },
  { path: 'orders', component: OrdersComponent },

  // Admin dashboard root
  {
    path: 'admin',
    loadComponent: () =>
      import('./pages/admin/admin.component').then(m => m.AdminComponent)
  },

{
  path: 'admin/products',
  loadComponent: () =>
    import('./pages/admin/products/admin-products/admin-products.component').then(m => m.AdminProductsComponent)
},
{
  path: 'admin/orders',
  loadComponent: () =>
    import('./pages/admin/orders/admin-orders/admin-orders.component').then(m => m.AdminOrdersComponent)
},
{
  path: 'admin/users',
  loadComponent: () =>
    import('./pages/admin/users/admin-users/admin-users.component').then(m => m.AdminUsersComponent)
}
];
