# Script para criar executável do launcher OnliDesk
Write-Host "🔧 Criando launcher executável para OnliDesk..."

# Verificar se ps2exe está instalado
try {
    Get-Command ps2exe -ErrorAction Stop | Out-Null
    Write-Host "✅ ps2exe encontrado"
} catch {
    Write-Host "⚠️ ps2exe não encontrado. Instalando..."
    try {
        Install-Module -Name ps2exe -Force -Scope CurrentUser
        Write-Host "✅ ps2exe instalado com sucesso"
    } catch {
        Write-Host "❌ Erro ao instalar ps2exe. Instalando manualmente..."
        
        # Baixar ps2exe manualmente
        $ps2exeUrl = "https://github.com/MScholtes/PS2EXE/archive/refs/heads/master.zip"
        $tempZip = "$env:TEMP\ps2exe-master.zip"
        $tempDir = "$env:TEMP\ps2exe-master"
        
        Invoke-WebRequest -Uri $ps2exeUrl -OutFile $tempZip
        Expand-Archive -Path $tempZip -DestinationPath $env:TEMP -Force
        
        # Importar módulo
        Import-Module "$tempDir\PS2EXE-master\ps2exe.psm1" -Force
        Write-Host "✅ ps2exe carregado manualmente"
    }
}

# Parâmetros para o executável
$scriptPath = "OnliDesk-Launcher.ps1"
$exePath = "OnliDesk.exe"
$iconPath = "Resources\OnliDesk.ico"

# Verificar se o script existe
if (-not (Test-Path $scriptPath)) {
    Write-Host "❌ Arquivo $scriptPath não encontrado!"
    exit 1
}

# Criar executável
Write-Host "🔨 Compilando $scriptPath para $exePath..."

$ps2exeParams = @{
    inputFile = $scriptPath
    outputFile = $exePath
    noConsole = $true
    title = "OnliDesk Launcher"
    description = "OnliDesk - Launcher com verificação automática do .NET Runtime"
    company = "OnliTec"
    product = "OnliDesk"
    copyright = "© 2024 OnliTec"
    version = "1.0.0.0"
    requireAdmin = $false
    supportOS = $true
    longPaths = $true
}

# Adicionar ícone se existir
if (Test-Path $iconPath) {
    $ps2exeParams.iconFile = $iconPath
    Write-Host "✅ Ícone adicionado: $iconPath"
}

try {
    ps2exe @ps2exeParams
    
    if (Test-Path $exePath) {
        $fileInfo = Get-Item $exePath
        Write-Host "✅ OnliDesk.exe criado com sucesso!"
        Write-Host "📁 Localização: $($fileInfo.FullName)"
        Write-Host "📏 Tamanho: $([math]::Round($fileInfo.Length / 1MB, 2)) MB"
        
        # Verificar propriedades do executável
        $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exePath)
        Write-Host "📋 Propriedades:"
        Write-Host "   - Título: $($versionInfo.FileDescription)"
        Write-Host "   - Versão: $($versionInfo.FileVersion)"
        Write-Host "   - Empresa: $($versionInfo.CompanyName)"
        
        Write-Host ""
        Write-Host "🚀 Launcher pronto para distribuição!"
        Write-Host ""
        Write-Host "📦 Para distribuir o OnliDesk:"
        Write-Host "1. Copie estes arquivos para o mesmo diretório:"
        Write-Host "   - OnliDesk.exe (Launcher - $([math]::Round($fileInfo.Length / 1MB, 2)) MB)"
        Write-Host "   - OliAcessoRemoto.exe (Aplicação principal)"
        Write-Host ""
        Write-Host "2. O usuário executa apenas OnliDesk.exe"
        Write-Host "3. O launcher verifica e instala o .NET automaticamente"
        Write-Host "4. Após instalação, o OnliDesk é executado automaticamente"
        Write-Host ""
        Write-Host "✨ Funcionalidades do launcher:"
        Write-Host "   ✅ Verificação automática do .NET 8 Desktop Runtime"
        Write-Host "   ✅ Download e instalação automática do .NET"
        Write-Host "   ✅ Interface gráfica amigável"
        Write-Host "   ✅ Detecção via registro, comando e pastas"
        Write-Host "   ✅ Tratamento de erros robusto"
        Write-Host "   ✅ Não requer PowerShell visível"
        
    } else {
        Write-Host "❌ Erro: OnliDesk.exe não foi criado!"
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao criar executável: $($_.Exception.Message)"
    
    # Fallback: criar um batch file simples
    Write-Host "🔄 Criando launcher alternativo (batch)..."
    
    $batchContent = @"
@echo off
title OnliDesk Launcher
echo 🔍 Verificando requisitos do OnliDesk...

REM Verificar se o .NET está instalado
dotnet --info >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ⚠️ .NET não encontrado. Abrindo página de download...
    start https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo Instruções:
    echo 1. Baixe '.NET Desktop Runtime 8.0' para Windows x64
    echo 2. Execute o instalador
    echo 3. Reinicie este launcher
    pause
    exit /b 1
)

echo ✅ .NET encontrado
echo 🚀 Iniciando OnliDesk...

if exist "OliAcessoRemoto.exe" (
    start "" "OliAcessoRemoto.exe"
) else (
    echo ❌ OliAcessoRemoto.exe não encontrado!
    echo Certifique-se de que o arquivo está na mesma pasta.
    pause
)
"@
    
    $batchContent | Out-File -FilePath "OnliDesk.bat" -Encoding ASCII
    Write-Host "✅ OnliDesk.bat criado como alternativa"
    
    exit 1
}

Write-Host ""
Write-Host "🎉 Processo concluído!"
