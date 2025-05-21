import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ProductService, Product } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  products: Product[] = [];
  carouselProducts: Product[] = [];
  paginatedProducts: Product[] = [];
  pages: number[] = [];
  pageSize = 8;
  currentPage = 1;
  isAdmin = false;
  username?: string;
  isLoggedIn = false;
  cartItemCount = 0;

  successMessage: string | null = null;
  errorMessage: string | null = null;

  constructor(
    private productService: ProductService,
    private cartService: CartService,
    public router: Router
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.checkToken();
    if (this.isLoggedIn) {
      this.loadCartCount();
    }
  }

  private loadProducts(): void {
    this.productService.getAll().subscribe(raw => {
      this.products = raw.map(p => {
        if (p.imageUrl?.startsWith('/')) {
          p.imageUrl = environment.apiUrl + p.imageUrl;
        }
        if (!p.imageUrl) {
          p.imageUrl = 'assets/placeholder.png';
        }
        return p;
      });
      this.carouselProducts = [...this.products]
        .sort(() => 0.5 - Math.random())
        .slice(0, 4);
      this.initPagination();
    });
  }

  private initPagination(): void {
    const total = Math.ceil(this.products.length / this.pageSize);
    this.pages = Array.from({ length: total }, (_, i) => i + 1);
    this.setPaginatedProducts();
  }

  private setPaginatedProducts(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    this.paginatedProducts = this.products.slice(start, start + this.pageSize);
  }

  prevPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.setPaginatedProducts();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.pages.length) {
      this.currentPage++;
      this.setPaginatedProducts();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.pages.length) {
      this.currentPage = page;
      this.setPaginatedProducts();
    }
  }

  addToCart(product: Product): void {
    this.cartService.addToCart(product.id).subscribe({
      next: () => {
        this.clearMessages();
        this.successMessage = `“${product.name}” ajouté au panier !`;
        this.productService.getById(product.id).subscribe({
          next: fresh => product.stock = fresh.stock,
          error: () => {}
        });
        this.cartItemCount++;
        setTimeout(() => this.clearMessages(), 3000);
      },
      error: (err: any) => {
        this.clearMessages();
        this.errorMessage = err.error?.message || 'Erreur lors de l’ajout';
        setTimeout(() => this.clearMessages(), 3000);
      }
    });
  }

  private loadCartCount(): void {
    this.cartService.getCart().subscribe({
      next: data => this.cartItemCount = data.itemCount,
      error: () => this.cartItemCount = 0
    });
  }

  private checkToken(): void {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.username = payload.sub;
      this.isAdmin = payload.role === 'admin';
      this.isLoggedIn = true;
    } else {
      this.username = undefined;
      this.isAdmin = false;
      this.isLoggedIn = false;
    }
  }

  logout(): void {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  private clearMessages(): void {
    this.successMessage = null;
    this.errorMessage = null;
  }

  viewProduct(product: Product): void {
    this.router.navigate(['/product', product.id]);
  }
}
