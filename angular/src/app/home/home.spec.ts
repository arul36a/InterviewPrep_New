import { TestBed } from '@angular/core/testing';
import { Home } from './home';

describe('Home', () => {
  it('doubles the count with computed', async () => {
    await TestBed.configureTestingModule({ imports: [Home] }).compileComponents();
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();
    fixture.componentInstance.inc();
    fixture.detectChanges();
    expect(fixture.componentInstance.doubled()).toBe(2);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Doubled: 2');
  });
});
