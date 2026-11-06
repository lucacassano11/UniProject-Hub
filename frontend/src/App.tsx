import type { ReactNode } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Registrazione from "./pages/Registrazione";
import HomeSessioni from "./pages/HomeSessioni";
import Sessione from "./pages/Sessione";
import HomeGruppi from "./pages/HomeGruppi";
import Gruppo from "./pages/Gruppo";
import Sicurezza from "./pages/Sicurezza";
import "./theme.css";

function Home() {
  const authed = !!sessionStorage.getItem("token");
  const ruolo = sessionStorage.getItem("ruolo");
  if (!authed) return <Navigate to="/login" />;
  if (ruolo === "DOCENTE") return <Navigate to="/sessioni" />;
  if (ruolo === "STUDENTE") return <Navigate to="/gruppi" />;
  if (ruolo === "GESTORE_SICUREZZA") return <Navigate to="/sicurezza" />;
  return (
    <div style={{ display: "flex", alignItems: "center", justifyContent: "center", minHeight: "100vh" }}>
      <div className="card" style={{ textAlign: "center", maxWidth: 440 }}>
        <h1>Uni<span style={{ color: "var(--accent)" }}>Project</span>-Hub</h1>
        <p style={{ color: "var(--muted)", marginTop: 12 }}>Autenticato come {ruolo}</p>
      </div>
    </div>
  );
}

function RequireDocente({ children }: { children: ReactNode }) {
  const authed = !!sessionStorage.getItem("token");
  const ruolo = sessionStorage.getItem("ruolo");
  if (!authed) return <Navigate to="/login" />;
  if (ruolo !== "DOCENTE") return <Navigate to="/" />;
  return <>{children}</>;
}

function RequireStudente({ children }: { children: ReactNode }) {
  const authed = !!sessionStorage.getItem("token");
  const ruolo = sessionStorage.getItem("ruolo");
  if (!authed) return <Navigate to="/login" />;
  if (ruolo !== "STUDENTE") return <Navigate to="/" />;
  return <>{children}</>;
}

function RequireGestore({ children }: { children: ReactNode }) {
  const authed = !!sessionStorage.getItem("token");
  const ruolo = sessionStorage.getItem("ruolo");
  if (!authed) return <Navigate to="/login" />;
  if (ruolo !== "GESTORE_SICUREZZA") return <Navigate to="/" />;
  return <>{children}</>;
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/registrazione" element={<Registrazione />} />
        <Route path="/sessioni" element={<RequireDocente><HomeSessioni /></RequireDocente>} />
        <Route path="/sessioni/:id" element={<RequireDocente><Sessione /></RequireDocente>} />
        <Route path="/gruppi" element={<RequireStudente><HomeGruppi /></RequireStudente>} />
        <Route path="/gruppi/:id" element={<RequireStudente><Gruppo /></RequireStudente>} />
        <Route path="/sicurezza" element={<RequireGestore><Sicurezza /></RequireGestore>} />
        <Route path="/" element={<Home />} />
      </Routes>
    </BrowserRouter>
  );
}
