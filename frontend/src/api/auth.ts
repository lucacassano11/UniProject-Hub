import { api } from "./client";

export type AuthResponse = { email: string; ruolo: string; token?: string };

export const register = (email: string, password: string, ruolo: string) =>
  api<AuthResponse>("/api/auth/register", {
    method: "POST",
    body: JSON.stringify({ email, password, ruolo }),
  });

export const login = (email: string, password: string) =>
  api<AuthResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
