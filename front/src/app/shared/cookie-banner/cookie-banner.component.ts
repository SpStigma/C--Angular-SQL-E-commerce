import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-cookie-banner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cookie-banner.component.html',
  styleUrls: ['./cookie-banner.component.css']
})
export class CookieBannerComponent implements OnInit {
  showBanner: boolean = false;

  ngOnInit(): void {
    const consentCookie = document.cookie
      .split('; ')
      .find(row => row.startsWith('cookieConsent='));
    if (!consentCookie) {
      this.showBanner = true;
    }
  }

  acceptCookies(): void {
    const expiryDate = new Date();
    expiryDate.setFullYear(expiryDate.getFullYear() + 1);
    document.cookie = `cookieConsent=true; expires=${expiryDate.toUTCString()}; path=/`;
    this.showBanner = false;
  }
}
