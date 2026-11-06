import { useEffect, useState, type CSSProperties } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import DashboardLayout from "../components/DashboardLayout";
import {
  getGruppo, abbandonaGruppo, invita, listAccessi, accettaAccesso, rifiutaAccesso,
  type Gruppo as GruppoT, type Accesso,
} from "../api/gruppi";
import {
  getAbstract, submitAbstract, getProgetto, uploadProgetto, accettaVoto, rifiutaVoto,
  type Abstract, type Progetto,
} from "../api/consegne";
import { nomeDaEmail, iniziali, coloreAvatar } from "../ui";

const ORDINE = ["NUOVO", "ABSTRACT_CONSEGNATO", "ABSTRACT_VALUTATO", "PROGETTO_CONSEGNATO", "PROGETTO_VALUTATO"];

async function fetchGruppoData(idGruppo: number, email: string) {
  const grp = await getGruppo(idGruppo);
  const accessi = grp?.capogruppoEmail === email
    ? await listAccessi(idGruppo).catch(() => [])
    : [];
  const abs = await getAbstract(idGruppo).catch(() => null);
  const prog = await getProgetto(idGruppo).catch(() => null);
  return { grp, accessi, abs, prog };
}

export default function Gruppo() {
  const { id = "" } = useParams();
  const idGruppo = Number(id);
  const nav = useNavigate();
  const email = sessionStorage.getItem("email") ?? "";
  const sessione = sessionStorage.getItem("sessione") ?? "";

  const [g, setG] = useState<GruppoT | null>(null);
  const [accessi, setAccessi] = useState<Accesso[]>([]);
  const [abs, setAbs] = useState<Abstract | null>(null);
  const [prog, setProg] = useState<Progetto | null>(null);
  const [invitoEmail, setInvitoEmail] = useState("");
  const [descr, setDescr] = useState("");
  const [fileZip, setFileZip] = useState("");
  const [err, setErr] = useState("");

  const isCapo = g?.capogruppoEmail === email;
  const idx = g ? ORDINE.indexOf(g.stato) : 0;

  useEffect(() => {
    let mounted = true;
    fetchGruppoData(idGruppo, email).then(
      (data) => {
        if (!mounted) return;
        setG(data.grp);
        setAccessi(data.accessi);
        setAbs(data.abs);
        setProg(data.prog);
      },
      () => {
        if (mounted) setErr("Errore nel caricamento");
      },
    );
    return () => { mounted = false; };
  }, [idGruppo, email]);

  async function reload() {
    const data = await fetchGruppoData(idGruppo, email);
    setG(data.grp);
    setAccessi(data.accessi);
    setAbs(data.abs);
    setProg(data.prog);
  }

  async function run(fn: () => Promise<unknown>, msg: string) {
    setErr("");
    try { await fn(); await reload(); } catch (e) { setErr(e instanceof Error ? e.message : msg); }
  }
  async function abbandona() {
    setErr("");
    try { await abbandonaGruppo(idGruppo); nav("/gruppi"); }
    catch (e) { setErr(e instanceof Error ? e.message : "Impossibile abbandonare"); }
  }

  if (!g) return <DashboardLayout active="gruppi"><p style={{ color: "var(--muted)" }}>Caricamento…</p></DashboardLayout>;

  const steps = [
    { t: "Gruppo creato", done: true, active: false },
    { t: "Gruppo completo", done: g.numeroMembri >= g.dimensione || idx >= 1, active: g.numeroMembri < g.dimensione && idx < 1 },
    { t: "Abstract approvato", done: idx >= 2, active: idx === 1 },
    { t: "Progetto consegnato", done: idx >= 3, active: idx === 2 },
    { t: "Valutazione", done: idx >= 4, active: idx === 3 },
  ];

  return (
    <DashboardLayout active="gruppi">
      <div style={{ fontSize: 13, color: "#64748b", marginBottom: 8 }}>
        <Link to="/gruppi" style={{ color: "var(--accent)" }}>Gruppi</Link> / Gruppo #{g.idGruppo}
      </div>
      <h2 style={{ fontSize: 24, fontWeight: 700 }}>Il mio Gruppo</h2>
      <div style={{ fontSize: 13, color: "#64748b", marginTop: 4, marginBottom: 20 }}>
        Sessione: <strong style={{ color: "var(--accent)" }}>{sessione}</strong> — Gruppo #{g.idGruppo}
      </div>

      {err && <p style={{ color: "#f87171", fontSize: 13, marginBottom: 12 }}>{err}</p>}

      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 20, marginBottom: 20 }}>
        {/* Membri */}
        <div className="card" style={{ padding: 0 }}>
          <div style={secHeader}>
            <h3 style={{ fontSize: 16, fontWeight: 600 }}>Membri ({g.numeroMembri}/{g.dimensione})</h3>
            <span style={g.numeroMembri >= g.dimensione ? badge("#22c55e") : badge("#eab308")}>
              {g.numeroMembri >= g.dimensione ? "Completo" : "In formazione"}
            </span>
          </div>
          <div style={{ padding: 20 }}>
            {g.membri.map((m) => (
              <div key={m.studenteEmail} style={{ display: "flex", alignItems: "center", gap: 12,
                padding: "10px 0", borderBottom: "1px solid var(--border)" }}>
                <div style={av(coloreAvatar(m.studenteEmail))}>{iniziali(m.studenteEmail)}</div>
                <div style={{ flex: 1 }}>
                  <div style={{ fontSize: 14, fontWeight: 600, color: "#fff" }}>{nomeDaEmail(m.studenteEmail)}</div>
                  <div style={{ fontSize: 12, color: "#64748b" }}>{m.studenteEmail}</div>
                </div>
                {m.isCapogruppo && <span style={badge("#6366f1")}>Capogruppo</span>}
              </div>
            ))}
            {isCapo && idx === 0 && (
              <div style={{ display: "flex", gap: 8, marginTop: 12 }}>
                <input type="email" placeholder="Invita: email@studio.unibo.it" value={invitoEmail}
                  onChange={(e) => setInvitoEmail(e.target.value)} style={inp} />
                <button className="btn-primary" style={{ padding: "8px 16px" }}
                  disabled={!invitoEmail.trim()}
                  onClick={() => run(() => invita(idGruppo, invitoEmail.trim()).then(() => setInvitoEmail("")), "Invito non riuscito")}>
                  Invita
                </button>
              </div>
            )}
          </div>
        </div>

        {/* Timeline */}
        <div className="card" style={{ padding: 0 }}>
          <div style={secHeader}><h3 style={{ fontSize: 16, fontWeight: 600 }}>Stato del Gruppo</h3></div>
          <div style={{ padding: 20 }}>
            {steps.map((s, i) => (
              <div key={i} style={{ display: "flex", gap: 14, alignItems: "center", padding: "8px 0" }}>
                <div style={dot(s.done ? "#22c55e" : s.active ? "#6366f1" : "#334155", s.done || s.active ? "#fff" : "#64748b")}>
                  {s.done ? "✓" : i + 1}
                </div>
                <div style={{ fontSize: 14, color: s.done || s.active ? "#e2e8f0" : "#64748b" }}>{s.t}</div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Richieste di accesso (capo, NUOVO) */}
      {isCapo && idx === 0 && (
        <div className="card" style={{ padding: 0, marginBottom: 20 }}>
          <div style={secHeader}><h3 style={{ fontSize: 16, fontWeight: 600 }}>Richieste di Accesso</h3></div>
          <div style={{ padding: 20 }}>
            {accessi.length === 0 && <p style={{ color: "var(--muted)", fontSize: 13 }}>Nessuna richiesta in attesa.</p>}
            {accessi.map((a) => (
              <div key={a.idRichiesta} style={{ display: "flex", justifyContent: "space-between",
                alignItems: "center", padding: "10px 0", borderBottom: "1px solid var(--border)" }}>
                <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                  <div style={av(coloreAvatar(a.studenteEmail), 28)}>{iniziali(a.studenteEmail)}</div>
                  <div>
                    <div style={{ fontSize: 14, fontWeight: 600 }}>{nomeDaEmail(a.studenteEmail)}</div>
                    <div style={{ fontSize: 12, color: "#64748b" }}>{a.studenteEmail}</div>
                  </div>
                </div>
                <div style={{ display: "flex", gap: 6 }}>
                  <button style={btn("#16a34a")} onClick={() => run(() => accettaAccesso(a.idRichiesta), "Errore")}>Accetta</button>
                  <button style={btn("#dc2626")} onClick={() => run(() => rifiutaAccesso(a.idRichiesta), "Errore")}>Rifiuta</button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Abstract */}
      <div className="card" style={{ marginBottom: 20 }}>
        <h3 style={{ fontSize: 16, marginBottom: 12 }}>📝 Abstract</h3>
        {idx === 0 && (
          isCapo ? (
            <div>
              <p style={{ fontSize: 13, color: "#64748b", marginBottom: 10 }}>
                Invia la proposta di progetto al docente (dopo l'invio il gruppo non sarà più modificabile).
              </p>
              <textarea value={descr} onChange={(e) => setDescr(e.target.value)}
                placeholder="Descrivi la proposta di progetto del tuo gruppo..." style={ta} />
              <button className="btn-primary" style={{ marginTop: 10 }} disabled={!descr.trim()}
                onClick={() => run(() => submitAbstract(idGruppo, descr.trim()).then(() => setDescr("")), "Invio non riuscito")}>
                Invia Abstract
              </button>
            </div>
          ) : <p style={{ color: "var(--muted)", fontSize: 13 }}>In attesa che il capogruppo invii l'abstract.</p>
        )}
        {idx === 1 && abs && (
          <div>
            {abs.esito === false
              ? <span style={pill("#eab308")}>✎ Da rivedere: {abs.modificheProposte}</span>
              : <span style={pill("#eab308")}>⏳ In attesa di valutazione dal docente</span>}
            <div style={absBox}>{abs.descrizione}<div style={{ fontSize: 11, color: "#64748b", marginTop: 8 }}>
              Inviato il {new Date(abs.dataOraConsegna).toLocaleString("it-IT")}</div></div>
            {isCapo && abs.esito === false && (
              <div style={{ marginTop: 12 }}>
                <textarea value={descr} onChange={(e) => setDescr(e.target.value)}
                  placeholder="Revisiona l'abstract..." style={ta} />
                <button className="btn-primary" style={{ marginTop: 10 }} disabled={!descr.trim()}
                  onClick={() => run(() => submitAbstract(idGruppo, descr.trim()).then(() => setDescr("")), "Invio non riuscito")}>
                  Riconsegna Abstract
                </button>
              </div>
            )}
          </div>
        )}
        {idx >= 2 && (
          <div>
            <span style={pill("#22c55e")}>✓ Abstract approvato dal docente</span>
            {abs && <div style={absBox}>{abs.descrizione}</div>}
          </div>
        )}
      </div>

      {/* Progetto */}
      {idx >= 2 && (
        <div className="card" style={{ marginBottom: 20 }}>
          <h3 style={{ fontSize: 16, marginBottom: 12 }}>📦 Progetto</h3>
          {idx === 2 && (
            isCapo ? (
              <div style={{ display: "flex", gap: 10, alignItems: "center", flexWrap: "wrap" }}>
                <input placeholder="nome-progetto.zip" value={fileZip}
                  onChange={(e) => setFileZip(e.target.value)} style={inp} />
                <button className="btn-primary" disabled={!fileZip.trim()}
                  onClick={() => run(() => uploadProgetto(idGruppo, fileZip.trim()).then(() => setFileZip("")), "Upload non riuscito")}>
                  Consegna Progetto
                </button>
              </div>
            ) : <p style={{ color: "var(--muted)", fontSize: 13 }}>In attesa che il capogruppo carichi il progetto.</p>
          )}
          {idx === 3 && prog && (
            <div>
              <span style={pill("#eab308")}>⏳ Progetto consegnato — in attesa di valutazione</span>
              <div style={absBox}><b>{prog.fileZip}</b><div style={{ fontSize: 11, color: "#64748b", marginTop: 6 }}>
                Consegnato il {new Date(prog.dataOraConsegna).toLocaleString("it-IT")}</div></div>
            </div>
          )}
          {idx === 4 && prog && (
            <div style={{ textAlign: "center", padding: 16 }}>
              <div style={{ fontSize: 44, fontWeight: 700, color: "#22c55e" }}>
                {prog.valutazione !== null && prog.valutazione > 30 ? "30 e lode" : `${prog.valutazione}/30`}
              </div>
              <div style={{ fontSize: 13, color: "var(--muted)", marginTop: 4 }}>Valutazione assegnata dal docente</div>
              {prog.accettazione === null ? (
                isCapo ? (
                  <div style={{ display: "flex", gap: 12, justifyContent: "center", marginTop: 16 }}>
                    <button style={{ ...btn("#16a34a"), padding: "10px 24px" }}
                      onClick={() => run(() => accettaVoto(idGruppo), "Errore")}>Accetta Voto</button>
                    <button style={{ ...btn("#dc2626"), padding: "10px 24px", background: "transparent",
                      border: "1px solid #dc2626", color: "#dc2626" }}
                      onClick={() => run(() => rifiutaVoto(idGruppo), "Errore")}>Rifiuta Voto</button>
                  </div>
                ) : <p style={{ color: "var(--muted)", fontSize: 13, marginTop: 12 }}>In attesa della decisione del capogruppo.</p>
              ) : (
                <div style={{ marginTop: 12 }}>
                  <span style={pill(prog.accettazione ? "#22c55e" : "#ef4444")}>
                    {prog.accettazione ? "Voto accettato" : "Voto rifiutato"}
                  </span>
                </div>
              )}
            </div>
          )}
        </div>
      )}

      {/* Abbandona (solo NUOVO) */}
      {idx === 0 && (
        <button onClick={abbandona} style={{ padding: "10px 16px", background: "transparent",
          color: "#ef4444", border: "1px solid #ef4444", borderRadius: 8, cursor: "pointer", fontSize: 13 }}>
          Abbandona Gruppo
        </button>
      )}
    </DashboardLayout>
  );
}

const secHeader: CSSProperties = {
  padding: "16px 20px", borderBottom: "1px solid var(--border)",
  display: "flex", justifyContent: "space-between", alignItems: "center",
};
const inp: CSSProperties = {
  flex: 1, padding: "8px 12px", background: "var(--bg)", border: "1px solid var(--border)",
  borderRadius: 6, color: "var(--text)", fontSize: 13, outline: "none", minWidth: 200,
};
const ta: CSSProperties = {
  width: "100%", padding: "12px 14px", background: "var(--bg)", border: "1px solid var(--border)",
  borderRadius: 8, color: "var(--text)", fontSize: 14, fontFamily: "inherit", resize: "vertical", minHeight: 90, outline: "none",
};
const absBox: CSSProperties = {
  background: "var(--bg)", borderRadius: 8, padding: 14, fontSize: 13, color: "#e2e8f0", marginTop: 10,
};
function badge(color: string): CSSProperties {
  return { fontSize: 11, padding: "3px 8px", borderRadius: 20, fontWeight: 600, color, background: `${color}26` };
}
function pill(color: string): CSSProperties {
  return { display: "inline-flex", alignItems: "center", gap: 6, padding: "6px 14px",
    background: `${color}1a`, border: `1px solid ${color}40`, borderRadius: 8, color, fontSize: 13, fontWeight: 600 };
}
function av(bg: string, size = 36): CSSProperties {
  return { width: size, height: size, borderRadius: "50%", background: bg, display: "flex",
    alignItems: "center", justifyContent: "center", fontSize: size < 32 ? 10 : 13, fontWeight: 700, color: "#fff", flexShrink: 0 };
}
function dot(bg: string, color: string): CSSProperties {
  return { width: 28, height: 28, borderRadius: "50%", background: bg, color, display: "flex",
    alignItems: "center", justifyContent: "center", fontSize: 12, flexShrink: 0 };
}
function btn(bg: string): CSSProperties {
  return { background: bg, color: "#fff", border: "none", padding: "6px 12px", borderRadius: 6, fontSize: 12, cursor: "pointer", fontWeight: 600 };
}
