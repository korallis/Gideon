/**
 * ESI API Type Definitions
 * TypeScript interfaces for EVE Online API responses
 */

// Authentication Types
export interface ESIAuthResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  refresh_token: string;
  scope?: string;
}

export interface ESITokenInfo {
  CharacterID: number;
  CharacterName: string;
  ExpiresOn: string;
  Scopes: string;
  TokenType: string;
  CharacterOwnerHash: string;
  IntellectualProperty: string;
}

// Character Types
export interface ESICharacter {
  character_id: number;
  name: string;
  description?: string;
  corporation_id: number;
  alliance_id?: number;
  birthday: string;
  gender: 'male' | 'female';
  race_id: number;
  bloodline_id: number;
  ancestry_id: number;
  security_status?: number;
  faction_id?: number;
  title?: string;
}

export interface ESICharacterSkills {
  skills: ESISkill[];
  total_sp: number;
  unallocated_sp?: number;
}

export interface ESISkill {
  skill_id: number;
  skillpoints_in_skill: number;
  trained_skill_level: number;
  active_skill_level: number;
}

// Market Types
export interface ESIMarketOrder {
  order_id: number;
  type_id: number;
  location_id: number;
  volume_total: number;
  volume_remain: number;
  min_volume: number;
  price: number;
  is_buy_order: boolean;
  duration: number;
  issued: string;
  range: string;
}

export interface ESIMarketHistory {
  date: string;
  order_count: number;
  volume: number;
  highest: number;
  average: number;
  lowest: number;
}

// Universe Types
export interface ESIType {
  type_id: number;
  name: string;
  description: string;
  published: boolean;
  group_id: number;
  market_group_id?: number;
  radius?: number;
  volume?: number;
  packaged_volume?: number;
  icon_id?: number;
  sound_id?: number;
  graphic_id?: number;
}

export interface ESIGroup {
  group_id: number;
  name: string;
  description: string;
  published: boolean;
  category_id: number;
  types: number[];
}

export interface ESIRegion {
  region_id: number;
  name: string;
  description: string;
  constellations: number[];
}

// Error Types
export interface ESIError {
  error: string;
  error_description?: string;
  sso_status?: number;
}

// Request/Response wrapper types
export interface ESIResponse<T> {
  data: T;
  headers: Record<string, string>;
  status: number;
}

export interface ESIErrorResponse {
  error: ESIError;
  status: number;
}

// Pagination types
export interface ESIPaginatedResponse<T> extends ESIResponse<T> {
  totalPages?: number;
  currentPage: number;
}