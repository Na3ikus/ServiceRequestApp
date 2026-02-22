Write-Host "Starting ServiceDeskSystem Web App and API concurrently..." -ForegroundColor Cyan

# Start API in background
Start-Process dotnet -ArgumentList "run --project ServiceDeskSystem.Api/ServiceDeskSystem.Api.csproj" -NoNewWindow
Write-Host "Started API on http://localhost:5182/swagger" -ForegroundColor Green

# Start Web App
Write-Host "Starting Web App on http://localhost:5034..." -ForegroundColor Green
dotnet run --project ServiceDeskSystem/ServiceDeskSystem.csproj

Write-Host "Both applications stopped." -ForegroundColor Yellow
