import { Component, OnInit } from '@angular/core';
import { CommonModule }       from '@angular/common';
import { Router }             from '@angular/router';                    
import { OrderService }       from '../../../../services/order.service';
import { Order, OrderStatus } from '../../../../models/order.model';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-orders.component.html',
  styleUrls: ['./admin-orders.component.css']
})
export class AdminOrdersComponent implements OnInit {
  orders: Order[]      = [];
  loading: boolean     = true;                                         
  errorMsg: string     = '';                                             
  statusEnum = OrderStatus;                                         

  // ← injection du service et du router
  constructor(
    private orderService: OrderService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.orderService.getAllOrders().subscribe({
      next: (orders) => {
        this.orders  = orders;
        this.loading = false;
      },
      error: (err) => {
        this.errorMsg = err.error?.message || 'Erreur lors du chargement des commandes';
        this.loading  = false;
      }
    });
  }

  viewDetails(id: number): void {
    this.router.navigate(['/admin/orders', id]);
  }
}
