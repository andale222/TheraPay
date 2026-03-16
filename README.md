# TheraPay
TheraPay is a free standalone desktop application for creating invoices for psychotherapists in Germany.

Because of the legal and administrative complexity involved, the project focuses on private health insurance, self-paying patients, and statutory health insurance cases handled through the reimbursement model (`Kostenerstattung`).

At its core, TheraPay manages patients, appointments, invoices, practice data, and regulated billing codes (GOP/GOÄ).

At the moment, data is stored in simple CSV-based files. In the future, password protection and encryption should be added. The secure operation of the software and compliance with applicable data protection requirements remain the responsibility of the user.


## Disclaimer
TheraPay is provided "as is", without warranty of any kind. It does not constitute legal, tax, or billing advice. You are responsible for verifying accuracy, correctness, and compliance for your specific use case.

## License
TheraPay is licensed under the MIT License. See `LICENSE` for details.

## Further development documentation
A document summarizing the current state of the project, its architecture, known risks, missing implementations, and possible next steps can be found [here](https://github.com/andale222/TheraPay/blob/main/docs/ARCHITEKTUR_UEBERSICHT.md).


## Creating executables
The program can be compiled as a standalone for Windows using the following command:
```
dotnet publish src/TheraPay.UI/TheraPay.UI.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=false
  ```
