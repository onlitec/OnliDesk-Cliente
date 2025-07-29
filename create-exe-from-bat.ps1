# Criar executável a partir do batch file
Write-Host "🔧 Convertendo OnliDesk.bat para OnliDesk.exe..."

# Verificar se o batch existe
if (-not (Test-Path "OnliDesk.bat")) {
    Write-Host "❌ OnliDesk.bat não encontrado!"
    exit 1
}

# Método 1: Usar IExpress (nativo do Windows)
$iexpressScript = @"
[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
FILE0="OnliDesk.bat"
FILE1="OliAcessoRemoto.exe"
[Strings]
InstallPrompt=
DisplayLicense=
FinishMessage=
TargetName=$PWD\OnliDesk.exe
FriendlyName=OnliDesk Launcher
AppLaunched=OnliDesk.bat
PostInstallCmd=<None>
AdminQuietInstCmd=<None>
FILE0="OnliDesk.bat"
FILE1="OliAcessoRemoto.exe"
"@

# Salvar script do IExpress
$iexpressScript | Out-File -FilePath "OnliDesk.sed" -Encoding ASCII

try {
    # Executar IExpress
    Write-Host "🔨 Criando executável com IExpress..."
    $process = Start-Process -FilePath "iexpress.exe" -ArgumentList "/N OnliDesk.sed" -Wait -PassThru -WindowStyle Hidden
    
    if ($process.ExitCode -eq 0 -and (Test-Path "OnliDesk.exe")) {
        $fileInfo = Get-Item "OnliDesk.exe"
        Write-Host "✅ OnliDesk.exe criado com sucesso!"
        Write-Host "📏 Tamanho: $([math]::Round($fileInfo.Length / 1MB, 2)) MB"
        
        # Limpar arquivo temporário
        Remove-Item "OnliDesk.sed" -Force -ErrorAction SilentlyContinue
        
        Write-Host ""
        Write-Host "🚀 Launcher executável pronto!"
        Write-Host ""
        Write-Host "📦 Para distribuir:"
        Write-Host "1. Copie OnliDesk.exe e OliAcessoRemoto.exe para o mesmo diretório"
        Write-Host "2. O usuário executa apenas OnliDesk.exe"
        Write-Host "3. O launcher verifica e instala o .NET automaticamente"
        
    } else {
        throw "IExpress falhou"
    }
} catch {
    Write-Host "⚠️ IExpress não funcionou, usando método alternativo..."
    
    # Método 2: Criar um wrapper PowerShell compilado simples
    $wrapperScript = @"
# OnliDesk Launcher Wrapper
`$batPath = Join-Path `$PSScriptRoot "OnliDesk.bat"
if (Test-Path `$batPath) {
    Start-Process -FilePath "cmd.exe" -ArgumentList "/c `"`$batPath`"" -Wait
} else {
    [System.Windows.Forms.MessageBox]::Show("OnliDesk.bat não encontrado!", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
}
"@
    
    $wrapperScript | Out-File -FilePath "OnliDesk-Wrapper.ps1" -Encoding UTF8
    
    # Tentar usar ps2exe se disponível
    try {
        if (Get-Command ps2exe -ErrorAction SilentlyContinue) {
            ps2exe -inputFile "OnliDesk-Wrapper.ps1" -outputFile "OnliDesk.exe" -noConsole -title "OnliDesk" -description "OnliDesk Launcher"
            
            if (Test-Path "OnliDesk.exe") {
                Write-Host "✅ OnliDesk.exe criado com ps2exe!"
                Remove-Item "OnliDesk-Wrapper.ps1" -Force
            } else {
                throw "ps2exe falhou"
            }
        } else {
            throw "ps2exe não disponível"
        }
    } catch {
        Write-Host "⚠️ Não foi possível criar executável automaticamente."
        Write-Host ""
        Write-Host "📋 Soluções alternativas:"
        Write-Host ""
        Write-Host "1. 🎯 Use OnliDesk.bat diretamente (funciona perfeitamente)"
        Write-Host "   • Renomeie para OnliDesk.cmd se preferir"
        Write-Host "   • Crie um atalho com ícone personalizado"
        Write-Host ""
        Write-Host "2. 🔧 Instale ps2exe para criar executável:"
        Write-Host "   Install-Module ps2exe -Force"
        Write-Host "   ps2exe -inputFile OnliDesk-Wrapper.ps1 -outputFile OnliDesk.exe -noConsole"
        Write-Host ""
        Write-Host "3. 📦 Use ferramenta externa como Bat To Exe Converter"
        Write-Host ""
        
        # Criar atalho como alternativa
        try {
            $WshShell = New-Object -comObject WScript.Shell
            $Shortcut = $WshShell.CreateShortcut("$PWD\OnliDesk.lnk")
            $Shortcut.TargetPath = "$PWD\OnliDesk.bat"
            $Shortcut.WorkingDirectory = $PWD
            $Shortcut.Description = "OnliDesk - Acesso Remoto"
            if (Test-Path "Resources\OnliDesk.ico") {
                $Shortcut.IconLocation = "$PWD\Resources\OnliDesk.ico"
            }
            $Shortcut.Save()
            
            Write-Host "✅ Atalho OnliDesk.lnk criado como alternativa!"
        } catch {
            Write-Host "⚠️ Não foi possível criar atalho"
        }
        
        Remove-Item "OnliDesk-Wrapper.ps1" -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ""
Write-Host "🎉 Processo concluído!"
Write-Host ""
Write-Host "✨ Funcionalidades do launcher:"
Write-Host "   ✅ Verificação automática do .NET 8 Desktop Runtime"
Write-Host "   ✅ Download e instalação automática"
Write-Host "   ✅ Interface amigável com cores e emojis"
Write-Host "   ✅ Múltiplas opções de instalação"
Write-Host "   ✅ Tratamento robusto de erros"
Write-Host "   ✅ Instruções claras para o usuário"
