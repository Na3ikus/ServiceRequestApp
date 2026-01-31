#!/usr/bin/env pwsh
# Скрипт для автоматичного запуску ServiceDeskSystem з ngrok

Write-Host "=== ServiceDeskSystem Demo Launcher ===" -ForegroundColor Cyan
Write-Host ""

# Перевірка, чи встановлений ngrok
$ngrokInstalled = Get-Command ngrok -ErrorAction SilentlyContinue

if (-not $ngrokInstalled) {
    Write-Host "⚠️  ngrok не знайдено!" -ForegroundColor Yellow
    Write-Host "Встановіть ngrok за інструкцією в NGROK_GUIDE.md" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Швидке встановлення через Chocolatey:" -ForegroundColor Gray
    Write-Host "  choco install ngrok" -ForegroundColor Gray
    Write-Host ""
    $continue = Read-Host "Продовжити без ngrok? (y/n)"
    if ($continue -ne 'y') { exit }
}

# Налаштування портів
$APP_PORT = 5001
$APP_URL = "https://localhost:$APP_PORT"

Write-Host "📦 Збірка проекту..." -ForegroundColor Yellow
$scriptRoot = $PSScriptRoot
if (-not $scriptRoot) { $scriptRoot = Get-Location }
Push-Location $scriptRoot

dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Помилка збірки проекту!" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "✅ Проект успішно зібрано!" -ForegroundColor Green
Write-Host ""

# Запуск додатку в фоновому режимі
Write-Host "🚀 Запуск Blazor додатку на $APP_URL..." -ForegroundColor Yellow
$appProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --urls=$APP_URL" -PassThru -NoNewWindow

Write-Host "⏳ Очікування запуску додатку..." -ForegroundColor Gray

$appReady = $false
for ($i = 0; $i -lt 15; $i++) {
    Start-Sleep -Seconds 1
    try {
        $response = Invoke-WebRequest -Uri $APP_URL -SkipCertificateCheck -TimeoutSec 3 -ErrorAction Stop
        $appReady = $true
        break
    } catch {
        # Продовжуємо очікування
    }
}

if ($appReady) {
    Write-Host "✅ Додаток успішно запущено!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Додаток ще запускається (це нормально для першого запуску)" -ForegroundColor Yellow
}

Write-Host ""

if ($ngrokInstalled) {
    Write-Host "🌐 Запуск ngrok тунелю..." -ForegroundColor Yellow
    $NGROK_DOMAIN = $env:NGROK_DOMAIN
    $ngrokLogOut = Join-Path $scriptRoot "ngrok.log"
    $ngrokLogErr = Join-Path $scriptRoot "ngrok-error.log"

    if ($NGROK_DOMAIN) {
        Write-Host "🔗 Використання статичного домену: $NGROK_DOMAIN" -ForegroundColor Cyan
        $ngrokArgs = "http --domain=$NGROK_DOMAIN $APP_URL --log=stdout"
    } else {
        $ngrokArgs = "http $APP_URL --log=stdout"
    }

    $ngrokProcess = Start-Process -FilePath "ngrok" -ArgumentList $ngrokArgs -PassThru -RedirectStandardOutput $ngrokLogOut -RedirectStandardError $ngrokLogErr

    Write-Host "⏳ Очікування запуску ngrok..." -ForegroundColor Gray
    Start-Sleep -Seconds 1

    if ($ngrokProcess.HasExited) {
        Write-Host "⚠️  ngrok завершився одразу. Деталі нижче:" -ForegroundColor Yellow
        if (Test-Path $ngrokLogOut) { Get-Content $ngrokLogOut | Select-Object -Last 20 }
        if (Test-Path $ngrokLogErr) { Get-Content $ngrokLogErr | Select-Object -Last 20 }
    }

    $publicUrl = $null
    for ($i = 0; $i -lt 20; $i++) {
        try {
            $ngrokApi = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" -TimeoutSec 3 -ErrorAction Stop
            if ($ngrokApi.tunnels.Count -gt 0) {
                $publicUrl = $ngrokApi.tunnels[0].public_url
                break
            }
        } catch {
            # ngrok API ще не готовий
        }
        Start-Sleep -Seconds 1
    }

    if ($publicUrl) {
        Write-Host ""
        Write-Host "============================================" -ForegroundColor Green
        Write-Host "✅ Додаток доступний публічно!" -ForegroundColor Green
        Write-Host "============================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "🌍 Публічний URL: $publicUrl" -ForegroundColor Cyan
        Write-Host "🏠 Локальний URL: $APP_URL" -ForegroundColor Cyan
        Write-Host "📊 ngrok Dashboard: http://localhost:4040" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "⚠️  УВАГА: На безкоштовному плані ngrok відвідувачі" -ForegroundColor Yellow
        Write-Host "   побачать попереджувальну сторінку перед переходом" -ForegroundColor Yellow
        Write-Host ""

        Start-Process $publicUrl
    } else {
        Write-Host "⚠️  Не вдалося автоматично отримати ngrok URL" -ForegroundColor Yellow
        Write-Host "Перевірте http://localhost:4040 або консоль ngrok, щоб побачити публічний URL" -ForegroundColor Yellow
        if (Test-Path $ngrokLogOut) {
            Write-Host "--- Останні рядки stdout ngrok ---" -ForegroundColor Gray
            Get-Content $ngrokLogOut | Select-Object -Last 20
        }
        if (Test-Path $ngrokLogErr) {
            Write-Host "--- Останні рядки stderr ngrok ---" -ForegroundColor Gray
            Get-Content $ngrokLogErr | Select-Object -Last 20
        }
    }
} else {
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "✅ Додаток запущено локально!" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "🏠 Локальний URL: $APP_URL" -ForegroundColor Cyan
    Write-Host ""
    Start-Process $APP_URL
}

Write-Host ""
Write-Host "Натисніть Ctrl+C для зупинки всіх сервісів..." -ForegroundColor Gray
Write-Host ""

try {
    Wait-Process -Id $appProcess.Id
} finally {
    Write-Host ""
    Write-Host "🛑 Зупинка сервісів..." -ForegroundColor Yellow

    if ($appProcess -and !$appProcess.HasExited) { Stop-Process -Id $appProcess.Id -Force -ErrorAction SilentlyContinue }
    if ($ngrokProcess -and !$ngrokProcess.HasExited) { Stop-Process -Id $ngrokProcess.Id -Force -ErrorAction SilentlyContinue }

    Pop-Location
    Write-Host "✅ Всі сервіси зупинено!" -ForegroundColor Green
}
