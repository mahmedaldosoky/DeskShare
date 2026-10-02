import type { Booking, CreateBookingRequest } from '../types/models';
import { http } from './httpClient';

export const bookingsApi = {
  getMine: () => http.get<Booking[]>('/api/bookings/mine'),
  getAllOnDate: (date: string) => http.get<Booking[]>(`/api/bookings?date=${date}`),
  create: (request: CreateBookingRequest) => http.post<Booking>('/api/bookings', request),
  cancel: (id: string) => http.delete(`/api/bookings/${id}`),
};
