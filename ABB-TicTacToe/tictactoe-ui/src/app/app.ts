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
  // Signals (not plain fields) are required here: this app runs zoneless (no zone.js),
  // so a plain field mutated inside an async subscribe() callback would never
  // trigger change detection and the view would go stale.
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

  // Games always start on X's turn, so move history parity tells us who played each move.
  movePlayer(moveNumber: number): string {
    return moveNumber % 2 === 0 ? 'X' : 'O';
  }

  // Converts a 0-8 board index into a 1-based row for display in the move history table.
  moveRow(cellIndex: number): number {
    return Math.floor(cellIndex / 3) + 1;
  }

  // Converts a 0-8 board index into a 1-based column for display in the move history table.
  moveCol(cellIndex: number): number {
    return (cellIndex % 3) + 1;
  }

  resetScoreboard(): void {
    const id = this.gameId();
    const state = this.gameState();
    if (!id || !state) {
      return;
    }

    // Only the scoreboard changes on the server; merge it into the existing state
    // rather than replacing gameState, since the reset endpoint doesn't return board/turn data.
    this.gameService.resetScoreboard(id).subscribe((scoreboard) => {
      this.gameState.set({ ...state, scoreboard });
    });
  }
}
