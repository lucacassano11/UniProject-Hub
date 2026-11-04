import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import DashboardLayout from "../components/DashboardLayout";
import { getSession, listAppelli, addAppello, deleteAppello, setInizioAbstract,
  type Session, type Appello } from "../api/sessioni";
import { panoramicaConsegne, approvaAbstract, rifiutaAbstract, valutaProgetto,
  type ConsegnaGruppo } from "../api/consegne";

export default function Sessione() {
  const { id = "" } = useParams();
  const [session, setSession] = useState<Session | null>(null);
  const [appelli, setAppelli] = useState<Appello[]>([]);
  const [dataOra, setDataOra] = useState("");
  const [scadenza, setScadenza] = useState("");
  const [inizio, setInizio] = useState("");
  const [consegne, setConsegne] = useState<ConsegnaGruppo[]>([]);

  async function load() {
    setSession(await getSession(id));
    setAppelli(await listAppelli(id));
    setConsegne(await panoramicaConsegne(id).catch(() => []));
  }
  useEffect(() => { load().catch(() => {}); }, [id]);

  async function aggiungiAppello() {
    if (!dataOra || !scadenza) return;
    await addAppello(id, dataOra, scadenza);
    setDataOra(""); setScadenza("");
    await load();
  }
  async function rimuoviAppello(idA: number) { await deleteAppello(idA); await load(); }
  async function salvaInizio() { if (inizio) { await setInizioAbstract(id, inizio); await load(); } }

  if (!session) return <DashboardLayout active="sessioni"><p style={{ color: "var(--muted)" }}>Caricamento…</p></DashboardLayout>;

  return (
    <DashboardLayout active="sessioni">
      <div style={{ fontSize: 13, color: "#64748b", marginBottom: 8 }}>
        <Link to="/sessioni" style={{ color: "var(--accent)" }}>Sessioni</Link> / {session.idSessione}
      </div>
      <h2 style={{ fontSize: 24, fontWeight: 700, marginBottom: 20 }}>Sessione {session.idSessione}</h2>

      <div style={{ display: "flex", gap: 24, marginBottom: 24, flexWrap: "wrap" }}>
        <Info val={session.votoMassimo} lbl="Voto Max" />
        <Info val={session.numeroAppelli} lbl="Appelli" />
      </div>

      <div className="card" style={{ marginBottom: 20 }}>
        <h3 style={{ fontSize: 16, marginBottom: 12 }}>Inizio consegna abstract</h3>
        <div style={{ display: "flex", gap: 10, alignItems: "center" }}>
          <span style={{ color: "var(--muted)", fontSize: 13 }}>
            Attuale: {session.inizioInvioAbstract ?? "non impostata"}
          </span>
          <input type="date" value={inizio} onChange={(e) => setInizio(e.target.value)} style={inputStyle} />
          <button className="btn-primary" onClick={salvaInizio}>Salva</button>
        </div>
      </div>

      <div className="card" style={{ marginBottom: 20 }}>
        <h3 style={{ fontSize: 16, marginBottom: 12 }}>Appelli</h3>
        {appelli.map((a) => (
          <div key={a.idAppello} style={{ display: "flex", justifyContent: "space-between",
            alignItems: "center", padding: "8px 0", borderTop: "1px solid var(--border)" }}>
            <span>{new Date(a.dataOraAppello).toLocaleString("it-IT")} — scadenza {a.scadenzaConsegnaProgetto}</span>
            <button onClick={() => rimuoviAppello(a.idAppello)} style={{ background: "none", border: "none",
              color: "#ef4444", cursor: "pointer" }}>🗑</button>
          </div>
        ))}
        {appelli.length === 0 && <p style={{ color: "var(--muted)", fontSize: 13 }}>Nessun appello.</p>}
        <div style={{ display: "flex", gap: 10, marginTop: 16, flexWrap: "wrap", alignItems: "center" }}>
          <input type="datetime-local" value={dataOra} onChange={(e) => setDataOra(e.target.value)} style={inputStyle} />
          <input type="date" value={scadenza} onChange={(e) => setScadenza(e.target.value)} style={inputStyle} />
          <button className="btn-primary" onClick={aggiungiAppello}>+ Aggiungi Appello</button>
        </div>
      </div>

      <div className="card">
        <h3 style={{ fontSize: 16, marginBottom: 12 }}>Consegne dei gruppi</h3>
        {consegne.length === 0 && <p style={{ color: "var(--muted)", fontSize: 13 }}>Nessun gruppo in questa sessione.</p>}
        {consegne.map((c) => (
          <ConsegnaRow key={c.idGruppo} c={c} onReload={load} />
        ))}
      </div>
    </DashboardLayout>
  );
}

