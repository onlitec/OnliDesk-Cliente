@echo off
title OnliDesk - Launcher
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
echo  [2] Cancelar
echo.
set /p choice="Escolha uma opção (1-2): "

if "%choice%"=="1" goto :open_download_page
if "%choice%"=="2" goto :cancel
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

REM Tentar executar via dotnet run primeiro (mais confiável)
if exist "OliAcessoRemoto.csproj" (
    echo  🔧 Executando via código fonte...
    dotnet run --project OliAcessoRemoto.csproj
    if %ERRORLEVEL% EQU 0 (
        echo  ✅ OnliDesk executado com sucesso!
        goto :success
    ) else (
        echo  ⚠️ Erro ao executar via código fonte, tentando executável...
    )
)

REM Tentar executar via executável
if exist "OliAcessoRemoto.exe" (
    echo  📦 Executando via executável...
    start "" "OliAcessoRemoto.exe"
    if %ERRORLEVEL% EQU 0 (
        echo  ✅ OnliDesk executado com sucesso!
        goto :success
    ) else (
        echo  ❌ Erro ao executar executável!
        goto :error
    )
) else (
    echo  ❌ Nem código fonte nem executável encontrado!
    goto :error
)

:success
echo.
echo  💡 Dica: Você pode criar um atalho para este arquivo
echo     para facilitar o acesso ao OnliDesk.
echo.
timeout /t 3 /nobreak >nul
exit /b 0

:error
echo.
echo  ❌ Erro ao iniciar OnliDesk!
echo.
echo  Possíveis soluções:
echo  • Verifique se os arquivos não estão corrompidos
echo  • Execute como administrador
echo  • Verifique se o antivírus não está bloqueando
echo  • Certifique-se de que OliAcessoRemoto.exe ou OliAcessoRemoto.csproj estão na mesma pasta
echo.
pause
exit /b 1
