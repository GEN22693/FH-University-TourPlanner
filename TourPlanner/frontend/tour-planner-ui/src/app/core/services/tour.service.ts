import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../api/api.config';
import {
  CreateTourData,
  Tour,
  TourImportExportData,
  TourStatistics,
  UpdateTourData,
} from '../../models/tour.model';
import {
  CreateTourLogData,
  TourLog,
  UpdateTourLogData,
} from '../../models/tour-log.model';

@Injectable({
  providedIn: 'root',
})
export class TourService {
  private readonly http = inject(HttpClient);

  getTours(): Observable<Tour[]> {
    return this.http.get<Tour[]>(`${API_BASE_URL}/tours`);
  }

  searchTours(query: string): Observable<Tour[]> {
    return this.http.get<Tour[]>(`${API_BASE_URL}/tours/search`, {
      params: { query },
    });
  }

  getStatistics(): Observable<TourStatistics> {
    return this.http.get<TourStatistics>(`${API_BASE_URL}/tours/statistics`);
  }

  exportTours(): Observable<Blob> {
    return this.http.get(`${API_BASE_URL}/tours/export`, {
      responseType: 'blob',
    });
  }

  importTours(data: TourImportExportData[]): Observable<Tour[]> {
    return this.http.post<Tour[]>(`${API_BASE_URL}/tours/import`, data);
  }

  getTourById(id: number): Observable<Tour> {
    return this.http.get<Tour>(`${API_BASE_URL}/tours/${id}`);
  }

  createTour(data: CreateTourData): Observable<Tour> {
    return this.http.post<Tour>(`${API_BASE_URL}/tours`, data);
  }

  updateTour(id: number, data: UpdateTourData): Observable<Tour> {
    return this.http.put<Tour>(`${API_BASE_URL}/tours/${id}`, data);
  }

  deleteTour(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/tours/${id}`);
  }

  getLogsByTour(tourId: number): Observable<TourLog[]> {
    return this.http.get<TourLog[]>(`${API_BASE_URL}/tours/${tourId}/logs`);
  }

  createTourLog(tourId: number, data: CreateTourLogData): Observable<TourLog> {
    return this.http.post<TourLog>(`${API_BASE_URL}/tours/${tourId}/logs`, data);
  }

  updateTourLog(
    tourId: number,
    logId: number,
    data: UpdateTourLogData,
  ): Observable<TourLog> {
    return this.http.put<TourLog>(
      `${API_BASE_URL}/tours/${tourId}/logs/${logId}`,
      data,
    );
  }

  deleteTourLog(tourId: number, logId: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/tours/${tourId}/logs/${logId}`);
  }
}
