@echo off
echo Compilando OnliDesk Launcher...

REM Verificar se o Visual Studio Build Tools está instalado
where cl >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Tentando localizar Visual Studio Build Tools...
    
    REM Tentar localizar VS 2022
    if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" (
        call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat"
    ) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat" (
        call "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat"
    ) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Auxiliary\Build\vcvars64.bat" (
        call "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Auxiliary\Build\vcvars64.bat"
    ) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat" (
        call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat"
    ) else (
        echo ERRO: Visual Studio Build Tools nao encontrado!
        echo.
        echo Para compilar o launcher, instale uma das opcoes:
        echo 1. Visual Studio Community 2022 (gratuito)
        echo 2. Visual Studio Build Tools 2022
        echo 3. MinGW-w64
        echo.
        echo Usando MinGW como alternativa...
        goto :mingw_build
    )
)

REM Compilar com Visual Studio
echo Compilando com Visual Studio...
cl /EHsc /O2 OnliDeskLauncher.cpp /Fe:OnliDesk.exe /link user32.lib shell32.lib advapi32.lib
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ OnliDesk.exe compilado com sucesso!
    echo Tamanho: 
    dir OnliDesk.exe | find ".exe"
    goto :success
) else (
    echo Erro na compilacao com Visual Studio, tentando MinGW...
    goto :mingw_build
)

:mingw_build
REM Tentar compilar com MinGW
where g++ >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERRO: Nenhum compilador C++ encontrado!
    echo.
    echo Instale uma das opcoes:
    echo 1. Visual Studio Community: https://visualstudio.microsoft.com/downloads/
    echo 2. MinGW-w64: https://www.mingw-w64.org/downloads/
    echo 3. MSYS2: https://www.msys2.org/
    pause
    exit /b 1
)

echo Compilando com MinGW...
g++ -O2 -static -mwindows OnliDeskLauncher.cpp -o OnliDesk.exe -luser32 -lshell32 -ladvapi32
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ OnliDesk.exe compilado com sucesso!
    echo Tamanho:
    dir OnliDesk.exe | find ".exe"
    goto :success
) else (
    echo ERRO: Falha na compilacao!
    pause
    exit /b 1
)

:success
echo.
echo 🚀 Launcher criado com sucesso!
echo.
echo Arquivos gerados:
echo - OnliDesk.exe (Launcher nativo)
echo - OliAcessoRemoto.exe (Aplicacao principal)
echo.
echo Para distribuir:
echo 1. Copie ambos os arquivos para o mesmo diretorio
echo 2. Execute OnliDesk.exe
echo 3. O launcher verificara e instalara o .NET automaticamente
echo.
pause
