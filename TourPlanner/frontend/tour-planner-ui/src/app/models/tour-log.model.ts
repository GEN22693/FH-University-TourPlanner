export interface TourLog {
  id: string;
  tourId: string;
  userId: number;
  date: string;
  comment: string;
  difficulty: number;
  totalDistance: number;
  totalTime: number;
  rating: number;
}
