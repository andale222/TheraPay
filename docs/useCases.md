# TheraPay - Use-cases (Stand: 31.05.2026)
Diese Datei dient als Orientierung für die Fähigkeiten, die das Programm erfüllen soll.


## UC-001 Patient anlegen

### Ziel
Ein neuer Patient wird im System erfasst.

### Akteur
Praxisnutzer

### Vorbedingungen
- Anwendung ist gestartet.
- Datenbestand ist geladen.
- Patient existiert noch nicht mit derselben internen ID.

### Hauptablauf
1. Nutzer öffnet die Patientenverwaltung.
2. Nutzer wählt „Patient anlegen“.
3. Nutzer gibt Stammdaten ein, z. B. Vorname, Nachname, Geburtsdatum, Kontaktdaten.
4. Nutzer fügt den Patienten hinzu.
5. System validiert die Eingaben.
6. System erzeugt oder übernimmt eine eindeutige Patient-ID.
7. System speichert den Patienten im aktuellen Datenbestand.
8. System zeigt den Patienten in der Patientenliste an.

### Alternativen / Fehlerfälle
- Pflichtfelder fehlen → System zeigt Validierungsfehler.
- Patient mit gleicher ID existiert bereits → System lehnt hinzufügen ab.
- Speichern schlägt fehl → System zeigt Fehlermeldung.

### Akzeptanzkriterien
- Ein gültiger Patient kann angelegt werden.
- Der Patient erscheint nach dem hinzufügen in der Patientenliste.
- Ungültige Eingaben werden nicht gespeichert.
- Jeder Patient besitzt eine eindeutige ID.


---

## UC-002 Termin anlegen

### Ziel
Ein neuer Behandlungstermin wird für einen Patienten angelegt.

### Akteur
Praxisnutzer

### Vorbedingungen
- Anwendung ist gestartet.
- Datenbestand ist geladen.
- Mindestens ein Patient existiert.
- Der gewünschte Zeitraum ist verfügbar.

### Hauptablauf
1. Nutzer öffnet Kalender oder Terminverwaltung.
2. Nutzer wählt Datum und Uhrzeit.
3. Nutzer wählt einen Patienten aus.
4. Nutzer gibt Dauer oder Endzeit ein.
5. Nutzer speichert den Termin.
6. System prüft, ob der Termin gültig ist.
7. System prüft, ob es eine Terminüberschneidung gibt.
8. System speichert den Termin.
9. System zeigt den Termin im Kalender oder in der Tagesliste an.

### Alternativen / Fehlerfälle
- Kein Patient ausgewählt → System zeigt Fehler.
- Dauer ist ungültig → System zeigt Fehler.
- Termin überschneidet sich mit vorhandenem Termin → System lehnt den Termin ab.
- Datum liegt außerhalb erlaubter Grenzen → System zeigt Fehler.

### Akzeptanzkriterien
- Ein gültiger Termin kann angelegt werden.
- Überschneidende Termine werden verhindert.
- Der Termin ist nach dem Speichern sichtbar.
- Der Termin ist eindeutig einem Patienten zugeordnet.


---

## UC-003 Termin ändern

### Ziel
Ein bestehender Termin wird angepasst.

### Akteur
Praxisnutzer

### Vorbedingungen
- Anwendung ist gestartet.
- Datenbestand ist geladen.
- Der zu ändernde Termin existiert.

### Hauptablauf
1. Nutzer öffnet die Terminübersicht.
2. Nutzer wählt einen bestehenden Termin aus.
3. Nutzer ändert Datum, Uhrzeit, Dauer, Patient oder weitere Angaben.
4. Nutzer speichert die Änderung.
5. System validiert die neuen Angaben.
6. System prüft erneut auf Terminüberschneidungen.
7. System aktualisiert den Termin.
8. System zeigt den geänderten Termin an.

### Alternativen / Fehlerfälle
- Termin existiert nicht mehr → System zeigt Fehler.
- Neue Zeit überschneidet sich mit anderem Termin → System lehnt Änderung ab.
- Ungültige Dauer oder Uhrzeit → System zeigt Validierungsfehler.
- Nutzer bricht Bearbeitung ab → Termin bleibt unverändert.

### Akzeptanzkriterien
- Ein Termin kann geändert werden.
- Ungültige Änderungen werden verhindert.
- Nach erfolgreicher Änderung ist nur die neue Version sichtbar.
- Nicht gespeicherte Änderungen verändern den Termin nicht.


---

## UC-004 Termin löschen

### Ziel
Ein bestehender Termin wird entfernt oder als gelöscht markiert.

### Akteur
Praxisnutzer

