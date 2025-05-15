import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { OrderService } from '../../../services/order.service';
import { Order } from '../../../models/order.model';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-orders.component.html',
  styleUrls: ['./admin-orders.component.css']
})
export class AdminOrdersComponent implements OnInit {
  orders: Order[] = [];
  loading = false;
  errorMessage: string | null = null;

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.fetchAllOrders();
  }

  private fetchAllOrders(): void {
    this.loading = true;
    this.orderService.getAllOrders().subscribe({
      next: (data) => {
        this.orders = data;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = err.message || 'Erreur lors du chargement';
        this.loading = false;
      }
    });
  }
}