function ConsegnaRow({ c, onReload }: { c: ConsegnaGruppo; onReload: () => Promise<void> }) {
  const [modifiche, setModifiche] = useState("");
  const [voto, setVoto] = useState(30);

  async function approva() { await approvaAbstract(c.idGruppo); await onReload(); }
  async function rifiuta() { if (modifiche.trim()) { await rifiutaAbstract(c.idGruppo, modifiche.trim()); setModifiche(""); await onReload(); } }
  async function valuta() { await valutaProgetto(c.idGruppo, voto); await onReload(); }

  return (
    <div style={{ padding: "12px 0", borderTop: "1px solid var(--border)" }}>
      <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 6 }}>
        <b>Gruppo #{c.idGruppo}</b>
        <span style={{ fontSize: 12, color: "var(--muted)" }}>{c.stato}</span>
      </div>

      {c.abstract ? (
        <div style={{ fontSize: 13, marginBottom: 8 }}>
          <div style={{ color: "var(--muted)" }}>Abstract: {c.abstract.descrizione}</div>
          {c.abstract.esito === null ? (
            <div style={{ display: "flex", gap: 8, marginTop: 8, flexWrap: "wrap", alignItems: "center" }}>
              <button className="btn-primary" onClick={approva}>Approva abstract</button>
              <input placeholder="modifiche proposte" value={modifiche}
                onChange={(e) => setModifiche(e.target.value)} style={inputStyle} />
              <button onClick={rifiuta} style={{ padding: "8px 14px", background: "var(--border)",
                color: "var(--text)", border: "none", borderRadius: 8, cursor: "pointer" }}>Rifiuta</button>
            </div>
          ) : <span style={{ color: c.abstract.esito ? "#22c55e" : "#f59e0b" }}>
            {c.abstract.esito ? "approvato" : "da rivedere"}</span>}
        </div>
      ) : <div style={{ fontSize: 13, color: "var(--muted)", marginBottom: 8 }}>Abstract non consegnato.</div>}

      {c.progetto && (
        <div style={{ fontSize: 13 }}>
          <div style={{ color: "var(--muted)" }}>Progetto: {c.progetto.fileZip}</div>
          {c.progetto.valutazione === null ? (
            <div style={{ display: "flex", gap: 8, marginTop: 8, alignItems: "center" }}>
              <input type="number" min={1} max={33} value={voto}
                onChange={(e) => setVoto(Number(e.target.value))} style={{ ...inputStyle, minWidth: 90 }} />
              <button className="btn-primary" onClick={valuta}>Valuta</button>
            </div>
          ) : <span>Voto: <b>{c.progetto.valutazione}</b>
            {c.progetto.accettazione !== null ? ` · ${c.progetto.accettazione ? "accettato" : "rifiutato"}` : " · in attesa di accettazione"}</span>}
        </div>
      )}
    </div>
  );
}

function Info({ val, lbl }: { val: number | string; lbl: string }) {
  return (
    <div style={{ background: "var(--panel)", borderRadius: 10, padding: 16, textAlign: "center",
      border: "1px solid var(--border)", minWidth: 120 }}>
      <div style={{ fontSize: 24, fontWeight: 700, color: "#fff" }}>{val}</div>
      <div style={{ fontSize: 11, color: "#64748b", textTransform: "uppercase", marginTop: 4 }}>{lbl}</div>
    </div>
  );
}

const inputStyle: React.CSSProperties = {
  padding: "8px 12px", background: "var(--bg)", border: "1px solid var(--border)",
  borderRadius: 8, color: "var(--text)", fontSize: 13, outline: "none",
};
