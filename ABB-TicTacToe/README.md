# Tic Tac Toe — ABB Assessment

A full-stack Tic Tac Toe application built as part of the ABB Software Development Manager assessment. Demonstrates clean architecture, RESTful API design, Angular frontend with reactive patterns, computer AI opponent, and a full unit test suite.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 22, TypeScript, Signals (zoneless) |
| Backend | .NET 9 Web API, C# |
| Tests | xUnit (.NET) |
| State | In-memory (server-side game state) |

---

## Features

- **Two Player mode** — play locally on the same device
- **vs Computer mode** — AI opponent with smart move strategy (win → block → center → corner → random)
- **Undo move** — step back one move at a time (both modes supported)
- **Move history** — live table showing every move played
- **Scoreboard** — tracks X Wins, O Wins, and Draws across games
- **Reset Scoreboard** — clear the scoreboard without ending the session
- **Back button** — return to mode selection without refreshing

---

## Project Structure

```
ABB-TicTacToe/
├── ARCHITECTURE.md                  # Design decisions and API contract
├── TicTacToe.API/                   # .NET Web API (backend)
│   ├── Controllers/
│   │   └── GameController.cs        # REST endpoints
│   ├── Models/
│   │   └── Game.cs                  # Game state, Board, Player enum
│   ├── Services/
│   │   ├── GameService.cs           # Core game logic
│   │   └── ComputerPlayerService.cs # AI move strategy
│   └── Program.cs                   # DI registration, CORS, JSON config
├── TicTacToe.API.Tests/             # xUnit test project
│   └── GameServiceTests.cs          # 19 unit tests
└── tictactoe-ui/                    # Angular frontend
    └── src/app/
        ├── app.ts                   # Root component (signals-based state)
        ├── app.html                 # Game board, scoreboard, history UI
        └── game.service.ts          # HTTP service for API calls
```

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/) and npm
- [Angular CLI](https://angular.dev/tools/cli): `npm install -g @angular/cli`

---

### 1 — Run the Backend API

```bash
cd ABB-TicTacToe/TicTacToe.API
dotnet run
```

The API starts on **http://localhost:5292**

> Verify it's running: open http://localhost:5292/api/game/health in a browser — you should see `"Healthy"`.

---

### 2 — Run the Frontend

Open a second terminal:

```bash
cd ABB-TicTacToe/tictactoe-ui
npm install
ng serve
```

Open **http://localhost:4200** in your browser.

---

### 3 — Run the Unit Tests

```bash
cd ABB-TicTacToe/TicTacToe.API.Tests
dotnet test
```

Expected output: **19 tests passing**.

---

## API Reference

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/game/start` | Start a new game (`mode`: `TwoPlayer` or `VsComputer`) |
| `GET` | `/api/game/{id}` | Get current game state |
| `POST` | `/api/game/{id}/move` | Make a move (`cell`: 0–8) |
| `POST` | `/api/game/{id}/undo` | Undo the last move |
| `POST` | `/api/game/{id}/reset` | Reset the board (keep scoreboard) |
| `POST` | `/api/scoreboard/reset` | Reset the scoreboard for a game |

Full API contract with request/response shapes is in [ARCHITECTURE.md](./ARCHITECTURE.md).

---

## Computer AI Strategy

The AI evaluates moves in priority order:

1. **Win** — take the winning cell if available
2. **Block** — prevent the human from winning on the next move
3. **Center** — take cell 4 if free
4. **Corner** — take any corner (0, 2, 6, 8) if free
5. **Any cell** — fall back to the first available cell

---

## Design Approach

This project was built **design-first**: the `ARCHITECTURE.md` file was committed before any application code, establishing the API contract, component responsibilities, and data flow. This mirrors how I lead engineering teams — alignment on design before implementation reduces rework and makes code review meaningful.

### Conceptualisation

The full solution was **conceptualised independently** — system architecture, component breakdown, API contract, game logic design, AI strategy, and test coverage plan were all defined upfront without AI assistance. This thinking is documented in `ARCHITECTURE.md`.

### Implementation

For the actual coding, **Claude Code (VS Code extension) was used as an AI pair programmer** for both the .NET Web API and the Angular frontend. This was a deliberate, transparent choice — I manage engineering teams where AI-assisted development is increasingly the norm, and I wanted to demonstrate that workflow in practice.

Every AI-generated change was:
- **Reviewed** for correctness and alignment with the design
- **Tested manually** end-to-end before committing
- **Committed with a descriptive message** explaining what changed and why

Bugs that Claude Code introduced or missed — such as the `gameId` vs `id` field mismatch that made the board unclickable — were diagnosed and fixed through independent reasoning, not further AI prompting. The commit history reflects this honest, iterative process.

> As an SDM, my value is in the decisions, the architecture, and the judgement — not in typing code. Using AI tooling transparently and effectively is part of modern engineering leadership.

---

## Bugs Diagnosed & Fixed

All three bugs below were identified through manual testing and code review — not flagged by AI. Each required tracing from a UI symptom back to its root cause.

### Bug 1 — Board cells unclickable after game start

**Symptom:** Game started successfully but clicking any cell on the board had no effect.

**Root Cause:** The API response returns a field named `id`, but the Angular `GameState` interface declared it as `gameId`. Because the field name didn't match, the signal was always `null`. The `makeMove()` method had an early return guard — `if (!id) return` — so every click was silently dropped.

**Fix:** Renamed `gameId: string` → `id: string` in the interface, and updated `this.gameId.set(state.gameId)` → `this.gameId.set(state.id)` in `startGame()`.

---

### Bug 2 — Computer AI not using smart strategy

**Symptom:** In VS Computer mode, the AI was making random moves instead of blocking or winning.

**Root Cause:** `ComputerPlayerService` with win/block/center/corner logic existed and was registered in DI, but `GameService` had its own private `PickComputerMove()` method that just picked a random empty cell — completely bypassing the service.

**Fix:** Injected `ComputerPlayerService` into `GameService` via constructor, removed the random picker, and wired `_computerPlayerService.GetBestMove(game.Board)` into the move flow.

---

### Bug 3 — Build artifacts tracked by git

**Symptom:** `git status` showed `bin/` and `obj/` folders in the test project as constantly modified, polluting the commit history.

**Root Cause:** The `TicTacToe.API.Tests/` project was missing a `.gitignore` file, so compiled output was being tracked.

**Fix:** Added `.gitignore` to the test project, then ran `git rm -r --cached bin/ obj/` to untrack the already-committed folders.

---

## Future Enhancements

These are features that would be natural next steps given more time, prioritised by user value.

### High Priority

| Enhancement | Reason |
|---|---|
| **Persistent storage (SQL / Redis)** | Game state is currently in-memory — restarting the API loses all sessions. A real database would support multiple concurrent users and session recovery. |
| **Authentication** | Named players with login would allow personal scoreboards and game history across sessions. |
| **Minimax AI** | The current AI uses a priority heuristic. Minimax with alpha-beta pruning would make the computer truly unbeatable and is the standard algorithm for this problem. |

### Medium Priority

| Enhancement | Reason |
|---|---|
| **Multiplayer over WebSocket** | Real-time two-player games between different devices — replaces the current same-device two-player mode. |
| **Replay mode** | Step through a completed game move by move using the existing `moveHistory` array — the data is already tracked. |
| **Mobile responsive UI** | The current layout works on desktop; a touch-optimised grid would improve mobile experience. |

### Low Priority / Nice to Have

| Enhancement | Reason |
|---|---|
| **Difficulty levels** | Easy (random), Medium (heuristic), Hard (minimax) — giving players a choice. |
| **Animated board** | Winning line highlight, cell placement animations — purely cosmetic but improves feel. |
| **Docker Compose** | Single `docker compose up` to start both API and frontend — simplifies onboarding for new developers. |

---

## Author

**Kalpendu Das**  
Technical Program Manager  
das.kalpendu@gmail.com
