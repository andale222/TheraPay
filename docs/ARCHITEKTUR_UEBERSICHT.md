# TheraPay - Architekturuebersicht (Stand: 12.03.2026)

## 1. Ziel dieser Datei
Diese Datei dient als schnelle Orientierung fuer die Weiterentwicklung:
- Welche Schichten/Projekte existieren
- Welche Funktionen bereits implementiert sind
- Wie die Abhaengigkeiten und Datenfluesse aussehen
- Wo es aktuell technische Luecken oder TODOs gibt

## 2. Solution-Struktur

```text
TheraPay.sln
|- src/
|  |- TheraPay.Domain
|  |- TheraPay.Core
|  |- TheraPay.Infrastructure/
|  |  |- csv
|  |  `- Export
|  `- TheraPay.UI
`- tests/
   |- TheraPay.Domain.Tests
   |- TheraPay.Core.Tests
   |- TheraPay.Infrastructure.csv.Tests
   `- TheraPay.UI.Tests
```

### Projektrollen
- `TheraPay.Domain`
  - reine Domaenenobjekte und Value-Typen
  - keine externen Package-Abhaengigkeiten
- `TheraPay.Core`
  - Use-Cases / Services / Repository-Abstraktionen
  - In-Memory-Repositories als aktuelle Implementierung
  - referenziert `TheraPay.Domain`
- `TheraPay.Infrastructure.csv`
  - Persistenz gegen CSV-Dateien (CsvHelper)
  - referenziert `TheraPay.Core` + `TheraPay.Domain`
- `TheraPay.UI`
  - Avalonia-Desktop-UI im MVVM-Stil
  - Navigation und DI (Microsoft.Extensions.DependencyInjection)
  - referenziert `TheraPay.Core` + `TheraPay.Domain`
- `TheraPay.Infrastructure.Export`
  - derzeit nur Platzhalter (keine funktionale Implementierung)

## 3. Domain-Schicht (`src/TheraPay.Domain`)

### `Patient`
- Eigenschaften: `FirstName`, `LastName`, `ID`
- aktuell nur Basisdaten, keine Validierungslogik im Konstruktor

### `Appointment`
- Eigenschaften: `Id (Guid)`, `Date`, `PatientID`, `DurationInMinutes`, `End`
- Regeln:
  - `SetDuration(int)` erlaubt nur `0..1440` Minuten
  - `OverlapsWith(Appointment)` prueft Zeitueberschneidung

### `PracticeData`
- Stammdaten fuer Praxis (Name, Adresse, IBAN, Steuer-ID etc.)
- momentan mit Platzhalter-Defaultwerten

### `Result`
- leichter Rueckgabetyp fuer Erfolg/Fehler:
  - `Ok` (bool)
  - `Error` (string?)

## 4. Core-Schicht (`src/TheraPay.Core`)

### Abstraktionen
- `IPatientRepository`
- `IAppointmentRepository`
- `IDataPersistence`
- `IPracticeDataStore`

### In-Memory-Repositories
- `InMemoryPatientRepository`
  - `Add` verhindert doppelte IDs
  - `GetAll`, `Count`, `GetPatient(index)`
- `InMemoryAppointmentRepository`
  - `Add`, `GetAll`, `Count`, `GetAppointment(index)`

### Services (Use-Cases)
- `PatientService`
  - `AddPatient(firstName, lastName, id)`
  - `ViewPatients()`
- `AppointmentService`
  - `AddAppointment(date, patientID, duration)`
  - blockiert ueberlappende Termine
  - `GetAppointmentsByDate(date)` filtert tageweise
  - `ViewAppointments()`

### Wichtige Design-Notiz
Die Services sind derzeit gegen konkrete Klassen verdrahtet (`InMemory...Repository`) und nicht gegen die Interfaces. Das macht Austauschbarkeit/Mocking schwerer als noetig.

## 5. Infrastruktur-Schicht CSV (`src/TheraPay.Infrastructure/csv`)

### Generischer CSV-Store
- `CsvStore<TDomain, TRecord>`
  - `SaveAll(IEnumerable<TDomain>)`
  - `LoadAll()`
  - Mapping ueber abstrakte Methoden `ToRecord` / `ToDomain`

### Konkrete Stores
- `CsvPatientStore` fuer `Patient`
- `CsvAppointmentStore` fuer `Appointment`
  - speichert Startzeit als ISO-8601-String (`"o"`)
  - liest per `DateTime.Parse` wieder ein
- `CsvPracticeInfoStore` fuer `PracticeData`

