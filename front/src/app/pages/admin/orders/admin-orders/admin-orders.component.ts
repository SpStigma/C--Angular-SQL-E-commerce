// front/src/app/pages/admin/orders/admin-orders/admin-orders.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule }       from '@angular/common';
import { firstValueFrom }     from 'rxjs';
import { OrderService }       from '../../../../services/order.service';
import { Order }              from '../../../../models/order.model';

@Component({
  selector: 'app-admin-orders',
  standalone: true,               // <-- mode standalone activé
  imports: [CommonModule],        // <-- importe NgIf, NgFor, pipes…
  templateUrl: './admin-orders.component.html',
  styleUrls: ['./admin-orders.component.css']
})
export class AdminOrdersComponent implements OnInit {
  orders: Order[] = [];
  loading = true;
  errorMsg = '';

  constructor(private orderService: OrderService) {}

  async ngOnInit(): Promise<void> {
    try {
      this.orders = await firstValueFrom(this.orderService.getAllOrders());
    } catch {
      this.errorMsg = 'Erreur lors du chargement des commandes';
    } finally {
      this.loading = false;
    }
  }

  viewDetails(id: number): void {
    console.log('Voir détails commande admin', id);
  }
}
