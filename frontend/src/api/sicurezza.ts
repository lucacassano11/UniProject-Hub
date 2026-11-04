import { api } from "./client";

export type UtenteSicurezza = {
  email: string; ruolo: string; failedAttempts: number; lockedUntil: string | null; bloccato: boolean;
};
export type Evento = {
  idEvento: number; tipo: string; attoreEmail: string | null; dettaglio: string; createdAt: string;
};

export const listUtenti = () => api<UtenteSicurezza[]>("/api/sicurezza/utenti");
export const blocca = (email: string) =>
  api<void>(`/api/sicurezza/utenti/${encodeURIComponent(email)}/blocca`, { method: "POST" });
export const sblocca = (email: string) =>
  api<void>(`/api/sicurezza/utenti/${encodeURIComponent(email)}/sblocca`, { method: "POST" });

export const listEventi = (tipo?: string, limit = 50) =>
  api<Evento[]>(`/api/log/eventi?${tipo ? `tipo=${encodeURIComponent(tipo)}&` : ""}limit=${limit}`);
