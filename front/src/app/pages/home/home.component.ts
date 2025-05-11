import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ProductService, Product } from '../../services/product.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  products: Product[] = [];
  bestSellers: Product[] = [];
  isAdmin = false;
  username = '';

  constructor(
    private productService: ProductService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.productService.getAll().subscribe((products: Product[]) => {
      this.products = products;
      this.bestSellers = [...products]
        .sort((a, b) => (b.soldCount ?? 0) - (a.soldCount ?? 0))
        .slice(0, 3);
    });

    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.username = payload['sub'];
      this.isAdmin = payload['role'] === 'admin';
    }
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}
