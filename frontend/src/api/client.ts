const BASE = import.meta.env.VITE_API_BASE ?? "http://localhost:8080";

export async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const token = sessionStorage.getItem("token");
  const res = await fetch(`${BASE}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
  });
  if (!res.ok) {
    let msg = `Errore ${res.status}`;
    try {
      const body = await res.json();
      if (body && typeof body.error === "string") msg = body.error;
    } catch { /* corpo non JSON */ }
    throw new Error(msg);
  }
  return res.status === 204 ? (undefined as T) : await res.json();
}
