# Quick test script for Docker setup with debug samples

Write-Host "?? Building Docker image..." -ForegroundColor Cyan
docker build -t clio:latest .

Write-Host ""
Write-Host "?? Creating output directory..." -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path output | Out-Null

Write-Host ""
Write-Host "?? Running CLI with debug samples..." -ForegroundColor Cyan
docker run --rm -v ${PWD}/output:/app/output clio:latest `
  apply /app/samples/description.yaml `
  --overlay /app/samples/overlay.yaml `
  -out /app/output/result.yaml

Write-Host ""
Write-Host "? Success! Result written to output/result.yaml" -ForegroundColor Green
Write-Host ""
Write-Host "?? Output content:" -ForegroundColor Yellow
Write-Host "====================" -ForegroundColor Yellow
Get-Content output/result.yaml
Write-Host "====================" -ForegroundColor Yellow
Write-Host ""
Write-Host "? Test complete!" -ForegroundColor Green
