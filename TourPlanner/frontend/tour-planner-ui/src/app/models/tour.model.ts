import { Difficulty } from './tour-log.model';

export type TransportType = 'Bike' | 'Hike' | 'Running' | 'Vacation';

export interface Tour {
  id: number;
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
  distance: number;
  estimatedTime: string;
  routeInformation: string;
  popularity: number;
  childFriendliness: string;
}

export interface CreateTourData {
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
}

export interface UpdateTourData {
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
}

export interface TourStatistics {
  totalTours: number;
  totalLogs: number;
  totalDistance: number;
  totalEstimatedTime: string;
  averageRating: number;
  mostPopularTourName: string;
  bestRatedTourName: string;
}

export interface TourImportExportData {
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
  distance: number;
  estimatedTime: string;
  routeInformation: string;
  popularity: number;
  childFriendliness: string;
  logs: TourLogImportExportData[];
}

export interface TourLogImportExportData {
  dateTime: string;
  comment: string;
  difficulty: Difficulty;
  totalDistance: number;
  totalTime: string;
  rating: number;
}
