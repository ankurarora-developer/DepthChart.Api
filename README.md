# DepthChart API

A **Clean Architecture .NET 8 Minimal API** for managing sports team depth charts.  
Supports adding/removing players, viewing backups, and printing the full depth chart for a team.  

---

## 🚀 Features

- **Add Player to Depth Chart**
  - Insert at a given depth or append if depth not specified.
  - Shifts existing players down when inserting in the middle.
  - Prevents duplicates for the same position.
  - Enforces sequential depths (no gaps).

- **Remove Player from Depth Chart**
  - Removes a player from a given position.
  - Returns removed player if found, otherwise an empty list.

- **Get Backups**
  - Returns all backups (players lower in depth) for a given player.
  - Returns empty if player not found or no backups exist.

- **Get Full Depth Chart**
  - Returns all positions and players for the team.

---

## 🛠️ Tech Stack

- [.NET 8](https://dotnet.microsoft.com/) — Minimal API  
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) + [Npgsql](https://www.npgsql.org/) — Postgres provider  
- **Clean Architecture Layers**:
  - `Domain` → Entities & interfaces  
  - `Application` → Business logic & services  
  - `Infrastructure` → EF Core repositories & DbContext  
  - `Api` → Minimal API endpoints & middleware  

---

## ⚙️ Setup

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)  
- [Docker](https://docs.docker.com/get-docker/)  

### 2. Run Postgres via Docker Compose
```bash
docker compose up -d
```
- Docker compose will seed team with Id aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa

---

## 🔗 Endpoints

- Local base url - http://localhost:5258
- Docker Base url - http://localhost:5000

👉 A Bruno collection is also available at the repo root (bruno-collection/) for easy manual API testing.

Health - 
GET /health

Add Player - 
POST /teams/{teamId}/depthchart/add
Body:
{
  "position": "QB",
  "name": "Tom Brady",
  "number": 12,
  "positionDepth": 0
}

Remove Player - 
POST /teams/{teamId}/depthchart/remove
Body:
{
  "position": "QB",
  "name": "Tom Brady",
  "number": 12
}

Get Backups - 
GET /teams/{teamId}/depthchart/{position}/{name}/{number}/backups

Full Depth Chart - 
GET /teams/{teamId}/depthchart

---
📖 Edge Cases Covered

No duplicate players per position.

Sequential depths enforced (no gaps).

Invalid inputs rejected at API boundary.

Removing non-existent players returns empty.

Backups empty if player not found or no backups.

---

🧪 Testing

- **Run unit tests**
```
dotnet test tests/DepthChart.UnitTests
```
- **Run integration tests**
```
dotnet test tests/DepthChart.IntegrationTests
```



