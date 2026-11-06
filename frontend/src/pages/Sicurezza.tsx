import { useEffect, useState, type CSSProperties } from "react";
import DashboardLayout from "../components/DashboardLayout";
import { listUtenti, blocca, sblocca, listEventi, type UtenteSicurezza, type Evento } from "../api/sicurezza";

async function fetchSicurezzaData() {
  const u = await listUtenti();
  const ev = await listEventi(undefined, 50).catch(() => []);
  return { u, ev };
}

export default function Sicurezza() {
  const [utenti, setUtenti] = useState<UtenteSicurezza[]>([]);
  const [eventi, setEventi] = useState<Evento[]>([]);
  const [err, setErr] = useState("");

  useEffect(() => {
    let mounted = true;
    fetchSicurezzaData().then(
      ({ u, ev }) => {
        if (!mounted) return;
        setUtenti(u);
        setEventi(ev);
      },
      () => {
        if (mounted) setErr("Errore nel caricamento");
      },
    );
    return () => { mounted = false; };
  }, []);

  async function toggle(u: UtenteSicurezza) {
    setErr("");
    try {
      if (u.bloccato) await sblocca(u.email); else await blocca(u.email);
      const res = await fetchSicurezzaData();
      setUtenti(res.u);
      setEventi(res.ev);
    } catch { setErr("Operazione non riuscita"); }
  }

  return (
    <DashboardLayout active="sicurezza">
      <h2 style={{ fontSize: 24, fontWeight: 700, marginBottom: 20 }}>Sicurezza</h2>
      {err && <p style={{ color: "#f87171", fontSize: 13, marginBottom: 12 }}>{err}</p>}

      <div className="card" style={{ marginBottom: 20 }}>
        <h3 style={{ fontSize: 16, marginBottom: 12 }}>Account</h3>
        <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 13 }}>
          <thead>
            <tr style={{ textAlign: "left", color: "var(--muted)" }}>
              <th style={th}>Email</th><th style={th}>Ruolo</th><th style={th}>Tentativi</th>
              <th style={th}>Stato</th><th style={th}></th>
            </tr>
          </thead>
          <tbody>
            {utenti.map((u) => (
              <tr key={u.email} style={{ borderTop: "1px solid var(--border)" }}>
                <td style={td}>{u.email}</td>
                <td style={td}>{u.ruolo}</td>
                <td style={td}>{u.failedAttempts}</td>
                <td style={{ ...td, color: u.bloccato ? "#ef4444" : "#22c55e" }}>
                  {u.bloccato ? "bloccato" : "attivo"}
                </td>
                <td style={td}>
                  <button onClick={() => toggle(u)} style={u.bloccato ? unlockBtn : lockBtn}>
                    {u.bloccato ? "Sblocca" : "Blocca"}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="card">
        <h3 style={{ fontSize: 16, marginBottom: 12 }}>Eventi recenti</h3>
        {eventi.length === 0 && <p style={{ color: "var(--muted)", fontSize: 13 }}>Nessun evento.</p>}
        {eventi.map((e) => (
          <div key={e.idEvento} style={{ padding: "8px 0", borderTop: "1px solid var(--border)", fontSize: 13 }}>
            <span style={{ color: "var(--accent)", fontWeight: 600 }}>{e.tipo}</span>
            <span style={{ color: "var(--muted)" }}> · {e.attoreEmail ?? "sistema"} · {new Date(e.createdAt).toLocaleString("it-IT")}</span>
            <div style={{ color: "var(--muted)" }}>{e.dettaglio}</div>
          </div>
        ))}
      </div>
    </DashboardLayout>
  );
}

const th: CSSProperties = { padding: "6px 8px", fontWeight: 600, textTransform: "uppercase", fontSize: 11 };
const td: CSSProperties = { padding: "8px 8px" };
const lockBtn: CSSProperties = {
  padding: "6px 12px", background: "transparent", color: "#ef4444",
  border: "1px solid #ef4444", borderRadius: 8, cursor: "pointer", fontSize: 12,
};
const unlockBtn: CSSProperties = {
  padding: "6px 12px", background: "var(--accent)", color: "#fff",
  border: "none", borderRadius: 8, cursor: "pointer", fontSize: 12,
};
