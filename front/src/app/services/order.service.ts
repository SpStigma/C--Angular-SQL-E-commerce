import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { Order } from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  /** Récupère les commandes de l'utilisateur connecté */
  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.apiUrl);
  }

  /** Récupère toutes les commandes (admin) */
  getAllOrders(): Observable<Order[]> {
    // avant : return this.http.get<Order[]>(`${this.baseUrl}/orders/all`);
    return this.http.get<Order[]>(`${this.apiUrl}`);
  }

  /** Récupère une commande par son ID */
  getOrderById(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${id}`);
  }
}