### Vorbedingungen
- Anwendung ist gestartet.
- Datenbestand ist geladen.
- Termin existiert.
- Termin ist noch nicht abgerechnet oder das Löschen abgerechneter Termine ist fachlich erlaubt.

### Hauptablauf
1. Nutzer öffnet die Terminübersicht.
2. Nutzer wählt einen Termin aus.
3. Nutzer klickt auf „Löschen“.
4. System fragt nach Bestätigung.
5. Nutzer bestätigt.
6. System löscht den Termin oder markiert ihn als gelöscht.
7. System entfernt den Termin aus der normalen Terminansicht.

### Alternativen / Fehlerfälle
- Nutzer bricht Bestätigung ab → Termin bleibt erhalten.
- Termin ist bereits abgerechnet → System verhindert Löschung oder verlangt Sonderbestätigung.
- Speichern schlägt fehl → System zeigt Fehlermeldung.

### Akzeptanzkriterien
- Ein nicht abgerechneter Termin kann gelöscht werden.
- Gelöschte Termine erscheinen nicht mehr in der normalen Terminliste.
- Der Nutzer muss das Löschen bestätigen.
- Abgerechnete Termine werden nicht versehentlich gelöscht.


---

## UC-005 Termine abrechnen / Rechnung erstellen

### Ziel
Aus einem oder mehreren Terminen wird eine Rechnung erstellt.

### Akteur
Praxisnutzer

### Vorbedingungen
- Anwendung ist gestartet.
- Datenbestand ist geladen.
- Patient existiert.
- Mindestens ein abrechenbarer Termin existiert.
- Für die Abrechnung sind notwendige GOP-/Leistungsdaten vorhanden.

### Hauptablauf
1. Nutzer öffnet die Abrechnung.
2. Nutzer wählt einen Patienten aus.
3. System zeigt abrechenbare Termine des Patienten an.
4. Nutzer wählt die abzurechnenden Termine aus.
5. Nutzer prüft oder ergänzt GOP-/Leistungspositionen.
6. Nutzer klickt auf „Rechnung erstellen“.
7. System erstellt eine Rechnung mit Rechnungsnummer, Datum, Patientendaten und Rechnungspositionen.
8. System markiert die ausgewählten Termine als abgerechnet.
9. System zeigt die erstellte Rechnung an.

### Alternativen / Fehlerfälle
- Keine abrechenbaren Termine vorhanden → System zeigt Hinweis.
- Termin wurde bereits abgerechnet → System verhindert doppelte Abrechnung.
- GOP fehlt → System verlangt Ergänzung.
- Rechnungsnummer kann nicht erzeugt werden → System zeigt Fehler.
- Nutzer bricht ab → Es wird keine Rechnung erstellt.

### Akzeptanzkriterien
- Aus gültigen Terminen kann eine Rechnung erstellt werden.
- Bereits abgerechnete Termine werden nicht erneut abgerechnet.
- Die Rechnung enthält Patient, Datum, Positionen, Beträge und Gesamtsumme.
- Abgerechnete Termine sind danach entsprechend markiert.


---

## UC-006 Rechnung als PDF exportieren

### Ziel
Eine bestehende Rechnung wird als PDF-Datei exportiert.

### Akteur
Praxisnutzer

### Vorbedingungen
- Anwendung ist gestartet.
- Eine Rechnung existiert.
- Speicherort ist verfügbar.
- Rechnungsdaten sind vollständig.

### Hauptablauf
1. Nutzer öffnet die Rechnungsübersicht.
2. Nutzer wählt eine Rechnung aus.
3. Nutzer klickt auf „Als PDF exportieren“.
4. System fragt Speicherort und Dateiname ab oder verwendet einen Standardpfad.
5. System erzeugt ein PDF mit Rechnungsdaten.
6. System speichert die PDF-Datei.
7. System bestätigt den erfolgreichen Export.

### Alternativen / Fehlerfälle
- Rechnung existiert nicht → System zeigt Fehler.
- Speicherort ist nicht verfügbar → System zeigt Fehler.
- PDF-Erzeugung schlägt fehl → System zeigt Fehler.
- Datei existiert bereits → System fragt nach Überschreiben oder erzeugt neuen Dateinamen.

### Akzeptanzkriterien
- Eine Rechnung kann als PDF exportiert werden.
- Das PDF enthält alle relevanten Rechnungsdaten.
- Der Exportpfad ist nachvollziehbar.
- Fehler beim Export werden angezeigt.


---

## UC-007 Daten laden

### Ziel
Beim Start oder auf Nutzeraktion werden gespeicherte Praxisdaten geladen.

### Akteur
Praxisnutzer / System

