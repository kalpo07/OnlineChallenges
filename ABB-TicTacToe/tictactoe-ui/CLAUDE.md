# Claude Code — AI Development Notes

This project was developed using Claude Code as an AI coding assistant.

## How Claude Code was used

- Backend (.NET Web API) and frontend (Angular) syntax and boilerplate were generated with Claude Code prompts
- All architectural decisions, API design, and data flow were designed independently before any code was written — see `../ARCHITECTURE.md`
- Test scenarios were defined upfront (listed in ARCHITECTURE.md) before prompting Claude to implement them
- All generated code was reviewed, understood, and where necessary corrected before committing

## What was not delegated to AI

- System architecture and layer responsibilities
- API contract design (endpoints, request/response structure)
- Key engineering decisions (backend as source of truth, undo behaviour, computer AI priority order)
- Test scenario identification

## Developer background

Java and Python background. Used Claude Code to bridge the .NET and Angular syntax gap while retaining full ownership of design and engineering decisions.