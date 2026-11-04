CREATE DATABASE IF NOT EXISTS uniprojecthub
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE uniprojecthub;

-- ===== UTENTI (gerarchia disgiunta) =====
CREATE TABLE utenti (
  email         VARCHAR(255) PRIMARY KEY,
  password_hash VARCHAR(255) NOT NULL,
  nome          VARCHAR(120) NOT NULL,
  cognome       VARCHAR(120) NOT NULL,
  ruolo         ENUM('DOCENTE','STUDENTE','GESTORE_SICUREZZA') NOT NULL,
  created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  failed_attempts INT NOT NULL DEFAULT 0,
  locked_until    TIMESTAMP NULL
);

CREATE TABLE studenti (
  email     VARCHAR(255) PRIMARY KEY,
  matricola VARCHAR(20) UNIQUE NOT NULL,
  FOREIGN KEY (email) REFERENCES utenti(email)
);

CREATE TABLE docenti (
  email VARCHAR(255) PRIMARY KEY,
  FOREIGN KEY (email) REFERENCES utenti(email)
);

CREATE TABLE gestori_sicurezza (
  email VARCHAR(255) PRIMARY KEY,
  FOREIGN KEY (email) REFERENCES utenti(email)
);

-- ===== SESSIONE / APPELLO =====
CREATE TABLE sessioni (
  id_sessione            VARCHAR(40) PRIMARY KEY,
  voto_massimo           INT NOT NULL,
  data_creazione_sessione DATE NOT NULL,
  inizio_invio_abstract  DATE NULL,
  docente_email          VARCHAR(255) NOT NULL,
  FOREIGN KEY (docente_email) REFERENCES docenti(email)
);

CREATE TABLE appelli (
  id_appello                INT AUTO_INCREMENT PRIMARY KEY,
  id_sessione               VARCHAR(40) NOT NULL,
  data_ora_appello          TIMESTAMP NOT NULL,
  scadenza_consegna_progetto DATE NOT NULL,
  FOREIGN KEY (id_sessione) REFERENCES sessioni(id_sessione)
);

-- ===== GRUPPO =====
CREATE TABLE gruppi (
  id_gruppo        INT AUTO_INCREMENT PRIMARY KEY,
  id_sessione      VARCHAR(40) NOT NULL,
  dimensione       INT NOT NULL DEFAULT 3,
  stato            ENUM('NUOVO','ABSTRACT_CONSEGNATO','ABSTRACT_VALUTATO','PROGETTO_CONSEGNATO','PROGETTO_VALUTATO') NOT NULL DEFAULT 'NUOVO',
  capogruppo_email VARCHAR(255) NULL,
  FOREIGN KEY (id_sessione) REFERENCES sessioni(id_sessione),
  FOREIGN KEY (capogruppo_email) REFERENCES studenti(email)
);

CREATE TABLE membri_gruppo (
  id_gruppo      INT,
  studente_email VARCHAR(255),
  data_ingresso  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id_gruppo, studente_email),
  FOREIGN KEY (id_gruppo) REFERENCES gruppi(id_gruppo),
  FOREIGN KEY (studente_email) REFERENCES studenti(email)
);

CREATE TABLE iscrizioni_appello (
  id_appello      INT,
  id_gruppo       INT,
  data_iscrizione TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id_appello, id_gruppo),
  FOREIGN KEY (id_appello) REFERENCES appelli(id_appello),
  FOREIGN KEY (id_gruppo) REFERENCES gruppi(id_gruppo)
);

