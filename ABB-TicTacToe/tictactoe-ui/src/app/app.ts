import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameService, GameState } from './game.service';

@Component({
  imports: [CommonModule],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  readonly gameState = signal<GameState | null>(null);
  readonly gameId = signal<string | null>(null);
  readonly selectedMode = signal('TwoPlayer');
  readonly gameStarted = signal(false);

  constructor(private readonly gameService: GameService) {}

  startGame(): void {
    this.gameService.startGame(this.selectedMode()).subscribe((state) => {
      this.gameId.set(state.id);
      this.gameState.set(state);
      this.gameStarted.set(true);
    });
  }

  makeMove(cellIndex: number): void {
    const id = this.gameId();
    const state = this.gameState();

    if (!id || !state) {
      return;
    }

    if (state.status !== 'InProgress') {
      return;
    }

    if (state.board[cellIndex]) {
      return;
    }

    this.gameService.makeMove(id, cellIndex, state.currentTurn).subscribe((newState) => {
      this.gameState.set(newState);
    });
  }

  undoMove(): void {
    const id = this.gameId();
    if (!id) {
      return;
    }

    this.gameService.undoMove(id).subscribe((state) => {
      this.gameState.set(state);
    });
  }

  resetGame(): void {
    const id = this.gameId();
    if (!id) {
      return;
    }

    this.gameService.resetGame(id).subscribe((state) => {
      this.gameState.set(state);
    });
  }

  goBack(): void {
    this.gameStarted.set(false);
    this.gameState.set(null);
    this.gameId.set(null);
  }

  movePlayer(moveNumber: number): string {
    return moveNumber % 2 === 0 ? 'X' : 'O';
  }

  moveRow(cellIndex: number): number {
    return Math.floor(cellIndex / 3) + 1;
  }

  moveCol(cellIndex: number): number {
    return (cellIndex % 3) + 1;
  }

  resetScoreboard(): void {
    const id = this.gameId();
    const state = this.gameState();
    if (!id || !state) {
      return;
    }

    this.gameService.resetScoreboard(id).subscribe((scoreboard) => {
      this.gameState.set({ ...state, scoreboard });
    });
  }
}
