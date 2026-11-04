import { api } from "./client";

export type Abstract = {
  idAbstract: number; idGruppo: number; descrizione: string;
  dataOraConsegna: string; modificheProposte: string | null; esito: boolean | null;
};
export type Progetto = {
  idProgetto: number; idGruppo: number; fileZip: string;
  dataOraConsegna: string; valutazione: number | null; accettazione: boolean | null;
};
export type ConsegnaGruppo = { idGruppo: number; stato: string; abstract: Abstract | null; progetto: Progetto | null };

const B = "/api/consegne";

export const getAbstract = (id: number) => api<Abstract>(`${B}/gruppi/${id}/abstract`);
export const submitAbstract = (id: number, descrizione: string) =>
  api<Abstract>(`${B}/gruppi/${id}/abstract`, { method: "POST", body: JSON.stringify({ descrizione }) });
export const approvaAbstract = (id: number) => api<void>(`${B}/gruppi/${id}/abstract/approva`, { method: "POST" });
export const rifiutaAbstract = (id: number, modificheProposte: string) =>
  api<void>(`${B}/gruppi/${id}/abstract/rifiuta`, { method: "POST", body: JSON.stringify({ modificheProposte }) });

export const getProgetto = (id: number) => api<Progetto>(`${B}/gruppi/${id}/progetto`);
export const uploadProgetto = (id: number, fileZip: string) =>
  api<Progetto>(`${B}/gruppi/${id}/progetto`, { method: "POST", body: JSON.stringify({ fileZip }) });
export const valutaProgetto = (id: number, valutazione: number) =>
  api<void>(`${B}/gruppi/${id}/progetto/valuta`, { method: "POST", body: JSON.stringify({ valutazione }) });
export const accettaVoto = (id: number) => api<void>(`${B}/gruppi/${id}/progetto/accetta`, { method: "POST" });
export const rifiutaVoto = (id: number) => api<void>(`${B}/gruppi/${id}/progetto/rifiuta`, { method: "POST" });

export const panoramicaConsegne = (idSessione: string) =>
  api<ConsegnaGruppo[]>(`${B}/sessioni/${idSessione}/consegne`);
