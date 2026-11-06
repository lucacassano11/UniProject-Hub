import { useState, type CSSProperties, type FormEvent } from "react";
import { useNavigate, Link } from "react-router-dom";
import { login } from "../api/auth";
import { sessioneEsiste } from "../api/gruppi";
import { Field } from "../components/Field";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [codice, setCodice] = useState("");
  const [error, setError] = useState("");
  const nav = useNavigate();

  const isStudente = email.includes("@studio.unibo.it");

  async function submit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError("");
    if (isStudente && codice.trim().length === 0) {
      setError("Inserisci il codice sessione fornito dal docente");
      return;
    }
    try {
      const res = await login(email, password);

      // Studente: valida il codice sessione prima di entrare
      if (res.ruolo === "STUDENTE") {
        if (res.token) sessionStorage.setItem("token", res.token);
        const code = codice.trim().toUpperCase();
        const { esiste } = await sessioneEsiste(code);
        if (!esiste) {
          sessionStorage.clear();
          setError("Codice sessione non valido");
          return;
        }
        sessionStorage.setItem("sessione", code);
      } else if (res.token) {
        sessionStorage.setItem("token", res.token);
      }

      sessionStorage.setItem("ruolo", res.ruolo);
      sessionStorage.setItem("email", res.email);
      nav("/");
    } catch (err) {
      sessionStorage.removeItem("token");
      setError(err instanceof Error ? err.message : "Credenziali non valide o account bloccato");
    }
  }

  return (
    <div style={center}>
      <form className="card" style={{ width: "100%", maxWidth: 440 }} onSubmit={submit}>
        <h1 style={{ textAlign: "center", marginBottom: 8 }}>
          Uni<span style={{ color: "var(--accent)" }}>Project</span>-Hub
        </h1>
        <p style={{ textAlign: "center", color: "var(--muted)", fontSize: 14, marginBottom: 24 }}>
          Accedi al tuo account
        </p>
        <Field label="Email" type="email" value={email} onChange={setEmail}
          placeholder="nome.cognome@studio.unibo.it" />
        <Field label="Password" type="password" value={password} onChange={setPassword}
          placeholder="Inserisci la password" />
        {isStudente && (
          <div style={{ marginBottom: 18 }}>
            <label style={labelStyle}>Codice Sessione</label>
            <input value={codice} maxLength={8}
              onChange={(e) => setCodice(e.target.value.toUpperCase())}
              placeholder="ES: AB3XK9MQ"
              style={{ ...inputStyle, letterSpacing: 2, textTransform: "uppercase" }} />
            <div style={{ fontSize: 12, color: "#64748b", marginTop: 6 }}>
              Inserisci il codice fornito dal docente per accedere alla sessione
            </div>
          </div>
        )}
        {error && <p style={{ color: "#f87171", fontSize: 13, marginBottom: 12 }}>{error}</p>}
        <button className="btn-primary" style={{ width: "100%" }} type="submit">Accedi</button>
        <p style={{ textAlign: "center", marginTop: 16, fontSize: 13, color: "var(--muted)" }}>
          Non hai un account? <Link to="/registrazione" style={{ color: "var(--accent)" }}>Registrati</Link>
        </p>
      </form>
    </div>
  );
}

const center: CSSProperties = {
  display: "flex", alignItems: "center", justifyContent: "center", minHeight: "100vh",
};
const labelStyle: CSSProperties = {
  display: "block", fontSize: 13, fontWeight: 600, color: "var(--muted)",
  marginBottom: 6, textTransform: "uppercase", letterSpacing: 0.5,
};
const inputStyle: CSSProperties = {
  width: "100%", padding: "12px 16px", background: "var(--bg)",
  border: "1px solid var(--border)", borderRadius: 8, color: "var(--text)",
  fontSize: 15, outline: "none",
};
