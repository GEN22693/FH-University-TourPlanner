import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';
import { TourService } from '../../core/services/tour.service';
import { Tour, TransportType } from '../../models/tour.model';
import { TourLog } from '../../models/tour-log.model';
import { MapPlaceholder } from '../../shared/components/map-placeholder/map-placeholder';
import { Navbar } from '../../shared/components/navbar/navbar';

@Component({
  selector: 'app-tour-detail',
  imports: [FormsModule, RouterLink, MapPlaceholder, Navbar],
  templateUrl: './tour-detail.html',
})
export class TourDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly tourService = inject(TourService);

  readonly currentUser = this.authService.currentUser;
  readonly tourId = signal(this.route.snapshot.paramMap.get('id') ?? '');

  readonly transportTypes: TransportType[] = ['Bike', 'Hike', 'Run', 'Vacation'];

  readonly tour = computed(() => this.tourService.getTourById(this.tourId()));
  readonly logs = computed(() => this.tourService.getLogsByTour(this.tourId()));

  readonly isEditTourModalOpen = signal(false);
  readonly isLogModalOpen = signal(false);

  readonly name = signal('');
  readonly description = signal('');
  readonly from = signal('');
  readonly to = signal('');
  readonly transportType = signal<TransportType>('Bike');
  readonly plannedDate = signal(new Date().toISOString().slice(0, 10));
  readonly tourErrorMessage = signal('');

  readonly logDate = signal(new Date().toISOString().slice(0, 16));
  readonly logComment = signal('');
  readonly logDifficulty = signal(1);
  readonly logDistance = signal(5);
  readonly logTime = signal(30);
  readonly logRating = signal(5);
  readonly logErrorMessage = signal('');
  readonly editingLogId = signal<string | null>(null);

  readonly averageRating = computed(() => {
    const tourLogs = this.logs();

    if (tourLogs.length === 0) {
      return 0;
    }

    const sum = tourLogs.reduce((total, log) => total + log.rating, 0);

    return Math.round((sum / tourLogs.length) * 10) / 10;
  });

  readonly totalLoggedDistance = computed(() =>
    this.logs().reduce((total, log) => total + log.totalDistance, 0),
  );

  readonly totalLoggedTime = computed(() =>
    this.logs().reduce((total, log) => total + log.totalTime, 0),
  );

  constructor() {
    const selectedTour = this.tour();

    if (selectedTour) {
      this.name.set(selectedTour.name);
      this.description.set(selectedTour.description);
      this.from.set(selectedTour.from);
      this.to.set(selectedTour.to);
      this.transportType.set(selectedTour.transportType);
      this.plannedDate.set(selectedTour.plannedDate || new Date().toISOString().slice(0, 10));
    }
  }

  openEditTourModal(): void {
    const selectedTour = this.tour();

    if (!selectedTour) {
      return;
    }

    this.tourErrorMessage.set('');
    this.name.set(selectedTour.name);
    this.description.set(selectedTour.description);
    this.from.set(selectedTour.from);
    this.to.set(selectedTour.to);
    this.transportType.set(selectedTour.transportType);
    this.plannedDate.set(selectedTour.plannedDate || new Date().toISOString().slice(0, 10));
    this.isEditTourModalOpen.set(true);
  }

  closeEditTourModal(): void {
    this.tourErrorMessage.set('');
    this.isEditTourModalOpen.set(false);
  }

  saveTour(): void {
    this.tourErrorMessage.set('');

    const selectedTour = this.tour();

    if (!selectedTour) {
      this.router.navigate(['/tours']);
      return;
    }

    const updatedName = this.name().trim();
    const updatedFrom = this.from().trim();
    const updatedTo = this.to().trim();
    const updatedDate = this.plannedDate();

    if (!updatedName || !updatedFrom || !updatedTo || !updatedDate) {
      this.tourErrorMessage.set('Name, date, from and destination are required.');
      return;
    }

    const updatedTour: Tour = {
      ...selectedTour,
      name: updatedName,
      description: this.description().trim(),
      from: updatedFrom,
      to: updatedTo,
      transportType: this.transportType(),
      plannedDate: updatedDate,
    };

    this.tourService.updateTour(updatedTour);
    this.closeEditTourModal();
  }

  openNewLogModal(): void {
    this.resetLogForm();
    this.isLogModalOpen.set(true);
  }

  closeLogModal(): void {
    this.logErrorMessage.set('');
    this.isLogModalOpen.set(false);
    this.resetLogForm();
  }

  saveLog(): void {
    this.logErrorMessage.set('');

    const selectedTour = this.tour();
    const user = this.currentUser();

    if (!selectedTour || !user) {
      this.router.navigate(['/login']);
      return;
    }

    if (!this.logDate() || !this.logComment().trim()) {
      this.logErrorMessage.set('Date and comment are required.');
      return;
    }

    if (this.logDifficulty() < 1 || this.logDifficulty() > 5) {
      this.logErrorMessage.set('Difficulty must be between 1 and 5.');
      return;
    }

    if (this.logRating() < 1 || this.logRating() > 5) {
      this.logErrorMessage.set('Rating must be between 1 and 5.');
      return;
    }

    if (this.logDistance() <= 0 || this.logTime() <= 0) {
      this.logErrorMessage.set('Distance and time must be greater than 0.');
      return;
    }

    const editingId = this.editingLogId();

    if (editingId) {
      const updatedLog: TourLog = {
        id: editingId,
        tourId: selectedTour.id,
        userId: user.id,
        date: this.logDate(),
        comment: this.logComment().trim(),
        difficulty: this.logDifficulty(),
        totalDistance: this.logDistance(),
        totalTime: this.logTime(),
        rating: this.logRating(),
      };

      this.tourService.updateTourLog(updatedLog);
      this.closeLogModal();
      return;
    }

    this.tourService.addTourLog({
      tourId: selectedTour.id,
      userId: user.id,
      date: this.logDate(),
      comment: this.logComment().trim(),
      difficulty: this.logDifficulty(),
      totalDistance: this.logDistance(),
      totalTime: this.logTime(),
      rating: this.logRating(),
    });

    this.closeLogModal();
  }

  editLog(log: TourLog): void {
    this.editingLogId.set(log.id);
    this.logDate.set(log.date);
    this.logComment.set(log.comment);
    this.logDifficulty.set(log.difficulty);
    this.logDistance.set(log.totalDistance);
    this.logTime.set(log.totalTime);
    this.logRating.set(log.rating);
    this.logErrorMessage.set('');
    this.isLogModalOpen.set(true);
  }

  deleteLog(logId: string): void {
    const confirmed = confirm('Do you really want to delete this tour log?');

    if (!confirmed) {
      return;
    }

    this.tourService.deleteTourLog(logId);

    if (this.editingLogId() === logId) {
      this.resetLogForm();
    }
  }

  setTransportType(value: string): void {
    this.transportType.set(value as TransportType);
  }

  formatDate(date: string | undefined): string {
    if (!date) {
      return 'No date';
    }

    return new Intl.DateTimeFormat('en', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    }).format(new Date(date));
  }

  formatDateTime(date: string): string {
    return new Intl.DateTimeFormat('en', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(date));
  }

  private resetLogForm(): void {
    this.editingLogId.set(null);
    this.logDate.set(new Date().toISOString().slice(0, 16));
    this.logComment.set('');
    this.logDifficulty.set(1);
    this.logDistance.set(5);
    this.logTime.set(30);
    this.logRating.set(5);
  }
}
