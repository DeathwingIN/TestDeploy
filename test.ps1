$json = Get-Content -Raw -Path "$env:TEMP\SystemApp\SymbolReference.json" | ConvertFrom-Json; $json.Codeunits | Where-Object { $_.Name -match "JWT|Token" } | Select-Object Id, Name
