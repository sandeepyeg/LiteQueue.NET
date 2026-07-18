import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: `
    <header class="app-header">
      <a routerLink="/" class="app-logo">LiteQueue</a>
      <nav class="app-nav">
        <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}">Dashboard</a>
        <a routerLink="/topics" routerLinkActive="active">Topics</a>
      </nav>
    </header>
    <main class="app-main">
      <router-outlet></router-outlet>
    </main>
  `,
  styles: [`
    .app-header {
      background: #1a1a2e;
      color: #e0e0e0;
      padding: 0 24px;
      display: flex;
      align-items: center;
      height: 56px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.15);
    }
    .app-logo {
      font-size: 20px;
      font-weight: 700;
      color: #e94560;
      text-decoration: none;
      margin-right: 40px;
    }
    .app-nav { display: flex; gap: 24px; }
    .app-nav a {
      color: #a0a0b0;
      text-decoration: none;
      font-size: 14px;
      padding: 4px 0;
      border-bottom: 2px solid transparent;
      transition: color 0.2s, border-color 0.2s;
    }
    .app-nav a:hover, .app-nav a.active { color: #e0e0e0; border-bottom-color: #e94560; }
    .app-main { padding: 24px; max-width: 1200px; margin: 0 auto; }
  `]
})
export class AppComponent { }
