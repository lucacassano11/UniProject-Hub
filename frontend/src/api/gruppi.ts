import { api } from "./client";

export type Membro = { studenteEmail: string; isCapogruppo: boolean };
export type Gruppo = {
  idGruppo: number; idSessione: string; dimensione: number; stato: string;
  capogruppoEmail: string | null; numeroMembri: number; membri: Membro[];
};
export type Invito = { idRichiesta: number; idGruppo: number; studenteEmail: string; esito: boolean | null };
export type Accesso = { idRichiesta: number; idGruppo: number; studenteEmail: string; esito: boolean | null };
export type Dimensione = { idRichiesta: number; idGruppo: number; idSessione: string; descrizione: string; esito: boolean | null };

const B = "/api/gruppi";

export const listMieiGruppi = () => api<Gruppo[]>(`${B}/gruppi/miei`);
export const listGruppiSessione = (idSessione: string) => api<Gruppo[]>(`${B}/sessioni/${idSessione}/gruppi`);
export const sessioneEsiste = (idSessione: string) => api<{ esiste: boolean }>(`${B}/sessioni/${idSessione}/esiste`);
export const getGruppo = (id: number) => api<Gruppo>(`${B}/gruppi/${id}`);
export const createGruppo = (idSessione: string) =>
  api<Gruppo>(`${B}/gruppi`, { method: "POST", body: JSON.stringify({ idSessione }) });
export const abbandonaGruppo = (id: number) =>
  api<void>(`${B}/gruppi/${id}/abbandona`, { method: "POST" });

export const invita = (id: number, studenteEmail: string) =>
  api<Invito>(`${B}/gruppi/${id}/inviti`, { method: "POST", body: JSON.stringify({ studenteEmail }) });
export const listInvitiMiei = () => api<Invito[]>(`${B}/inviti/miei`);
export const accettaInvito = (idR: number) => api<void>(`${B}/inviti/${idR}/accetta`, { method: "POST" });
export const rifiutaInvito = (idR: number) => api<void>(`${B}/inviti/${idR}/rifiuta`, { method: "POST" });

export const chiediAccesso = (id: number) => api<Accesso>(`${B}/gruppi/${id}/richieste-accesso`, { method: "POST" });
export const listAccessi = (id: number) => api<Accesso[]>(`${B}/gruppi/${id}/richieste-accesso`);
export const accettaAccesso = (idR: number) => api<void>(`${B}/richieste-accesso/${idR}/accetta`, { method: "POST" });
export const rifiutaAccesso = (idR: number) => api<void>(`${B}/richieste-accesso/${idR}/rifiuta`, { method: "POST" });

export const chiediDimensione = (id: number, nuovaDimensione: number, descrizione: string) =>
  api<Dimensione>(`${B}/gruppi/${id}/richieste-dimensione`, { method: "POST", body: JSON.stringify({ nuovaDimensione, descrizione }) });
export const listDimensioni = (id: number) => api<Dimensione[]>(`${B}/gruppi/${id}/richieste-dimensione`);
