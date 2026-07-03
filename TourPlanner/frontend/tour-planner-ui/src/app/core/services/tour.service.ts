import { Injectable, signal } from '@angular/core';
import { Tour } from '../../models/tour.model';
import { TourLog } from '../../models/tour-log.model';

@Injectable({
  providedIn: 'root',
})
export class TourService {
  private readonly toursKey = 'tourplanner_tours';
  private readonly tourLogsKey = 'tourplanner_tour_logs';

  private readonly toursSignal = signal<Tour[]>(this.loadTours());
  private readonly tourLogsSignal = signal<TourLog[]>(this.loadTourLogs());

  readonly tours = this.toursSignal.asReadonly();
  readonly tourLogs = this.tourLogsSignal.asReadonly();

  getToursByUser(userId: number): Tour[] {
    return this.toursSignal().filter((tour) => tour.userId === userId);
  }

  getTourById(tourId: string): Tour | undefined {
    return this.toursSignal().find((tour) => tour.id === tourId);
  }

  addTour(tour: Omit<Tour, 'id' | 'createdAt'>): void {
    const newTour: Tour = {
      ...tour,
      id: crypto.randomUUID(),
      createdAt: new Date().toISOString(),
    };

    const updatedTours = [...this.toursSignal(), newTour];
    this.saveTours(updatedTours);
  }

  updateTour(updatedTour: Tour): void {
    const updatedTours = this.toursSignal().map((tour) =>
      tour.id === updatedTour.id ? updatedTour : tour,
    );

    this.saveTours(updatedTours);
  }

  deleteTour(tourId: string): void {
    const updatedTours = this.toursSignal().filter((tour) => tour.id !== tourId);
    const updatedLogs = this.tourLogsSignal().filter((log) => log.tourId !== tourId);

    this.saveTours(updatedTours);
    this.saveTourLogs(updatedLogs);
  }

  getLogsByTour(tourId: string): TourLog[] {
    return this.tourLogsSignal().filter((log) => log.tourId === tourId);
  }

  addTourLog(log: Omit<TourLog, 'id'>): void {
    const newLog: TourLog = {
      ...log,
      id: crypto.randomUUID(),
    };

    const updatedLogs = [...this.tourLogsSignal(), newLog];
    this.saveTourLogs(updatedLogs);
  }

  updateTourLog(updatedLog: TourLog): void {
    const updatedLogs = this.tourLogsSignal().map((log) =>
      log.id === updatedLog.id ? updatedLog : log,
    );

    this.saveTourLogs(updatedLogs);
  }

  deleteTourLog(logId: string): void {
    const updatedLogs = this.tourLogsSignal().filter((log) => log.id !== logId);
    this.saveTourLogs(updatedLogs);
  }

  private saveTours(tours: Tour[]): void {
    localStorage.setItem(this.toursKey, JSON.stringify(tours));
    this.toursSignal.set(tours);
  }

  private saveTourLogs(logs: TourLog[]): void {
    localStorage.setItem(this.tourLogsKey, JSON.stringify(logs));
    this.tourLogsSignal.set(logs);
  }

  private loadTours(): Tour[] {
    const rawTours = localStorage.getItem(this.toursKey);

    if (!rawTours) {
      return [];
    }

    return JSON.parse(rawTours) as Tour[];
  }

  private loadTourLogs(): TourLog[] {
    const rawLogs = localStorage.getItem(this.tourLogsKey);

    if (!rawLogs) {
      return [];
    }

    return JSON.parse(rawLogs) as TourLog[];
  }
}
