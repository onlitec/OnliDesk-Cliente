@echo off
title OnliDesk - Verificando Requisitos
color 0B
cls

echo.
echo  ╔══════════════════════════════════════════════════════════════╗
echo  ║                         OnliDesk                            ║
echo  ║                    Acesso Remoto Seguro                     ║
echo  ╚══════════════════════════════════════════════════════════════╝
echo.
echo  🔍 Verificando requisitos do sistema...
echo.

REM Verificar se o executável principal existe
if not exist "OliAcessoRemoto.exe" (
    echo  ❌ Erro: OliAcessoRemoto.exe não encontrado!
    echo.
    echo  Certifique-se de que os seguintes arquivos estão na mesma pasta:
    echo  • OnliDesk.bat ^(este arquivo^)
    echo  • OliAcessoRemoto.exe ^(aplicação principal^)
    echo.
    pause
    exit /b 1
)

REM Verificar se o .NET Desktop Runtime está instalado
echo  🔍 Verificando .NET 8 Desktop Runtime...
dotnet --list-runtimes 2>nul | findstr /C:"Microsoft.WindowsDesktop.App 8." >nul
if %ERRORLEVEL% EQU 0 (
    echo  ✅ .NET 8 Desktop Runtime encontrado
    goto :launch_app
)

REM Verificar se o .NET está instalado (qualquer versão)
dotnet --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo  ❌ .NET não está instalado
    goto :install_dotnet
)

REM .NET está instalado mas não a versão Desktop 8.0
echo  ⚠️  .NET encontrado, mas .NET 8 Desktop Runtime não está instalado
goto :install_dotnet

:install_dotnet
echo.
echo  ╔══════════════════════════════════════════════════════════════╗
echo  ║                 .NET 8 Runtime Necessário                   ║
echo  ╚══════════════════════════════════════════════════════════════╝
echo.
echo  OnliDesk requer o .NET 8 Desktop Runtime para funcionar.
echo.
echo  📋 Informações da instalação:
echo  • Tamanho: ~55MB
echo  • Tempo: 2-3 minutos
echo  • Necessário apenas uma vez
echo  • Instalação oficial da Microsoft
echo.
echo  🔗 Opções de instalação:
echo  [1] Abrir página de download oficial (recomendado)
echo  [2] Download direto do instalador
echo  [3] Cancelar
echo.
set /p choice="Escolha uma opção (1-3): "

if "%choice%"=="1" goto :open_download_page
if "%choice%"=="2" goto :direct_download
if "%choice%"=="3" goto :cancel
goto :install_dotnet

:open_download_page
echo.
echo  🌐 Abrindo página oficial de download...
start https://dotnet.microsoft.com/download/dotnet/8.0
echo.
echo  📋 Instruções:
echo  1. Na página que abriu, procure por ".NET Desktop Runtime 8.0"
echo  2. Clique em "Download x64" (para Windows 64-bit)
echo  3. Execute o arquivo baixado
echo  4. Siga as instruções do instalador
echo  5. Após a instalação, execute este launcher novamente
echo.
pause
exit /b 0

:direct_download
echo.
echo  📥 Iniciando download direto...
echo  🔗 URL: https://download.microsoft.com/download/6/0/f/60fc8ea7-d5d1-4c7b-8b9b-1e8d0a5c1e5a/windowsdesktop-runtime-8.0.8-win-x64.exe
echo.

REM Tentar baixar usando PowerShell
powershell -Command "try { Invoke-WebRequest -Uri 'https://download.microsoft.com/download/6/0/f/60fc8ea7-d5d1-4c7b-8b9b-1e8d0a5c1e5a/windowsdesktop-runtime-8.0.8-win-x64.exe' -OutFile '%TEMP%\dotnet-desktop-runtime-8.0.8-win-x64.exe'; Write-Host '✅ Download concluído'; exit 0 } catch { Write-Host '❌ Erro no download'; exit 1 }"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo  ✅ Download concluído com sucesso!
    echo  🚀 Executando instalador...
    echo.
    echo  ⚠️  Siga as instruções na tela para completar a instalação.
    echo.
    
    REM Executar o instalador
    "%TEMP%\dotnet-desktop-runtime-8.0.8-win-x64.exe"
    
    echo.
    echo  ✅ Instalação concluída!
    echo  🔄 Verificando instalação...
    
    REM Aguardar um pouco e verificar novamente
    timeout /t 3 /nobreak >nul
    
    dotnet --list-runtimes 2>nul | findstr /C:"Microsoft.WindowsDesktop.App 8." >nul
    if %ERRORLEVEL% EQU 0 (
        echo  ✅ .NET 8 Desktop Runtime instalado com sucesso!
        echo.
        goto :launch_app
    ) else (
        echo  ⚠️  Instalação pode não ter sido concluída.
        echo  💡 Tente reiniciar o computador e execute este launcher novamente.
        echo.
        pause
        exit /b 1
    )
) else (
    echo.
    echo  ❌ Erro no download automático.
    echo  🔄 Redirecionando para download manual...
    goto :open_download_page
)

:cancel
echo.
echo  ❌ Instalação cancelada.
echo  OnliDesk não pode funcionar sem o .NET 8 Desktop Runtime.
echo.
pause
exit /b 1

:launch_app
echo.
echo  ╔══════════════════════════════════════════════════════════════╗
echo  ║                    Iniciando OnliDesk                       ║
echo  ╚══════════════════════════════════════════════════════════════╝
echo.
echo  🚀 Todos os requisitos atendidos!
echo  📱 Iniciando OnliDesk...
echo.

REM Executar o OnliDesk usando dotnet run (mais confiável)
if exist "OliAcessoRemoto.csproj" (
    echo  🚀 Executando via dotnet run...
    dotnet run --project OliAcessoRemoto.csproj
) else (
    echo  🚀 Executando executável...
    start "" "OliAcessoRemoto.exe"
)

if %ERRORLEVEL% EQU 0 (
    echo  ✅ OnliDesk iniciado com sucesso!
    echo.
    echo  💡 Dica: Você pode criar um atalho para este arquivo
    echo     para facilitar o acesso ao OnliDesk.
    echo.
    timeout /t 3 /nobreak >nul
) else (
    echo  ❌ Erro ao iniciar OnliDesk!
    echo.
    echo  Possíveis soluções:
    echo  • Verifique se OliAcessoRemoto.exe não está corrompido
    echo  • Execute como administrador
    echo  • Verifique se o antivírus não está bloqueando
    echo.
    pause
)

exit /b 0
