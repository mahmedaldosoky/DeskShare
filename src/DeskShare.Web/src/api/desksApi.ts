import type { Desk, SaveDeskRequest } from '../types/models';
import { http } from './httpClient';

export const desksApi = {
  getAll: () => http.get<Desk[]>('/api/desks'),
  getAvailable: (date: string) => http.get<Desk[]>(`/api/desks/available?date=${date}`),
  create: (request: SaveDeskRequest) => http.post<Desk>('/api/desks', request),
  update: (id: string, request: SaveDeskRequest) => http.put<Desk>(`/api/desks/${id}`, request),
  remove: (id: string) => http.delete(`/api/desks/${id}`),
};
