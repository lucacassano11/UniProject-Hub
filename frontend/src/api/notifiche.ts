import { api } from "./client";

export type Notifica = {
  idNotifica: number; tipo: string; messaggio: string; letta: boolean; createdAt: string;
};

const B = "/api/notifiche";

export const listNotifiche = (soloNonLette = false) =>
  api<Notifica[]>(`${B}/notifiche?soloNonLette=${soloNonLette}`);
export const contaNonLette = () => api<{ nonLette: number }>(`${B}/notifiche/conteggio`);
export const segnaLetta = (id: number) => api<void>(`${B}/notifiche/${id}/letta`, { method: "POST" });
