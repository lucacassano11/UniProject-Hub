import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { register } from "../api/auth";
import { Field } from "./Login";

export default function Registrazione() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [conferma, setConferma] = useState("");
  const [ruolo, setRuolo] = useState("STUDENTE");
  const [error, setError] = useState("");
  const nav = useNavigate();

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    if (password !== conferma) { setError("Le password non coincidono"); return; }
    try {
      await register(email, password, ruolo);
      nav("/login");
    } catch (e) {
      setError(e instanceof Error ? e.message : "Registrazione fallita");
    }
  }

  return (
    <div style={{ display: "flex", alignItems: "center", justifyContent: "center", minHeight: "100vh" }}>
      <form className="card" style={{ width: "100%", maxWidth: 440 }} onSubmit={submit}>
        <h1 style={{ textAlign: "center", marginBottom: 24 }}>
          Uni<span style={{ color: "var(--accent)" }}>Project</span>-Hub
        </h1>
        <Field label="Email universitaria" type="email" value={email} onChange={setEmail}
          placeholder="nome.cognome@studio.unibo.it" />
        <Field label="Password" type="password" value={password} onChange={setPassword}
          placeholder="Min 8, maiuscola, minuscola, numero, simbolo" />
        <Field label="Conferma Password" type="password" value={conferma} onChange={setConferma}
          placeholder="Ripeti la password" />
        <div style={{ display: "flex", gap: 12, marginBottom: 20 }}>
          <RoleBtn label="Studente" desc="@studio.unibo.it" active={ruolo === "STUDENTE"}
            onClick={() => setRuolo("STUDENTE")} />
          <RoleBtn label="Docente" desc="@unibo.it" active={ruolo === "DOCENTE"}
            onClick={() => setRuolo("DOCENTE")} />
        </div>
        {error && <p style={{ color: "#f87171", fontSize: 13, marginBottom: 12 }}>{error}</p>}
        <button className="btn-primary" style={{ width: "100%" }} type="submit">Registrati</button>
        <p style={{ textAlign: "center", marginTop: 16, fontSize: 13, color: "var(--muted)" }}>
          Hai gia un account? <Link to="/login" style={{ color: "var(--accent)" }}>Accedi</Link>
        </p>
      </form>
    </div>
  );
}

function RoleBtn(props: { label: string; desc: string; active: boolean; onClick: () => void }) {
  return (
    <div onClick={props.onClick} style={{
      flex: 1, padding: 14, background: "var(--bg)", borderRadius: 10, textAlign: "center",
      cursor: "pointer",
      border: `2px solid ${props.active ? "var(--accent)" : "var(--border)"}`,
    }}>
      <div style={{ fontSize: 14, fontWeight: 600 }}>{props.label}</div>
      <div style={{ fontSize: 11, color: "#64748b", marginTop: 2 }}>{props.desc}</div>
    </div>
  );
}