### Persistenz-Orchestrierung
- `CsvDataPersistence : IDataPersistence`
  - `LoadInto(IPatientRepository, IAppointmentRepository)`
  - `SaveFrom(IPatientRepository, IAppointmentRepository)`

### CSV-Records
- `PatientCsvRecord` / `AppointmentCsvRecord` enthalten je ein Feld `IsDeleted`
- Soft-Delete-Logik ist bisher nicht implementiert (Feld wird nicht genutzt)

## 6. UI-Schicht (`src/TheraPay.UI`)

### Technischer Stil
- Avalonia + MVVM
- DI-Container in `Bootstrapper`
- Navigation ueber `NavigationStore` + `NavigationService`
- `RelayCommand` fuer Commands

### Start- und Navigationsfluss
1. `Program.Main` startet Avalonia
2. `App.OnFrameworkInitializationCompleted` baut DI-Container
3. `MainWindowViewModel` navigiert initial zu `HomeViewModel`
4. `App.axaml` mappt ViewModels auf Views per DataTemplates

### ViewModels (vereinfacht)
- `HomeViewModel`
  - kombiniert `PatientPanel` + `CalendarPanel`
  - Navigation zu Patientenverwaltung und Terminbearbeitung
- `PatientsViewModel`
  - Formular zum Hinzufuegen von Patient:innen
  - Speichern in In-Memory-Repository
- `AppointmentEditViewModel`
  - Terminanlage aus Datum + Start/Endzeit
  - Dauer wird berechnet
  - speichert ueber `AppointmentService`
- `PatientPanelViewModel`
  - Datagrid + Filter-Toggles (teilweise MVP/TODO)
- `CalendarPanelViewModel`
  - Datagrid + Tagesfilter auf Termine

### UI-Reifegrad
- Struktur/Navigation ist funktionsfaehig
- mehrere Bindings/Buttons sind noch Platzhalter (`TODO` oder leere Bindings)
- Teile der Fachdaten im UI sind aktuell nur Placeholder-Felder

## 7. Abhaengigkeiten

### Runtime-Packages
- `CsvHelper` (33.1.0) in `TheraPay.Infrastructure/csv`
- Avalonia 11.3.12 Pakete in `TheraPay.UI`
- `CommunityToolkit.Mvvm` (8.4.0) in `TheraPay.UI`
- `Microsoft.Extensions.DependencyInjection` (10.0.3) in `TheraPay.UI`

### Test-Packages
- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`
- `coverlet.collector`

### Plattform/Framework
- alle Projekte aktuell auf `net10.0`

## 8. Teststatus

Ausgefuehrt am **12.03.2026** mit:

```bash
dotnet test TheraPay.sln
```

Ergebnis:
- `TheraPay.Domain.Tests`: 12/12 bestanden
- `TheraPay.Core.Tests`: 20/20 bestanden
- `TheraPay.Infrastructure.csv.Tests`: 12/12 bestanden
- `TheraPay.UI.Tests`: 1/1 bestanden
- Gesamt: **45/45 bestanden**

Hinweis aus Build:
- Avalonia-Warnung zu `MainWindow.axaml` (Loader-Erreichbarkeit), Build/Test dennoch erfolgreich.

## 9. Bekannte Luecken / Risiken fuer die Weiterentwicklung

1. Service-Kopplung an konkrete In-Memory-Repositories statt Interfaces.
2. Persistenz ist vorhanden, aber in der UI-Bootstrapper-Verdrahtung derzeit nicht aktiv eingebunden.
3. Soft-Delete ist im Datenmodell vorbereitet (`IsDeleted`), aber fachlich nicht umgesetzt.
4. `IPracticeDataStore` und `CsvPracticeInfoStore` sind nicht konsistent (Methodennamen/Interface-Implementierung fehlt).
5. Viele UI-Felder fuer erweiterte Patient-/Terminattribute sind noch nicht im Domain/Core verankert.
6. `TheraPay.Infrastructure.Export` ist noch ein Leerprojekt.
7. UI-Testabdeckung ist aktuell minimal (nur Dummy-Test).

## 10. Sinnvolle naechste Schritte

1. Services auf Interface-basierte Konstruktoren umstellen (`IPatientRepository`, `IAppointmentRepository`).
2. Persistenz-Lifecycle definieren (App-Start laden, beim Beenden speichern).
3. Domain fuer echte Patienten-/Terminattribute erweitern (statt Placeholder im UI).
4. Soft-Delete fachlich durchziehen (Domain, Core, CSV, UI).
5. `PracticeData` sauber ueber Interface anbinden und in UI editierbar machen.
6. UI-Tests fuer kritische Fluesse erhoehen (Navigation, AddPatient, AddAppointment, Validierung).

