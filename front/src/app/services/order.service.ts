// front/src/app/services/order.service.ts
import { Injectable } from '@angular/core';
import { HttpClient }   from '@angular/common/http';
import { environment }  from '../../environments/environment';
import { Observable }   from 'rxjs';
import { Order }        from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private apiUrl = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  /** Place une commande à partir du panier actuel */
  placeOrder(): Observable<Order> {
    return this.http.post<Order>(this.apiUrl, {});
  }

  /** Récupère les commandes de l'utilisateur */
  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.apiUrl);
  }

  /** Récupère une commande par son ID */
  getOrderById(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${id}`);
  }

  // Nouvelle méthode pour récupérer TOUTES les commandes (admin)
  getAllOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/all`);
  }
}
