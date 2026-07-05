import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { TourService } from '../../core/services/tour.service';
import { Tour, TransportType, UpdateTourData } from '../../models/tour.model';
import {
  CreateTourLogData,
  Difficulty,
  TourLog,
  UpdateTourLogData,
} from '../../models/tour-log.model';
import { RouteMap } from '../../shared/components/route-map/route-map';
import { Navbar } from '../../shared/components/navbar/navbar';

@Component({
  selector: 'app-tour-detail',
  imports: [FormsModule, RouterLink, RouteMap, Navbar],
  templateUrl: './tour-detail.html',
})
export class TourDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly tourService = inject(TourService);
  private readonly destroyRef = inject(DestroyRef);

  readonly tourId = signal<number | null>(this.readTourId());

  readonly tour = signal<Tour | null>(null);
  readonly logs = signal<TourLog[]>([]);

  readonly isLoadingTour = signal(false);
  readonly isLoadingLogs = signal(false);
  readonly isSavingTour = signal(false);
  readonly isSavingLog = signal(false);

  readonly pageErrorMessage = signal('');

  readonly isEditTourModalOpen = signal(false);
  readonly isLogModalOpen = signal(false);

  readonly name = signal('');
  readonly description = signal('');
  readonly from = signal('');
  readonly to = signal('');
  readonly transportType = signal<TransportType>('Bike');
  readonly tourErrorMessage = signal('');

  readonly logDate = signal(new Date().toISOString().slice(0, 16));
  readonly logComment = signal('');
  readonly logDifficulty = signal<Difficulty>('Easy');
  readonly logDistance = signal(5);
  readonly logTimeInMinutes = signal(30);
  readonly logRating = signal(5);
  readonly logErrorMessage = signal('');
  readonly editingLogId = signal<number | null>(null);

  readonly transportTypes: TransportType[] = ['Bike', 'Hike', 'Running', 'Vacation'];
  readonly difficulties: Difficulty[] = ['Easy', 'Medium', 'Hard'];

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

  readonly totalLoggedTimeInMinutes = computed(() =>
    this.logs().reduce((total, log) => total + this.timeSpanToMinutes(log.totalTime), 0),
  );

  ngOnInit(): void {
    const id = this.tourId();

    if (!id) {
      this.pageErrorMessage.set('Invalid tour id.');
      return;
    }

    this.loadTour(id);
    this.loadLogs(id);
  }

  loadTour(id: number): void {
    this.pageErrorMessage.set('');
    this.isLoadingTour.set(true);

    this.tourService
      .getTourById(id)
      .pipe(
        finalize(() => this.isLoadingTour.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (tour) => {
          this.tour.set(tour);
          this.fillTourForm(tour);
        },
        error: (error: unknown) => {
          if (this.redirectIfUnauthorized(error)) {
            return;
          }

          this.pageErrorMessage.set(this.getErrorMessage(error, 'Tour could not be loaded.'));
        },
      });
  }

  loadLogs(tourId: number): void {
    this.isLoadingLogs.set(true);

    this.tourService
      .getLogsByTour(tourId)
      .pipe(
        finalize(() => this.isLoadingLogs.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (logs) => {
          this.logs.set(logs);
        },
        error: (error: unknown) => {
          if (this.redirectIfUnauthorized(error)) {
            return;
          }

          this.pageErrorMessage.set(this.getErrorMessage(error, 'Logs could not be loaded.'));
        },
      });
  }

  openEditTourModal(): void {
    const selectedTour = this.tour();

    if (!selectedTour) {
      return;
    }

    this.tourErrorMessage.set('');
    this.fillTourForm(selectedTour);
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

    if (!updatedName || !updatedFrom || !updatedTo) {
      this.tourErrorMessage.set('Name, from and destination are required.');
      return;
    }

    const updateData: UpdateTourData = {
      name: updatedName,
      description: this.description().trim(),
      from: updatedFrom,
      to: updatedTo,
      transportType: this.transportType(),
    };

    this.isSavingTour.set(true);

    this.tourService
      .updateTour(selectedTour.id, updateData)
      .pipe(
        finalize(() => this.isSavingTour.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (updatedTour) => {
          this.tour.set(updatedTour);
          this.closeEditTourModal();
        },
        error: (error: unknown) => {
          if (this.redirectIfUnauthorized(error)) {
            return;
          }

          this.tourErrorMessage.set(this.getErrorMessage(error, 'Tour could not be updated.'));
        },
      });
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

    if (!selectedTour) {
      this.router.navigate(['/tours']);
      return;
    }

    if (!this.logDate() || !this.logComment().trim()) {
      this.logErrorMessage.set('Date and comment are required.');
      return;
    }

    if (this.logRating() < 1 || this.logRating() > 5) {
      this.logErrorMessage.set('Rating must be between 1 and 5.');
      return;
    }

    if (this.logDistance() <= 0 || this.logTimeInMinutes() <= 0) {
      this.logErrorMessage.set('Distance and time must be greater than 0.');
      return;
    }

    const logData: CreateTourLogData | UpdateTourLogData = {
      dateTime: this.logDate(),
      comment: this.logComment().trim(),
      difficulty: this.logDifficulty(),
      totalDistance: this.logDistance(),
      totalTime: this.minutesToTimeSpan(this.logTimeInMinutes()),
      rating: this.logRating(),
    };

    const editingId = this.editingLogId();

    this.isSavingLog.set(true);

    if (editingId) {
      this.tourService
        .updateTourLog(selectedTour.id, editingId, logData)
        .pipe(
          finalize(() => this.isSavingLog.set(false)),
          takeUntilDestroyed(this.destroyRef),
        )
        .subscribe({
          next: (updatedLog) => {
            this.logs.update((logs) =>
              logs.map((log) => (log.id === updatedLog.id ? updatedLog : log)),
            );
            this.loadTour(selectedTour.id);
            this.closeLogModal();
          },
          error: (error: unknown) => {
            if (this.redirectIfUnauthorized(error)) {
              return;
            }

            this.logErrorMessage.set(this.getErrorMessage(error, 'Log could not be updated.'));
          },
        });

      return;
    }

    this.tourService
      .createTourLog(selectedTour.id, logData)
      .pipe(
        finalize(() => this.isSavingLog.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (createdLog) => {
          this.logs.update((logs) => [...logs, createdLog]);
          this.loadTour(selectedTour.id);
          this.closeLogModal();
        },
        error: (error: unknown) => {
          if (this.redirectIfUnauthorized(error)) {
            return;
          }

          this.logErrorMessage.set(this.getErrorMessage(error, 'Log could not be created.'));
        },
      });
  }

  editLog(log: TourLog): void {
    this.editingLogId.set(log.id);
    this.logDate.set(this.toDateTimeInputValue(log.dateTime));
    this.logComment.set(log.comment);
    this.logDifficulty.set(log.difficulty);
    this.logDistance.set(log.totalDistance);
    this.logTimeInMinutes.set(this.timeSpanToMinutes(log.totalTime));
    this.logRating.set(log.rating);
    this.logErrorMessage.set('');
    this.isLogModalOpen.set(true);
  }

  deleteLog(logId: number): void {
    const selectedTour = this.tour();

    if (!selectedTour) {
      return;
    }

    const confirmed = confirm('Do you really want to delete this tour log?');

    if (!confirmed) {
      return;
    }

    this.tourService
      .deleteTourLog(selectedTour.id, logId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.logs.update((logs) => logs.filter((log) => log.id !== logId));
          this.loadTour(selectedTour.id);

          if (this.editingLogId() === logId) {
            this.resetLogForm();
          }
        },
        error: (error: unknown) => {
          if (this.redirectIfUnauthorized(error)) {
            return;
          }

          this.pageErrorMessage.set(this.getErrorMessage(error, 'Log could not be deleted.'));
        },
      });
  }

  setTransportType(value: string): void {
    const selectedType = value as TransportType;

    if (this.transportTypes.includes(selectedType)) {
      this.transportType.set(selectedType);
    }
  }

  setDifficulty(value: string): void {
    const selectedDifficulty = value as Difficulty;

    if (this.difficulties.includes(selectedDifficulty)) {
      this.logDifficulty.set(selectedDifficulty);
    }
  }

  getTransportLabel(type: TransportType): string {
    if (type === 'Bike') {
      return 'Cycling';
    }

    if (type === 'Hike') {
      return 'Hiking';
    }

    if (type === 'Running') {
      return 'Running';
    }

    return 'Vacation';
  }

  formatTourDistance(distanceInMeters: number): string {
    const distanceInKilometers = distanceInMeters / 1000;

    return `${Math.round(distanceInKilometers * 10) / 10} km`;
  }

  formatLogDistance(distanceInKilometers: number): string {
    return `${Math.round(distanceInKilometers * 10) / 10} km`;
  }

  formatDuration(timeSpan: string): string {
    const totalMinutes = this.timeSpanToMinutes(timeSpan);

    if (totalMinutes <= 0) {
      return '0 min';
    }

    return this.formatMinutes(totalMinutes);
  }

  formatMinutes(totalMinutes: number): string {
    const roundedMinutes = Math.round(totalMinutes);
    const hours = Math.floor(roundedMinutes / 60);
    const minutes = roundedMinutes % 60;

    if (hours === 0) {
      return `${minutes} min`;
    }

    if (minutes === 0) {
      return `${hours} h`;
    }

    return `${hours} h ${minutes} min`;
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

  private fillTourForm(tour: Tour): void {
    this.name.set(tour.name);
    this.description.set(tour.description);
    this.from.set(tour.from);
    this.to.set(tour.to);
    this.transportType.set(tour.transportType);
  }

  private resetLogForm(): void {
    this.editingLogId.set(null);
    this.logDate.set(new Date().toISOString().slice(0, 16));
    this.logComment.set('');
    this.logDifficulty.set('Easy');
    this.logDistance.set(5);
    this.logTimeInMinutes.set(30);
    this.logRating.set(5);
  }

  private minutesToTimeSpan(totalMinutes: number): string {
    const roundedMinutes = Math.max(0, Math.round(totalMinutes));

    const days = Math.floor(roundedMinutes / 1440);
    const remainingMinutes = roundedMinutes % 1440;

    const hours = Math.floor(remainingMinutes / 60);
    const minutes = remainingMinutes % 60;

    const hoursText = hours.toString().padStart(2, '0');
    const minutesText = minutes.toString().padStart(2, '0');

    if (days > 0) {
      return `${days}.${hoursText}:${minutesText}:00`;
    }

    return `${hoursText}:${minutesText}:00`;
  }

  private timeSpanToMinutes(timeSpan: string): number {
    if (!timeSpan) {
      return 0;
    }

    const match = /^(?:(\d+)\.)?(\d{1,2}):(\d{2}):(\d{2}(?:\.\d+)?)$/.exec(timeSpan);

    if (!match) {
      return 0;
    }

    const days = Number(match[1] ?? 0);
    const hours = Number(match[2]) || 0;
    const minutes = Number(match[3]) || 0;
    const seconds = Number.parseFloat(match[4]) || 0;

    return days * 24 * 60 + hours * 60 + minutes + Math.round(seconds / 60);
  }

  private toDateTimeInputValue(dateTime: string): string {
    const date = new Date(dateTime);

    if (Number.isNaN(date.getTime())) {
      return dateTime.slice(0, 16);
    }

    const timezoneOffsetInMilliseconds = date.getTimezoneOffset() * 60_000;
    const localDate = new Date(date.getTime() - timezoneOffsetInMilliseconds);

    return localDate.toISOString().slice(0, 16);
  }

  private readTourId(): number | null {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Number(rawId);

    if (!Number.isInteger(id) || id <= 0) {
      return null;
    }

    return id;
  }

  private getErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'Backend is not reachable.';
      }

      const backendMessage = error.error?.message;

      if (typeof backendMessage === 'string' && backendMessage.trim()) {
        return backendMessage;
      }
    }

    return fallbackMessage;
  }

  private redirectIfUnauthorized(error: unknown): boolean {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      this.router.navigate(['/login']);
      return true;
    }

    return false;
  }
}
