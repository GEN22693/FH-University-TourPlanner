export type TransportType = 'Bike' | 'Hike' | 'Run' | 'Vacation';

export interface Tour {
  id: string;
  userId: number;
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
  plannedDate: string;
  distance: number;
  estimatedTime: number;
  routeInfo: string;
  createdAt: string;
}
