import { api } from "./client";

export type Session = {
  idSessione: string; votoMassimo: number; dataCreazione: string;
  inizioInvioAbstract: string | null; numeroAppelli: number;
};
export type Appello = {
  idAppello: number; dataOraAppello: string; scadenzaConsegnaProgetto: string;
};

export const listSessions = () => api<Session[]>("/api/sessioni/sessioni");
export const createSession = (votoMassimo: number) =>
  api<Session>("/api/sessioni/sessioni", { method: "POST", body: JSON.stringify({ votoMassimo }) });
export const getSession = (id: string) => api<Session>(`/api/sessioni/sessioni/${id}`);
export const deleteSession = (id: string) =>
  api<void>(`/api/sessioni/sessioni/${id}`, { method: "DELETE" });
export const setInizioAbstract = (id: string, inizioInvioAbstract: string) =>
  api<void>(`/api/sessioni/sessioni/${id}/inizio-abstract`, { method: "PUT", body: JSON.stringify({ inizioInvioAbstract }) });

export const listAppelli = (id: string) => api<Appello[]>(`/api/sessioni/sessioni/${id}/appelli`);
export const addAppello = (id: string, dataOraAppello: string, scadenzaConsegnaProgetto: string) =>
  api<Appello>(`/api/sessioni/sessioni/${id}/appelli`, { method: "POST", body: JSON.stringify({ dataOraAppello, scadenzaConsegnaProgetto }) });
export const deleteAppello = (idAppello: number) =>
  api<void>(`/api/sessioni/appelli/${idAppello}`, { method: "DELETE" });
