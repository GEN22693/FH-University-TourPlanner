import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { TourService } from '../../core/services/tour.service';
import {
  CreateTourData,
  Tour,
  TourImportExportData,
  TourStatistics,
  TransportType,
} from '../../models/tour.model';
import { Navbar } from '../../shared/components/navbar/navbar';

@Component({
  selector: 'app-tourlist',
  imports: [FormsModule, RouterLink, Navbar],
  templateUrl: './tourlist.html',
})
export class Tourlist implements OnInit {
  private readonly tourService = inject(TourService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly tours = signal<Tour[]>([]);
  readonly statistics = signal<TourStatistics | null>(null);

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isSearching = signal(false);
  readonly isExporting = signal(false);
  readonly isImporting = signal(false);
  readonly isCreateModalOpen = signal(false);

  readonly name = signal('');
  readonly description = signal('');
  readonly from = signal('');
  readonly to = signal('');
  readonly transportType = signal<TransportType>('Bike');

  readonly searchText = signal('');
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly transportTypes: TransportType[] = ['Bike', 'Hike', 'Running', 'Vacation'];

  readonly displayedTours = computed(() => {
    return [...this.tours()].sort((a, b) => a.name.localeCompare(b.name));
  });

  readonly totalDistanceInMeters = computed(() => {
    return this.statistics()?.totalDistance ?? this.tours().reduce((sum, tour) => sum + tour.distance, 0);
  });

  readonly totalEstimatedTimeInMinutes = computed(() => {
    const statistics = this.statistics();

    if (statistics) {
      return this.timeSpanToMinutes(statistics.totalEstimatedTime);
    }

    return this.tours().reduce((sum, tour) => sum + this.timeSpanToMinutes(tour.estimatedTime), 0);
  });

  ngOnInit(): void {
    this.loadTours();
    this.loadStatistics();
  }

  loadTours(): void {
    this.errorMessage.set('');
    this.isLoading.set(true);

    this.tourService
      .getTours()
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (tours) => {
          this.tours.set(tours);
        },
        error: (error: unknown) => {
          if (this.redirectIfUnauthorized(error)) {
            return;
          }

          this.errorMessage.set(this.getErrorMessage(error, 'Tours could not be loaded.'));
        },
      });
  }

  loadStatistics(): void {
    this.tourService
      .getStatistics()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (statistics) => this.statistics.set(statistics),
        error: () => this.statistics.set(null),
      });
  }

  searchTours(): void {
    const query = this.searchText().trim();

    if (!query) {
      this.loadTours();
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.isSearching.set(true);

    this.tourService
      .searchTours(query)
      .pipe(
        finalize(() => this.isSearching.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (tours) => this.tours.set(tours),
        error: (error: unknown) => {
          if (this.redirectIfUnauthorized(error)) {
            return;
          }

          this.errorMessage.set(this.getErrorMessage(error, 'Search failed.'));
        },
      });
  }

  clearSearch(): void {
    this.searchText.set('');
    this.loadTours();
  }

  openCreateModal(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
    this.isCreateModalOpen.set(true);
  }

  closeCreateModal(): void {
    this.errorMessage.set('');
    this.isCreateModalOpen.set(false);
  }

  addTour(): void {
    this.errorMessage.set('');
    this.successMessage.set('');

    const tourName = this.name().trim();
    const tourFrom = this.from().trim();
    const tourTo = this.to().trim();

    if (!tourName || !tourFrom || !tourTo) {
      this.errorMessage.set('Name, from and destination are required.');
      return;
    }

    const createData: CreateTourData = {
      name: tourName,
      description: this.description().trim(),
      from: tourFrom,
      to: tourTo,
      transportType: this.transportType(),
    };

    this.isSaving.set(true);

    this.tourService
      .createTour(createData)
      .pipe(
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (createdTour) => {
          this.tours.update((tours) => [...tours, createdTour]);
          this.loadStatistics();
          this.resetForm();
          this.closeCreateModal();
          this.successMessage.set('Tour created successfully.');
        },
        error: (error: unknown) => {
          if (this.redirectIfUnauthorized(error)) {
            return;
          }

          this.errorMessage.set(this.getErrorMessage(error, 'Tour could not be created.'));
        },
      });
  }

  deleteTour(tourId: number): void {
    const confirmed = confirm('Do you really want to delete this tour?');

    if (!confirmed) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');

    this.tourService
      .deleteTour(tourId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.tours.update((tours) => tours.filter((tour) => tour.id !== tourId));
          this.loadStatistics();
          this.successMessage.set('Tour deleted successfully.');
        },
        error: (error: unknown) => {
          if (this.redirectIfUnauthorized(error)) {
            return;
          }

          this.errorMessage.set(this.getErrorMessage(error, 'Tour could not be deleted.'));
        },
      });
  }

  exportTours(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
    this.isExporting.set(true);

    this.tourService
      .exportTours()
      .pipe(
        finalize(() => this.isExporting.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (file) => {
          this.downloadFile(file, 'tourplanner-export.json');
          this.successMessage.set('Tours exported successfully.');
        },
        error: (error: unknown) => {
          if (this.redirectIfUnauthorized(error)) {
            return;
          }

          this.errorMessage.set(this.getErrorMessage(error, 'Export failed.'));
        },
      });
  }

  async importTours(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.isImporting.set(true);

    try {
      const fileText = await file.text();
      const parsedData = JSON.parse(fileText) as unknown;

      if (!Array.isArray(parsedData)) {
        this.errorMessage.set('Import file must contain a JSON array.');
        this.isImporting.set(false);
        input.value = '';
        return;
      }

      this.tourService
        .importTours(parsedData as TourImportExportData[])
        .pipe(
          finalize(() => {
            this.isImporting.set(false);
            input.value = '';
          }),
          takeUntilDestroyed(this.destroyRef),
        )
        .subscribe({
          next: (createdTours) => {
            this.tours.update((tours) => [...tours, ...createdTours]);
            this.loadStatistics();
            this.successMessage.set(`${createdTours.length} tour(s) imported successfully.`);
          },
          error: (error: unknown) => {
            if (this.redirectIfUnauthorized(error)) {
              return;
            }

            this.errorMessage.set(this.getErrorMessage(error, 'Import failed.'));
          },
        });
    } catch {
      this.errorMessage.set('Import file is not valid JSON.');
      this.isImporting.set(false);
      input.value = '';
    }
  }

  setTransportType(value: string): void {
    const selectedType = value as TransportType;

    if (this.transportTypes.includes(selectedType)) {
      this.transportType.set(selectedType);
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

  getRouteInitials(tour: Tour): string {
    const start = tour.from.trim().charAt(0).toUpperCase() || 'A';
    const end = tour.to.trim().charAt(0).toUpperCase() || 'B';

    return `${start}${end}`;
  }

  formatTourDistance(distanceInMeters: number): string {
    const distanceInKilometers = distanceInMeters / 1000;

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

  getChildFriendlinessLabel(value: string): string {
    return value || 'Unknown';
  }

  private resetForm(): void {
    this.name.set('');
    this.description.set('');
    this.from.set('');
    this.to.set('');
    this.transportType.set('Bike');
  }

  private downloadFile(file: Blob, fileName: string): void {
    const url = URL.createObjectURL(file);
    const link = document.createElement('a');

    link.href = url;
    link.download = fileName;
    link.click();

    URL.revokeObjectURL(url);
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
