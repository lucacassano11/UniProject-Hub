import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import DashboardLayout from "../components/DashboardLayout";
import { listSessions, createSession, deleteSession, type Session } from "../api/sessioni";

export default function HomeSessioni() {
  const [sessions, setSessions] = useState<Session[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [voto, setVoto] = useState(30);
  const nav = useNavigate();

  async function load() { setSessions(await listSessions()); }
  useEffect(() => { load().catch(() => {}); }, []);

  async function create() {
    await createSession(voto);
    setShowModal(false);
    setVoto(30);
    await load();
  }
  async function remove(id: string, e: React.MouseEvent) {
    e.stopPropagation();
    try { await deleteSession(id); await load(); }
    catch { alert("Impossibile eliminare la sessione (potrebbe avere gruppi)"); }
  }

  return (
    <DashboardLayout active="sessioni">
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 28 }}>
        <h2 style={{ fontSize: 24, fontWeight: 700 }}>Le mie Sessioni</h2>
        <button className="btn-primary" onClick={() => setShowModal(true)}>+ Nuova Sessione</button>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(320px, 1fr))", gap: 16 }}>
        {sessions.map((s) => (
          <div key={s.idSessione} onClick={() => nav(`/sessioni/${s.idSessione}`)} style={{
            background: "var(--panel)", borderRadius: 12, padding: 24,
            border: "1px solid var(--border)", cursor: "pointer" }}>
            <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 16 }}>
              <div>
                <div style={{ fontSize: 17, fontWeight: 600, color: "#fff" }}>{s.idSessione}</div>
                <div style={{ fontSize: 13, color: "#64748b" }}>Creata il {s.dataCreazione}</div>
              </div>
              <button onClick={(e) => remove(s.idSessione, e)} title="Elimina" style={{
                background: "none", border: "none", color: "#64748b", cursor: "pointer", fontSize: 16 }}>🗑</button>
            </div>
            <div style={{ display: "flex", gap: 20, paddingTop: 16, borderTop: "1px solid var(--border)" }}>
              <Stat val={s.votoMassimo} lbl="Voto Max" />
              <Stat val={s.numeroAppelli} lbl="Appelli" />
            </div>
          </div>
        ))}
        {sessions.length === 0 && <p style={{ color: "var(--muted)" }}>Nessuna sessione. Creane una.</p>}
      </div>

      {showModal && (
        <div onClick={(e) => { if (e.target === e.currentTarget) setShowModal(false); }} style={overlay}>
          <div style={{ background: "var(--panel)", borderRadius: 16, padding: 32, width: 400, border: "1px solid var(--border)" }}>
            <h3 style={{ fontSize: 18, marginBottom: 20 }}>Nuova Sessione</h3>
            <label style={{ display: "block", fontSize: 13, fontWeight: 600, color: "var(--muted)", marginBottom: 6 }}>Voto Massimo</label>
            <input type="number" min={1} max={33} value={voto} onChange={(e) => setVoto(Number(e.target.value))}
              style={{ width: "100%", padding: "10px 14px", background: "var(--bg)", border: "1px solid var(--border)",
                borderRadius: 8, color: "var(--text)", fontSize: 14, outline: "none" }} />
            <div style={{ display: "flex", gap: 10, justifyContent: "flex-end", marginTop: 20 }}>
              <button onClick={() => setShowModal(false)} style={{ padding: "10px 20px", background: "var(--border)",
                color: "var(--text)", border: "none", borderRadius: 8, cursor: "pointer" }}>Annulla</button>
              <button className="btn-primary" onClick={create}>Crea Sessione</button>
            </div>
          </div>
        </div>
      )}
    </DashboardLayout>
  );
}

function Stat({ val, lbl }: { val: number; lbl: string }) {
  return (
    <div style={{ textAlign: "center" }}>
      <div style={{ fontSize: 20, fontWeight: 700, color: "#fff" }}>{val}</div>
      <div style={{ fontSize: 11, color: "#64748b", textTransform: "uppercase", letterSpacing: 0.5, marginTop: 2 }}>{lbl}</div>
    </div>
  );
}

const overlay: React.CSSProperties = {
  position: "fixed", inset: 0, background: "rgba(0,0,0,0.6)", zIndex: 100,
  display: "flex", alignItems: "center", justifyContent: "center",
};
