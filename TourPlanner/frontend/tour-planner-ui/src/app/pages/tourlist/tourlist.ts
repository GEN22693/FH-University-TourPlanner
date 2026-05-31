import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';
import { TourService } from '../../core/services/tour.service';
import { Tour, TransportType } from '../../models/tour.model';

@Component({
  selector: 'app-tourlist',
  imports: [FormsModule, RouterLink],
  templateUrl: './tourlist.html',
})
export class Tourlist {
  private readonly authService = inject(AuthService);
  private readonly tourService = inject(TourService);
  private readonly router = inject(Router);

  readonly currentUser = this.authService.currentUser;

  readonly name = signal('');
  readonly description = signal('');
  readonly from = signal('');
  readonly to = signal('');
  readonly transportType = signal<TransportType>('Bike');
  readonly plannedDate = signal(new Date().toISOString().slice(0, 10));

  readonly searchText = signal('');
  readonly errorMessage = signal('');

  readonly transportTypes: TransportType[] = ['Bike', 'Hike', 'Run', 'Vacation'];

  readonly userTours = computed(() => {
    const user = this.currentUser();

    if (!user) {
      return [];
    }

    return this.tourService.getToursByUser(user.id);
  });

  readonly filteredTours = computed(() => {
    const search = this.searchText().toLowerCase().trim();

    const sortedTours = [...this.userTours()].sort((a, b) => {
      const dateA = a.plannedDate || '9999-12-31';
      const dateB = b.plannedDate || '9999-12-31';

      return dateA.localeCompare(dateB);
    });

    if (!search) {
      return sortedTours;
    }

    return sortedTours.filter((tour) =>
      `${tour.name} ${tour.description} ${tour.from} ${tour.to} ${tour.transportType} ${tour.plannedDate}`
        .toLowerCase()
        .includes(search),
    );
  });

  readonly upcomingTours = computed(() => {
    const today = new Date().toISOString().slice(0, 10);

    return this.userTours().filter((tour) => tour.plannedDate && tour.plannedDate >= today).length;
  });

  readonly totalDistance = computed(() =>
    this.userTours().reduce((sum, tour) => sum + tour.distance, 0),
  );

  addTour(): void {
    this.errorMessage.set('');

    const user = this.currentUser();

    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    const tourName = this.name().trim();
    const tourFrom = this.from().trim();
    const tourTo = this.to().trim();
    const date = this.plannedDate();

    if (!tourName || !tourFrom || !tourTo || !date) {
      this.errorMessage.set('Name, date, from and to are required.');
      return;
    }

    this.tourService.addTour({
      userId: user.id,
      name: tourName,
      description: this.description().trim(),
      from: tourFrom,
      to: tourTo,
      transportType: this.transportType(),
      plannedDate: date,
      distance: this.createFakeDistance(),
      estimatedTime: this.createFakeEstimatedTime(),
      routeInfo: 'Simulated route for frontend intermediate submission',
    });

    this.resetForm();
  }

  deleteTour(tourId: string): void {
    const confirmed = confirm('Do you really want to delete this tour?');

    if (!confirmed) {
      return;
    }

    this.tourService.deleteTour(tourId);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  setTransportType(value: string): void {
    this.transportType.set(value as TransportType);
  }

  formatDate(date: string | undefined): string {
    if (!date) {
      return 'Not scheduled';
    }

    return new Intl.DateTimeFormat('en', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    }).format(new Date(date));
  }

  getTransportLabel(type: TransportType): string {
    if (type === 'Bike') {
      return 'Cycling route';
    }

    if (type === 'Hike') {
      return 'Mountain path';
    }

    if (type === 'Run') {
      return 'Running track';
    }

    return 'Travel itinerary';
  }

  getCoverNumber(tour: Tour): string {
    const number = tour.name.length + tour.from.length + tour.to.length + tour.distance;

    return String(number).slice(-2).padStart(2, '0');
  }

  getRouteInitials(tour: Tour): string {
    const start = tour.from.trim().charAt(0).toUpperCase() || 'A';
    const end = tour.to.trim().charAt(0).toUpperCase() || 'B';

    return `${start} → ${end}`;
  }

  private resetForm(): void {
    this.name.set('');
    this.description.set('');
    this.from.set('');
    this.to.set('');
    this.transportType.set('Bike');
    this.plannedDate.set(new Date().toISOString().slice(0, 10));
  }

  private createFakeDistance(): number {
    return Math.floor(Math.random() * 40) + 5;
  }

  private createFakeEstimatedTime(): number {
    return Math.floor(Math.random() * 160) + 35;
  }
}
