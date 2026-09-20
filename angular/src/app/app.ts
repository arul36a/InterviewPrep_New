import { Component, effect, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

type Theme = 'dark' | 'light';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  readonly theme = signal<Theme>(this.readStoredTheme());

  constructor() {
    effect(() => {
      const theme = this.theme();
      document.documentElement.dataset['theme'] = theme;
      localStorage.setItem('theme', theme);
    });
  }

  toggleTheme() {
    this.theme.update((current) => (current === 'dark' ? 'light' : 'dark'));
  }

  private readStoredTheme(): Theme {
    const stored = localStorage.getItem('theme');
    return stored === 'light' ? 'light' : 'dark';
  }
}
