// front/src/app/pages/orders/orders.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule }       from '@angular/common';
import { Router }            from '@angular/router';
import { OrderService }      from '../../services/order.service';
import { Order, OrderStatus }from '../../models/order.model';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.css']
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  loading = true;
  errorMsg = '';
  // Expose l'enum pour le template
  statusEnum = OrderStatus;

  constructor(
    private orderService: OrderService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.orderService.getMyOrders().subscribe({
      next: (orders: Order[]) => {
        this.orders  = orders;
        this.loading = false;
      },
      error: () => {
        this.errorMsg = 'Erreur lors du chargement des commandes';
        this.loading  = false;
      }
    });
  }

  viewDetails(id: number): void {
    this.router.navigate(['/orders', id]);
  }
}