-- ===== RICHIESTE (gerarchia disgiunta) =====
CREATE TABLE richieste (
  id_richiesta INT AUTO_INCREMENT PRIMARY KEY,
  tipo         ENUM('INVITO','ACCESSO','DIMENSIONE_GRUPPO') NOT NULL,
  esito        BOOLEAN NULL,
  created_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE inviti (
  id_richiesta   INT PRIMARY KEY,
  id_gruppo      INT NOT NULL,
  studente_email VARCHAR(255) NOT NULL,
  FOREIGN KEY (id_richiesta) REFERENCES richieste(id_richiesta),
  FOREIGN KEY (id_gruppo) REFERENCES gruppi(id_gruppo),
  FOREIGN KEY (studente_email) REFERENCES studenti(email)
);

CREATE TABLE richieste_accesso (
  id_richiesta   INT PRIMARY KEY,
  id_gruppo      INT NOT NULL,
  studente_email VARCHAR(255) NOT NULL,
  FOREIGN KEY (id_richiesta) REFERENCES richieste(id_richiesta),
  FOREIGN KEY (id_gruppo) REFERENCES gruppi(id_gruppo),
  FOREIGN KEY (studente_email) REFERENCES studenti(email)
);

CREATE TABLE richieste_dimensione_gruppo (
  id_richiesta INT PRIMARY KEY,
  id_sessione  VARCHAR(40) NOT NULL,
  id_gruppo    INT NOT NULL,
  descrizione  TEXT NOT NULL,
  FOREIGN KEY (id_richiesta) REFERENCES richieste(id_richiesta),
  FOREIGN KEY (id_sessione) REFERENCES sessioni(id_sessione),
  FOREIGN KEY (id_gruppo) REFERENCES gruppi(id_gruppo)
);

-- ===== ABSTRACT / PROGETTO =====
CREATE TABLE abstracts (
  id_abstract       INT AUTO_INCREMENT PRIMARY KEY,
  id_gruppo         INT UNIQUE NOT NULL,
  docente_email     VARCHAR(255) NULL,
  descrizione       TEXT NOT NULL,
  data_ora_consegna TIMESTAMP NOT NULL,
  modifiche_proposte TEXT NULL,
  esito             BOOLEAN NULL,
  FOREIGN KEY (id_gruppo) REFERENCES gruppi(id_gruppo),
  FOREIGN KEY (docente_email) REFERENCES docenti(email)
);

CREATE TABLE progetti (
  id_progetto       INT AUTO_INCREMENT PRIMARY KEY,
  id_gruppo         INT UNIQUE NOT NULL,
  docente_email     VARCHAR(255) NULL,
  file_zip          VARCHAR(500) NOT NULL,
  data_ora_consegna TIMESTAMP NOT NULL,
  valutazione       INT NULL,
  accettazione      BOOLEAN NULL,
  FOREIGN KEY (id_gruppo) REFERENCES gruppi(id_gruppo),
  FOREIGN KEY (docente_email) REFERENCES docenti(email)
);

-- ===== NOTIFICHE (polling) =====
CREATE TABLE notifiche (
  id_notifica        INT AUTO_INCREMENT PRIMARY KEY,
  destinatario_email VARCHAR(255) NOT NULL,
  tipo               VARCHAR(50)  NOT NULL,
  messaggio          VARCHAR(500) NOT NULL,
  letta              BOOLEAN NOT NULL DEFAULT FALSE,
  created_at         TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (destinatario_email) REFERENCES utenti(email)
);

-- ===== LOG EVENTI (sicurezza) =====
CREATE TABLE log_eventi (
  id_evento    INT AUTO_INCREMENT PRIMARY KEY,
  tipo         VARCHAR(50)  NOT NULL,
  attore_email VARCHAR(255) NULL,
  dettaglio    VARCHAR(500) NOT NULL,
  created_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ===== DATI DEMO (password per tutti gli account demo: DemoPass1!) =====
INSERT INTO utenti (email, password_hash, nome, cognome, ruolo) VALUES
  ('marco.verdi@studio.unibo.it', '$2a$11$cxpmbw3xB3eqxZoiodmv/eUaQN5QWoUVQ6/RM4rtbUXGQfHvHxmdm', 'Marco', 'Verdi', 'STUDENTE'),
  ('giulia.neri@studio.unibo.it', '$2a$11$p2/QdSFQVX.SlRg1RHBbTO1Z3U9zXzobVD2PREDMXgOCaYn3Sijim', 'Giulia', 'Neri', 'STUDENTE'),
  ('alessio.blu@studio.unibo.it', '$2a$11$dBfpWk0eLsAzO7DFPT1XOuVMEpJTELpuSTHCV5Y4U9jHuh6WEnQ02', 'Alessio', 'Blu', 'STUDENTE'),
  ('mario.rossi@unibo.it', '$2a$11$KQZoX3/a/FAW8KJ0HpKBHepw9nl9qO5mvGkAwT5gGVUimqPQWVUn.', 'Mario', 'Rossi', 'DOCENTE'),
  ('anna.bianchi@unibo.it', '$2a$11$ztm0u2d5BYM15zF2ykV4FeiVerQ8GCQmYRs9nMyBbCQX4ws4A7e8W', 'Anna', 'Bianchi', 'GESTORE_SICUREZZA');

INSERT INTO studenti (email, matricola) VALUES
  ('marco.verdi@studio.unibo.it', 'S000001'),
  ('giulia.neri@studio.unibo.it', 'S000002'),
  ('alessio.blu@studio.unibo.it', 'S000003');

INSERT INTO docenti (email) VALUES ('mario.rossi@unibo.it');
INSERT INTO gestori_sicurezza (email) VALUES ('anna.bianchi@unibo.it');

INSERT INTO sessioni (id_sessione, voto_massimo, data_creazione_sessione, docente_email)
  VALUES ('AB3XK9MQ', 30, '2026-03-01', 'mario.rossi@unibo.it');
