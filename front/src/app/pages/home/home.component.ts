import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html'
})
export class HomeComponent {
  products = [
    {
      title: 'PlayStation 5',
      description: 'Console nouvelle génération avec lecteur Blu-ray.',
      price: 549.99,
      image: 'https://picsum.photos/seed/ps5/400/250'
    },
    {
      title: 'Xbox Series X',
      description: 'Puissance de jeu ultime avec SSD ultra rapide.',
      price: 499.99,
      image: 'https://picsum.photos/seed/xbox/400/250'
    },
    {
      title: 'Nintendo Switch OLED',
      description: 'Console hybride avec écran OLED vibrant.',
      price: 349.99,
      image: 'https://picsum.photos/seed/switch/400/250'
    },
    {
      title: 'Steam Deck',
      description: 'Console portable avec bibliothèque Steam intégrée.',
      price: 419.99,
      image: 'https://picsum.photos/seed/steamdeck/400/250'
    }
  ];
}
