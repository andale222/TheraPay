# TheraPay

## Disclaimer
TheraPay is provided "as is" without warranty of any kind. It is not legal, tax, or billing advice. You are responsible for verifying correctness and compliance for your specific use case.

## License
TheraPay is licensed under the MIT License. See `LICENSE`.


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
