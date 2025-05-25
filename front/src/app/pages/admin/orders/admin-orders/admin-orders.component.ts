// front/src/app/pages/admin/orders/admin-orders/admin-orders.component.ts

import { Component, OnInit }               from '@angular/core';
import { CommonModule }                    from '@angular/common';
import { RouterModule }                    from '@angular/router';
import { firstValueFrom }                  from 'rxjs';

import { OrderService }                    from '../../../../services/order.service';
import { Order }                           from '../../../../models/order.model';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-orders.component.html',
})
export class AdminOrdersComponent implements OnInit {
  orders: Order[] = [];
  loading = false;
  errorMessage = '';

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.fetchOrders();
  }

  private async fetchOrders(): Promise<void> {
    this.loading = true;
    this.errorMessage = '';

    try {
      // Si vous avez ajouté getAllOrders() côté service/admin, sinon getOrders()
      this.orders = await firstValueFrom(this.orderService.getOrders());
    } catch (err: any) {
      console.error('Erreur chargement commandes :', err);
      this.errorMessage = err?.message || 'Erreur chargement commandes';
    } finally {
      this.loading = false;
    }
  }
}
