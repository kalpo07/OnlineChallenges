# Tic Tac Toe — Architecture & Design

> This document was written before implementation began.

---

## Overview

A browser-based Tic Tac Toe game with two modes: two-player (same device) and player vs computer. The application follows a **backend-as-source-of-truth** architecture — all game logic, state management, and validation live exclusively in the .NET Web API. The Angular frontend is a thin rendering surface that sends user actions and displays what the backend returns.

---

## Architecture Diagram

┌─────────────────────────────────────────┐
│ Angular Frontend │
│ (Rendering only — no game logic here) │
│ │
│ GameBoardComponent │
│ ScoreboardComponent │
│ GameService (HTTP calls only) │
└──────────────┬──────────────────────────┘
│ REST API (HTTP/JSON)
│ Every response returns full GameState
▼
┌─────────────────────────────────────────┐
│ .NET Web API (C#) │
│ │
│ Controllers/GameController.cs │
│ Services/GameService.cs │
│ Services/ComputerPlayerService.cs │
│ Services/ScoreboardService.cs │
│ Models/Game.cs │
│ Models/GameStateResponse.cs │
└──────────────┬──────────────────────────┘
│
▼
┌─────────────────────────────────────────┐
│ In-Memory Storage │
│ Dictionary<Guid, Game> │
│ Scoreboard singleton │
└─────────────────────────────────────────┘


---

## Key Design Decisions

**1. Backend is the single source of truth**
The frontend never calculates win conditions, validates moves, or determines whose turn it is. It only renders what the API returns. This keeps the frontend replaceable and the logic testable independently.

**2. Every API response returns full game state**
Rather than returning partial updates, every endpoint returns a complete `GameStateResponse`. The frontend never needs to merge or track state between calls.

**3. Undo behaviour differs by mode**
In two-player mode, undo removes one move. In computer mode, undo removes two moves (the player's move and the computer's response) — otherwise the player would face the computer's move again immediately. Undo is disabled after the game ends to preserve score integrity.

**4. Computer AI uses greedy heuristic**
Priority order: Win → Block opponent's win → Take centre → Take a corner → Take any available cell. This makes the computer a reasonable opponent without being unbeatable.

---

## API Contract

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/game/start` | Create new game session |
| GET | `/api/game/{id}` | Get current game state |
| POST | `/api/game/{id}/move` | Submit a player move |
| POST | `/api/game/{id}/undo` | Undo last move |
| POST | `/api/game/{id}/reset` | Reset board, keep scores |
| GET | `/api/game/{id}/scoreboard` | Get win/draw counts |
| DELETE | `/api/game/{id}` | End session |

---

## Test Scenarios (defined before coding)

1. New game starts with empty board, X goes first
2. Valid move updates board and switches turn
3. Move on occupied cell is rejected
4. Move on completed game is rejected
5. Horizontal win detected correctly (all 3 rows)
6. Vertical win detected correctly (all 3 columns)
7. Diagonal win detected correctly (both diagonals)
8. Draw detected when board is full with no winner
9. Undo in two-player mode removes one move
10. Undo in computer mode removes two moves (player + computer)
11. Undo on first move of game is rejected
12. Undo after game ends is rejected
13. Computer always blocks an immediate player win
14. Scoreboard increments correctly on win and draw
15. Reset clears board but preserves scoreboard

---

## Backend File Structure

TicTacToe.API/
├── Controllers/
│ └── GameController.cs
├── Services/
│ ├── GameService.cs
│ ├── ComputerPlayerService.cs
│ └── ScoreboardService.cs
├── Models/
│ ├── Game.cs
│ └── GameStateResponse.cs
└── Program.cs


---

*This document was written before implementation. Code structure follows from these decisions.*