### Vorbedingungen
- Anwendung wird gestartet oder Nutzer wählt „Daten laden“.
- Speicherpfad ist bekannt oder wird ausgewählt.
- Datendateien existieren oder es wird ein leerer Datenbestand erzeugt.

### Hauptablauf
1. System ermittelt den aktuellen Speicherpfad.
2. System lädt Patienten, Termine, Rechnungen, Praxisdaten und ggf. GOP-Daten.
3. System validiert die geladenen Daten.
4. System stellt die Daten den Services und der UI bereit.
5. System zeigt die geladenen Daten in den jeweiligen Ansichten an.

### Alternativen / Fehlerfälle
- Datei existiert nicht → System startet mit leerem Datenbestand oder zeigt Hinweis.
- Datei ist beschädigt → System zeigt Fehler.
- Datenformat ist veraltet → System führt Migration durch oder lehnt Laden ab.
- Zugriff verweigert → System zeigt Fehler.

### Akzeptanzkriterien
- Gültige Daten können geladen werden.
- Die UI zeigt nach dem Laden den geladenen Zustand.
- Fehlende oder defekte Dateien führen nicht zu einem unkontrollierten Absturz.
- Der geladene Datenbestand ist konsistent.


---

## UC-008 Daten speichern

### Ziel
Der aktuelle Datenbestand wird dauerhaft gespeichert.

### Akteur
Praxisnutzer / System

### Vorbedingungen
- Anwendung ist gestartet.
- Es existiert ein aktueller Datenbestand.
- Speicherpfad ist bekannt oder wird ausgewählt.

### Hauptablauf
1. Nutzer klickt auf „Speichern“ oder System speichert automatisch beim Beenden.
2. System sammelt aktuelle Patienten, Termine, Rechnungen, Praxisdaten und GOP-Daten.
3. System validiert den Datenbestand.
4. System schreibt die Daten in die Speicherdateien.
5. System bestätigt erfolgreiches Speichern oder beendet die Anwendung.

### Alternativen / Fehlerfälle
- Speicherpfad fehlt → System fragt Speicherort ab.
- Schreibrechte fehlen → System zeigt Fehler.
- Speichern schlägt teilweise fehl → System zeigt Fehler und verhindert Datenverlust.
- Datenbestand ist ungültig → System lehnt Speichern ab oder meldet betroffene Daten.

### Akzeptanzkriterien
- Der aktuelle Zustand kann gespeichert werden.
- Nach Neustart kann derselbe Zustand wieder geladen werden.
- Fehler beim Speichern werden sichtbar gemeldet.
- Speichern überschreibt nicht versehentlich falsche Datenbestände.


---

## UC-009 GOP hinzufügen

### Ziel
Eine GOP-/Leistungsposition wird im System angelegt oder einem Termin bzw. einer Rechnung zugeordnet.

### Akteur
Praxisnutzer

### Vorbedingungen
- Anwendung ist gestartet.
- Datenbestand ist geladen.
- Termin, Rechnung oder GOP-Katalog ist geöffnet.
- GOP-Code oder Leistungsdaten sind bekannt.

### Hauptablauf Variante A: GOP zum Katalog hinzufügen
1. Nutzer öffnet GOP-/Leistungskatalog.
2. Nutzer klickt auf „GOP hinzufügen“.
3. Nutzer gibt Code, Beschreibung, Betrag und ggf. Dauer/Typ ein.
4. Nutzer speichert die GOP.
5. System validiert die Angaben.
6. System fügt die GOP dem Katalog hinzu.

### Hauptablauf Variante B: GOP zu Termin/Rechnung hinzufügen
1. Nutzer öffnet Termin oder Rechnung.
2. Nutzer klickt auf „GOP hinzufügen“.
3. Nutzer wählt eine GOP aus dem Katalog.
4. System übernimmt Beschreibung und Betrag.
5. Nutzer passt optional Menge oder Zusatztext an.
6. System fügt die Position hinzu.
7. System aktualisiert die Summe.

### Alternativen / Fehlerfälle
- GOP-Code fehlt → System zeigt Fehler.
- GOP-Code existiert bereits im Katalog → System verhindert Dublette oder fragt nach Aktualisierung.
- Betrag ist ungültig → System zeigt Fehler.
- Termin ist bereits abgerechnet → Änderung wird verhindert oder als Rechnungskorrektur behandelt.

### Akzeptanzkriterien
- Eine gültige GOP kann angelegt werden.
- Eine GOP kann einem Termin oder einer Rechnung zugeordnet werden.
- Doppelte GOP-Codes werden erkannt.
- Rechnungsbeträge werden nach Hinzufügen korrekt aktualisiert.