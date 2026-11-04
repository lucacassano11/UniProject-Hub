// Helper UI condivisi: nome visualizzato, iniziali e colore avatar derivati dall'email.

export function nomeDaEmail(email: string): string {
  const local = email.split("@")[0] ?? email;
  return local
    .split(/[._]/)
    .filter(Boolean)
    .map((p) => p.charAt(0).toUpperCase() + p.slice(1))
    .join(" ");
}

export function iniziali(email: string): string {
  const parts = (email.split("@")[0] ?? email).split(/[._]/).filter(Boolean);
  const a = parts[0]?.[0] ?? "?";
  const b = parts[1]?.[0] ?? "";
  return (a + b).toUpperCase();
}

const palette = ["#6366f1", "#ec4899", "#f59e0b", "#22c55e", "#06b6d4", "#a855f7", "#f43f5e", "#14b8a6", "#3b82f6", "#84cc16"];

export function coloreAvatar(email: string): string {
  let h = 0;
  for (let i = 0; i < email.length; i++) h = (h * 31 + email.charCodeAt(i)) >>> 0;
  return palette[h % palette.length];
}
