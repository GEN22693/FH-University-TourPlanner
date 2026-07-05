export type Difficulty = 'Easy' | 'Medium' | 'Hard';

export interface TourLog {
  id: number;
  tourId: number;
  dateTime: string;
  comment: string;
  difficulty: Difficulty;
  totalDistance: number;
  totalTime: string;
  rating: number;
}

export interface CreateTourLogData {
  dateTime: string;
  comment: string;
  difficulty: Difficulty;
  totalDistance: number;
  totalTime: string;
  rating: number;
}

export interface UpdateTourLogData {
  dateTime: string;
  comment: string;
  difficulty: Difficulty;
  totalDistance: number;
  totalTime: string;
  rating: number;
}
