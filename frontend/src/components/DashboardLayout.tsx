import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { listNotifiche, contaNonLette, segnaLetta, type Notifica } from "../api/notifiche";
import { nomeDaEmail, iniziali, coloreAvatar } from "../ui";

const ruoloLabel: Record<string, string> = {
  DOCENTE: "Docente", STUDENTE: "Studente", GESTORE_SICUREZZA: "Gestore Sicurezza",
};

export default function DashboardLayout({ children, active }: { children: React.ReactNode; active?: string }) {
  const nav = useNavigate();
  const ruolo = sessionStorage.getItem("ruolo") ?? "";
  const email = sessionStorage.getItem("email") ?? "";
  const [notifOpen, setNotifOpen] = useState(false);
  const [nonLette, setNonLette] = useState(0);
  const [notifiche, setNotifiche] = useState<Notifica[]>([]);

  function logout() { sessionStorage.clear(); nav("/login"); }

  async function refresh() {
    try { setNonLette((await contaNonLette()).nonLette); } catch { /* silenzioso */ }
  }
  useEffect(() => {
    refresh();
    const t = setInterval(refresh, 12000);
    return () => clearInterval(t);
  }, []);

  async function toggleNotif() {
    const next = !notifOpen;
    setNotifOpen(next);
    if (next) { try { setNotifiche(await listNotifiche(false)); } catch { /* silenzioso */ } }
  }
  async function leggi(n: Notifica) {
    if (!n.letta) { try { await segnaLetta(n.idNotifica); } catch { /* silenzioso */ } }
    await refresh();
    setNotifiche(await listNotifiche(false).catch(() => notifiche));
  }

  const navItem = (key: string, label: string, to: string) => (
    <Link to={to} style={navStyle(active === key)}>{label}</Link>
  );

  return (
    <div style={{ display: "flex", minHeight: "100vh" }}>
      <aside style={{ width: 240, background: "var(--panel)", padding: "24px 16px",
        borderRight: "1px solid var(--border)", position: "fixed", height: "100vh",
        display: "flex", flexDirection: "column" }}>
        <div style={{ fontSize: 18, fontWeight: 700, color: "#fff", padding: "0 8px 24px",
          borderBottom: "1px solid var(--border)", marginBottom: 16 }}>
          Uni<span style={{ color: "var(--accent)" }}>Project</span>-Hub
        </div>

        {ruolo === "DOCENTE" && navItem("sessioni", "📚 Le mie Sessioni", "/sessioni")}
        {ruolo === "STUDENTE" && navItem("gruppi", "👥 Gruppi", "/gruppi")}
        {ruolo === "GESTORE_SICUREZZA" && navItem("sicurezza", "🛡️ Sicurezza", "/sicurezza")}

        <button onClick={toggleNotif} style={{ ...navStyle(false), display: "flex",
          alignItems: "center", justifyContent: "space-between", background: "none", border: "none",
          cursor: "pointer", width: "100%", textAlign: "left", fontSize: 14 }}>
          <span>🔔 Notifiche</span>
          {nonLette > 0 && <span style={badge}>{nonLette}</span>}
        </button>

        <div style={{ flex: 1 }} />

        <div style={{ borderTop: "1px solid var(--border)", paddingTop: 12, display: "flex",
          alignItems: "center", gap: 10 }}>
          <div style={{ width: 32, height: 32, borderRadius: "50%", background: coloreAvatar(email),
            display: "flex", alignItems: "center", justifyContent: "center", fontSize: 12,
            fontWeight: 700, color: "#fff" }}>{iniziali(email)}</div>
          <div style={{ flex: 1, minWidth: 0 }}>
            <div style={{ fontSize: 13, fontWeight: 600, color: "#e2e8f0", overflow: "hidden",
              textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{nomeDaEmail(email)}</div>
            <div style={{ fontSize: 11, color: "#64748b" }}>{ruoloLabel[ruolo] ?? ruolo}</div>
          </div>
          <button onClick={logout} title="Esci" style={{ background: "none", border: "none",
            color: "var(--muted)", cursor: "pointer", fontSize: 16 }}>↩</button>
        </div>
      </aside>

      <main style={{ marginLeft: 240, flex: 1, padding: 32, position: "relative" }}>
        {notifOpen && (
          <div style={{ position: "fixed", left: 248, bottom: 24, width: 320, zIndex: 60,
            background: "var(--panel)", border: "1px solid var(--border)", borderRadius: 12,
            boxShadow: "0 20px 40px rgba(0,0,0,0.4)", maxHeight: 420, overflowY: "auto" }}>
            <div style={{ padding: "12px 16px", borderBottom: "1px solid var(--border)",
              fontWeight: 600, fontSize: 14 }}>Notifiche</div>
            {notifiche.length === 0 && <p style={{ padding: 16, color: "var(--muted)", fontSize: 13 }}>Nessuna notifica.</p>}
            {notifiche.map((n) => (
              <div key={n.idNotifica} onClick={() => leggi(n)} style={{ padding: "10px 16px",
                borderBottom: "1px solid var(--border)", cursor: "pointer",
                background: n.letta ? "transparent" : "rgba(99,102,241,0.10)" }}>
                <div style={{ fontSize: 11, color: "var(--accent)", fontWeight: 600 }}>{n.tipo}</div>
                <div style={{ fontSize: 13 }}>{n.messaggio}</div>
                <div style={{ fontSize: 11, color: "#64748b", marginTop: 2 }}>
                  {new Date(n.createdAt).toLocaleString("it-IT")}
                </div>
              </div>
            ))}
          </div>
        )}
        {children}
      </main>
    </div>
  );
}

function navStyle(activeItem: boolean): React.CSSProperties {
  return {
    display: "block", padding: "10px 12px", borderRadius: 8, marginBottom: 2,
    fontSize: 14, textDecoration: "none",
    color: activeItem ? "var(--accent)" : "var(--muted)",
    background: activeItem ? "rgba(99,102,241,0.15)" : "transparent",
    fontWeight: activeItem ? 600 : 400,
  };
}

const badge: React.CSSProperties = {
  background: "#ef4444", color: "#fff", borderRadius: 999, fontSize: 11, fontWeight: 700,
  minWidth: 18, height: 18, padding: "0 5px", display: "flex", alignItems: "center", justifyContent: "center",
};
