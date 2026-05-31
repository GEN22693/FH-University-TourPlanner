export interface TourLog {
  id: string;
  tourId: string;
  userId: string;
  date: string;
  comment: string;
  difficulty: number;
  totalDistance: number;
  totalTime: number;
  rating: number;
}
