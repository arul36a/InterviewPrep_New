import { Component, computed, signal } from '@angular/core';

/** Phase 1 — standalone component, signal state, computed, @if / @for. */
@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  readonly count = signal(0);
  readonly doubled = computed(() => this.count() * 2);
  readonly lines = signal([
    { code: '4000', name: 'Book income' },
    { code: '5000', name: 'Meals (permanent)' },
  ]);

  inc() {
    this.count.update((n) => n + 1);
  }

  dec() {
    this.count.update((n) => n - 1);
  }
}
