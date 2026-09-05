import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameState {
  gameId: string;
  board: string[];
  currentTurn: string;
  status: string;
  winner: string | null;
  winningCells: number[];
  canUndo: boolean;
  scoreboard: Scoreboard;
}

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly baseUrl = 'http://localhost:5000/api/game';

  constructor(private readonly http: HttpClient) {}

  startGame(mode: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/start`, { mode });
  }

  getGame(id: string): Observable<GameState> {
    return this.http.get<GameState>(`${this.baseUrl}/${id}`);
  }

  // TODO: No loading indicator between move submission and API response. Would add disabled board state during API call.
  makeMove(id: string, cellIndex: number, player: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/${id}/move`, { cellIndex, player });
  }

  undoMove(id: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/${id}/undo`, {});
  }

  resetGame(id: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/${id}/reset`, {});
  }
}
