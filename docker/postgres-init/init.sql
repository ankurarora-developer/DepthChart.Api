-- Enable UUID extension if not already
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Teams (Patriots, Jets, etc.)
CREATE TABLE teams (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    sport TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Players (shared across teams)
CREATE TABLE players (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    number INT NOT NULL,  
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Root depth chart per team
CREATE TABLE depth_charts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    team_id UUID NOT NULL UNIQUE,         -- one chart per team
    updated_at_utc TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_depth_chart_team FOREIGN KEY (team_id) REFERENCES teams(id) ON DELETE CASCADE
);

-- Positions (QB, RB, WR, etc.)
CREATE TABLE depth_chart_positions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    depth_chart_id UUID NOT NULL,
    position_code TEXT NOT NULL,
    CONSTRAINT fk_position_chart FOREIGN KEY (depth_chart_id) REFERENCES depth_charts(id) ON DELETE CASCADE,
    CONSTRAINT uq_position_per_chart UNIQUE (depth_chart_id, position_code)
);

-- Entries (players at depth levels)
CREATE TABLE depth_chart_entries (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    position_id UUID NOT NULL,
    player_id UUID NOT NULL,
    depth INT NOT NULL,
    CONSTRAINT fk_entry_position FOREIGN KEY (position_id) REFERENCES depth_chart_positions(id) ON DELETE CASCADE,
    CONSTRAINT fk_entry_player FOREIGN KEY (player_id) REFERENCES players(id) ON DELETE CASCADE,
    CONSTRAINT uq_depth_per_position UNIQUE (position_id, depth)
);

-- Helpful indexes
CREATE INDEX idx_depth_chart_team_id ON depth_charts(team_id);
CREATE INDEX idx_position_chart_id ON depth_chart_positions(depth_chart_id);
CREATE INDEX idx_entry_position_id ON depth_chart_entries(position_id);
CREATE INDEX idx_entry_player_id ON depth_chart_entries(player_id);


--seed teams
INSERT INTO teams (id, name, sport, created_at)
VALUES (
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  'Patriots',
  'NFL',
  NOW()
);