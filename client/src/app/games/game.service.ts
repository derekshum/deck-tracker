import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface GameSummary {
  id: number;
  name: string;
  description: string | null;
  totalDecks: number;
  winRate: number;
}

export interface CreateGameRequest {
  name: string;
  description: string | null;
}

export interface UpdateGameRequest {
  name: string;
  description: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/games';

  list(): Observable<GameSummary[]> {
    return this.http.get<GameSummary[]>(this.baseUrl)
  }

  getById(id: number): Observable<GameSummary> {
    return this.http.get<GameSummary>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateGameRequest): Observable<GameSummary> {
    return this.http.post<GameSummary>(this.baseUrl, request);
  }

  update(id: number, request: UpdateGameRequest): Observable<GameSummary> {
    return this.http.put<GameSummary>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
