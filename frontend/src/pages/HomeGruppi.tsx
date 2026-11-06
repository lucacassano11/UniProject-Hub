import { useEffect, useState, type CSSProperties } from "react";
import { useNavigate } from "react-router-dom";
import DashboardLayout from "../components/DashboardLayout";
import {
  listGruppiSessione, listMieiGruppi, createGruppo, chiediAccesso, type Gruppo,
} from "../api/gruppi";
import { nomeDaEmail, iniziali, coloreAvatar } from "../ui";

async function fetchHomeGruppiData(sessione: string) {
  const tutti = await listGruppiSessione(sessione);
  const miei = await listMieiGruppi();
  return { tutti, mio: miei.find((g) => g.idSessione === sessione) ?? null };
}

export default function HomeGruppi() {
  const nav = useNavigate();
  const sessione = sessionStorage.getItem("sessione") ?? "";
  const [gruppi, setGruppi] = useState<Gruppo[]>([]);
  const [mioGruppo, setMioGruppo] = useState<Gruppo | null>(null);
  const [search, setSearch] = useState("");
  const [err, setErr] = useState("");

  useEffect(() => {
    let mounted = true;
    fetchHomeGruppiData(sessione).then(
      ({ tutti, mio }) => {
        if (!mounted) return;
        setGruppi(tutti);
        setMioGruppo(mio);
      },
      () => {
        if (mounted) setErr("Errore nel caricamento dei gruppi");
      },
    );
    return () => { mounted = false; };
  }, [sessione]);

  async function crea() {
    setErr("");
    try { const g = await createGruppo(sessione); nav(`/gruppi/${g.idGruppo}`); }
    catch (e) { setErr(e instanceof Error ? e.message : "Impossibile creare il gruppo"); }
  }
  async function richiedi(id: number) {
    setErr("");
    try {
      await chiediAccesso(id);
      const { tutti, mio } = await fetchHomeGruppiData(sessione);
      setGruppi(tutti);
      setMioGruppo(mio);
    } catch (e) { setErr(e instanceof Error ? e.message : "Richiesta non riuscita"); }
  }

  const visibili = gruppi.filter((g) =>
    !search || (g.capogruppoEmail ?? "").toLowerCase().includes(search.toLowerCase())
    || nomeDaEmail(g.capogruppoEmail ?? "").toLowerCase().includes(search.toLowerCase()));

  return (
    <DashboardLayout active="gruppi">
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 24 }}>
        <div>
          <h2 style={{ fontSize: 24, fontWeight: 700 }}>Gruppi Disponibili</h2>
          <div style={{ fontSize: 13, color: "#64748b", marginTop: 4 }}>
            Sessione: <strong style={{ color: "var(--accent)" }}>{sessione}</strong>
          </div>
        </div>
        {!mioGruppo && <button className="btn-primary" onClick={crea}>+ Crea Gruppo</button>}
      </div>

      {err && <p style={{ color: "#f87171", fontSize: 13, marginBottom: 12 }}>{err}</p>}

      {mioGruppo ? (
        <div style={{ ...alertBox, background: "rgba(34,197,94,0.08)", borderColor: "rgba(34,197,94,0.3)", color: "#22c55e" }}>
          <span>✓ Fai parte del Gruppo #{mioGruppo.idGruppo}.</span>
          <button className="btn-primary" style={{ padding: "6px 14px" }}
            onClick={() => nav(`/gruppi/${mioGruppo.idGruppo}`)}>Vai al tuo gruppo</button>
        </div>
      ) : (
        <div style={alertBox}>⚠ Non fai ancora parte di nessun gruppo. Creane uno o richiedi l'accesso!</div>
      )}

      <input placeholder="Cerca gruppo per nome capogruppo..." value={search}
        onChange={(e) => setSearch(e.target.value)}
        style={{ width: "100%", padding: "10px 16px", background: "var(--panel)",
          border: "1px solid var(--border)", borderRadius: 8, color: "var(--text)",
          fontSize: 14, outline: "none", marginBottom: 20 }} />

      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: 16 }}>
        {visibili.map((g) => {
          const pieno = g.numeroMembri >= g.dimensione;
          return (
            <div key={g.idGruppo} style={card}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 12 }}>
                <div style={{ fontSize: 16, fontWeight: 600, color: "#fff" }}>
                  Gruppo di {nomeDaEmail(g.capogruppoEmail ?? "")}
                </div>
                <span style={pieno ? badgeRed : badgeGreen}>{pieno ? "Completo" : "Aperto"}</span>
              </div>
              <div style={{ fontSize: 13, color: "#64748b", marginBottom: 12 }}>
                Capogruppo: {nomeDaEmail(g.capogruppoEmail ?? "")}
              </div>
              <div style={{ display: "flex", alignItems: "center", marginBottom: 14 }}>
                {g.membri.map((m, i) => (
                  <div key={m.studenteEmail} title={m.studenteEmail} style={{ width: 28, height: 28,
                    borderRadius: "50%", background: coloreAvatar(m.studenteEmail), display: "flex",
                    alignItems: "center", justifyContent: "center", fontSize: 10, fontWeight: 700,
                    color: "#fff", border: "2px solid var(--panel)", marginLeft: i === 0 ? 0 : -6 }}>
                    {iniziali(m.studenteEmail)}
                  </div>
                ))}
                <span style={{ fontSize: 13, color: "var(--muted)", marginLeft: 8 }}>
                  {g.numeroMembri}/{g.dimensione} membri
                </span>
              </div>
              <button onClick={() => richiedi(g.idGruppo)} disabled={pieno || !!mioGruppo}
                style={pieno || mioGruppo ? btnDisabled : btnFull}>
                {pieno ? "Gruppo completo" : "Richiedi Accesso"}
              </button>
            </div>
          );
        })}
        {visibili.length === 0 && <p style={{ color: "var(--muted)" }}>Nessun gruppo in questa sessione.</p>}
      </div>
    </DashboardLayout>
  );
}

const alertBox: CSSProperties = {
  background: "rgba(234,179,8,0.1)", border: "1px solid rgba(234,179,8,0.3)", borderRadius: 10,
  padding: "14px 20px", marginBottom: 20, fontSize: 14, color: "#eab308",
  display: "flex", alignItems: "center", justifyContent: "space-between", gap: 10,
};
const card: CSSProperties = {
  background: "var(--panel)", borderRadius: 12, padding: 20, border: "1px solid var(--border)",
};
const badgeGreen: CSSProperties = {
  fontSize: 11, padding: "3px 8px", borderRadius: 20, fontWeight: 600,
  background: "rgba(34,197,94,0.15)", color: "#22c55e",
};
const badgeRed: CSSProperties = {
  fontSize: 11, padding: "3px 8px", borderRadius: 20, fontWeight: 600,
  background: "rgba(239,68,68,0.15)", color: "#ef4444",
};
const btnFull: CSSProperties = {
  width: "100%", marginTop: 8, padding: "8px 14px", background: "var(--accent)", color: "#fff",
  border: "none", borderRadius: 6, fontSize: 13, fontWeight: 600, cursor: "pointer",
};
const btnDisabled: CSSProperties = {
  ...btnFull, background: "var(--border)", color: "#64748b", cursor: "not-allowed",
};
