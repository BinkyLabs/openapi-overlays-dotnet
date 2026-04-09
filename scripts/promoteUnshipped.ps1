$nullableConstant = "#nullable enable"
$removedPrefix = "*REMOVED*"
$unshippedDocuments = Get-ChildItem -Filter *.Unshipped* -Recurse | Select-Object -ExpandProperty FullName
foreach ($unshippedDocumentPath in $unshippedDocuments) {
    $shippedDocumentPath = $unshippedDocumentPath -replace '\.Unshipped', '.Shipped'
    $unshippedDocumentContent = Get-Content $unshippedDocumentPath -Raw
    $unshippedDocumentContent = ($unshippedDocumentContent -replace [regex]::Escape($nullableConstant), '').Trim()
    if ([string]::IsNullOrWhiteSpace($unshippedDocumentContent)) {
        Write-Host "No content to promote for $unshippedDocumentPath, skipping." -ForegroundColor Yellow
        continue
    }

    # Process removed API entries
    $unshippedLines = $unshippedDocumentContent -split "`n" | ForEach-Object { $_.TrimEnd() }
    $removedLines = $unshippedLines | Where-Object { $_.StartsWith($removedPrefix) }
    if ($removedLines.Count -gt 0) {
        $shippedContent = Get-Content $shippedDocumentPath -Raw
        $shippedLines = $shippedContent -split "`n" | ForEach-Object { $_.TrimEnd() }
        foreach ($removedLine in $removedLines) {
            $apiEntry = $removedLine.Substring($removedPrefix.Length)
            $shippedLines = $shippedLines | Where-Object { $_ -ne $apiEntry }
            Write-Host "Removed '$apiEntry' from $shippedDocumentPath" -ForegroundColor Cyan
        }
        Set-Content -Path $shippedDocumentPath -Value ($shippedLines -join "`n") -NoNewline
        $unshippedLines = $unshippedLines | Where-Object { -not $_.StartsWith($removedPrefix) }
        $unshippedDocumentContent = ($unshippedLines -join "`n").Trim()
    }

    if ([string]::IsNullOrWhiteSpace($unshippedDocumentContent)) {
        Write-Host "No remaining content to promote for $unshippedDocumentPath after processing removals." -ForegroundColor Yellow
        Set-Content -Path $unshippedDocumentPath -Value $nullableConstant -Verbose
        continue
    }
    Add-Content -Path $shippedDocumentPath -Value $unshippedDocumentContent -Verbose
    Set-Content -Path $unshippedDocumentPath -Value $nullableConstant -Verbose
}