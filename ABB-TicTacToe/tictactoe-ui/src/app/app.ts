import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GameService, GameState } from './game.service';

@Component({
  imports: [CommonModule, FormsModule],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  gameState: GameState | null = null;
  gameId: string | null = null;
  selectedMode = 'TwoPlayer';
  gameStarted = false;

  constructor(private readonly gameService: GameService) {}

  startGame(): void {
    this.gameService.startGame(this.selectedMode).subscribe((state) => {
      this.gameId = state.gameId;
      this.gameState = state;
      this.gameStarted = true;
    });
  }

  makeMove(cellIndex: number): void {
    if (!this.gameId || !this.gameState) {
      return;
    }

    if (this.gameState.status !== 'InProgress') {
      return;
    }

    if (this.gameState.board[cellIndex]) {
      return;
    }

    this.gameService
      .makeMove(this.gameId, cellIndex, this.gameState.currentTurn)
      .subscribe((state) => {
        this.gameState = state;
      });
  }

  undoMove(): void {
    if (!this.gameId) {
      return;
    }

    this.gameService.undoMove(this.gameId).subscribe((state) => {
      this.gameState = state;
    });
  }

  resetGame(): void {
    if (!this.gameId) {
      return;
    }

    this.gameService.resetGame(this.gameId).subscribe((state) => {
      this.gameState = state;
    });
  }
}
