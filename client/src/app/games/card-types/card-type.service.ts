import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CardType {
  id: number;
  name: string;
  description: string | null;
}

export interface CardTypeRequest {
  name: string;
  description: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class CardTypeService {
  private readonly http = inject(HttpClient);

  private gameUrl(gameId: number): string {
    return `/api/games/${gameId}/card-types`;
  }

  list(gameId: number): Observable<CardType[]> {
    return this.http.get<CardType[]>(this.gameUrl(gameId));
  }

  getById(cardTypeId: number): Observable<CardType> {
    return this.http.get<CardType>(`/api/card-types/${cardTypeId}`);
  }

  create(gameId: number, request: CardTypeRequest): Observable<CardType> {
    return this.http.post<CardType>(this.gameUrl(gameId), request);
  }

  update(cardTypeId: number, request: CardTypeRequest): Observable<CardType> {
    return this.http.put<CardType>(`/api/card-types/${cardTypeId}`, request);
  }

  delete(cardTypeId: number): Observable<void> {
    return this.http.delete<void>(`/api/card-types/${cardTypeId}`);
  }
}