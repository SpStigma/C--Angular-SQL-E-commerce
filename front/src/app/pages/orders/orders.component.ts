import { Component, OnInit } from '@angular/core';
import { CommonModule }       from '@angular/common';
import { OrderService }       from '../../services/order.service';
import { Order }              from '../../models/order.model';

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

  constructor(private orderService: OrderService) {}

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
    console.log('Voir détails commande', id);
  }
}
