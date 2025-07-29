# OnliDesk Launcher - Verificação automática do .NET Runtime
# Este script verifica se o .NET 8 está instalado e oferece instalação automática

param(
    [switch]$Silent = $false
)

# Configurações
$AppName = "OnliDesk"
$DotNetVersion = "8.0"
$ExecutableName = "OliAcessoRemoto.exe"
$DownloadUrl = "https://download.microsoft.com/download/6/0/f/60fc8ea7-d5d1-4c7b-8b9b-1e8d0a5c1e5a/windowsdesktop-runtime-8.0.8-win-x64.exe"

# Função para mostrar mensagens
function Show-Message {
    param(
        [string]$Message,
        [string]$Title = $AppName,
        [string]$Type = "Information"
    )
    
    if (-not $Silent) {
        $IconType = switch ($Type) {
            "Error" { [System.Windows.Forms.MessageBoxIcon]::Error }
            "Warning" { [System.Windows.Forms.MessageBoxIcon]::Warning }
            "Question" { [System.Windows.Forms.MessageBoxIcon]::Question }
            default { [System.Windows.Forms.MessageBoxIcon]::Information }
        }
        
        Add-Type -AssemblyName System.Windows.Forms
        [System.Windows.Forms.MessageBox]::Show($Message, $Title, [System.Windows.Forms.MessageBoxButtons]::OK, $IconType)
    } else {
        Write-Host "[$Type] $Message"
    }
}

# Função para perguntar sim/não
function Ask-Question {
    param(
        [string]$Message,
        [string]$Title = $AppName
    )
    
    if ($Silent) {
        return $true
    }
    
    Add-Type -AssemblyName System.Windows.Forms
    $result = [System.Windows.Forms.MessageBox]::Show($Message, $Title, [System.Windows.Forms.MessageBoxButtons]::YesNo, [System.Windows.Forms.MessageBoxIcon]::Question)
    return ($result -eq [System.Windows.Forms.DialogResult]::Yes)
}

# Função para verificar se o .NET está instalado
function Test-DotNetInstalled {
    try {
        # Método 1: Verificar via comando dotnet
        $dotnetInfo = & dotnet --info 2>$null
        if ($LASTEXITCODE -eq 0 -and $dotnetInfo -match "Microsoft\.WindowsDesktop\.App $DotNetVersion") {
            Write-Host "✅ .NET $DotNetVersion Desktop Runtime encontrado via comando dotnet"
            return $true
        }
        
        # Método 2: Verificar via registro do Windows
        $registryPaths = @(
            "HKLM:\SOFTWARE\dotnet\Setup\InstalledVersions\x64\Microsoft.WindowsDesktop.App",
            "HKLM:\SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x86\Microsoft.WindowsDesktop.App"
        )
        
        foreach ($path in $registryPaths) {
            if (Test-Path $path) {
                $versions = Get-ChildItem $path -ErrorAction SilentlyContinue
                foreach ($version in $versions) {
                    if ($version.Name -match $DotNetVersion) {
                        Write-Host "✅ .NET $DotNetVersion Desktop Runtime encontrado no registro: $($version.Name)"
                        return $true
                    }
                }
            }
        }
        
        # Método 3: Verificar pastas de instalação
        $installPaths = @(
            "$env:ProgramFiles\dotnet\shared\Microsoft.WindowsDesktop.App",
            "${env:ProgramFiles(x86)}\dotnet\shared\Microsoft.WindowsDesktop.App"
        )
        
        foreach ($path in $installPaths) {
            if (Test-Path $path) {
                $versions = Get-ChildItem $path -Directory -ErrorAction SilentlyContinue
                foreach ($version in $versions) {
                    if ($version.Name -match "^$DotNetVersion") {
                        Write-Host "✅ .NET $DotNetVersion Desktop Runtime encontrado em: $($version.FullName)"
                        return $true
                    }
                }
            }
        }
        
        Write-Host "❌ .NET $DotNetVersion Desktop Runtime não encontrado"
        return $false
    }
    catch {
        Write-Host "❌ Erro ao verificar .NET: $($_.Exception.Message)"
        return $false
    }
}

# Função para baixar e instalar o .NET Runtime
function Install-DotNetRuntime {
    $message = @"
$AppName requer o .NET $DotNetVersion Desktop Runtime para funcionar.

Deseja baixar e instalar automaticamente?

• Tamanho: ~55MB
• Tempo: 2-3 minutos  
• Necessário apenas uma vez
• Instalação oficial da Microsoft
"@
    
    if (Ask-Question $message) {
        try {
            Show-Message "Iniciando download do .NET $DotNetVersion Desktop Runtime...`n`n• O instalador será baixado e executado automaticamente`n• Aguarde a conclusão da instalação`n• O $AppName será iniciado automaticamente após a instalação"
            
            # Baixar o instalador
            $tempFile = "$env:TEMP\dotnet-desktop-runtime-$DotNetVersion-installer.exe"
            Write-Host "Baixando .NET Runtime de: $DownloadUrl"
            
            $webClient = New-Object System.Net.WebClient
            $webClient.DownloadFile($DownloadUrl, $tempFile)
            
            if (Test-Path $tempFile) {
                Write-Host "✅ Download concluído: $tempFile"
                
                # Executar o instalador
                Show-Message "Executando instalador do .NET Runtime...`n`nSiga as instruções na tela para completar a instalação."
                
                $process = Start-Process -FilePath $tempFile -ArgumentList "/quiet" -Wait -PassThru
                
                if ($process.ExitCode -eq 0) {
                    Show-Message "✅ .NET $DotNetVersion Desktop Runtime instalado com sucesso!`n`nO $AppName será iniciado agora."
                    
                    # Limpar arquivo temporário
                    Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
                    
                    return $true
                } else {
                    Show-Message "❌ Erro durante a instalação (Código: $($process.ExitCode))`n`nTente instalar manualmente em:`nhttps://dotnet.microsoft.com/download/dotnet/$DotNetVersion" "Erro de Instalação" "Error"
                    return $false
                }
            } else {
                Show-Message "❌ Erro ao baixar o instalador.`n`nVerifique sua conexão com a internet e tente novamente." "Erro de Download" "Error"
                return $false
            }
        }
        catch {
            Show-Message "❌ Erro durante a instalação: $($_.Exception.Message)`n`nPara instalar manualmente:`n1. Acesse: https://dotnet.microsoft.com/download/dotnet/$DotNetVersion`n2. Baixe 'Desktop Runtime' para Windows x64`n3. Execute o instalador`n4. Reinicie o $AppName" "Erro de Instalação" "Error"
            return $false
        }
    } else {
        $manualMessage = @"
$AppName não pode funcionar sem o .NET $DotNetVersion Desktop Runtime.

Para instalar manualmente:
1. Acesse: https://dotnet.microsoft.com/download/dotnet/$DotNetVersion
2. Baixe 'Desktop Runtime' para Windows x64
3. Execute o instalador
4. Reinicie o $AppName
"@
        Show-Message $manualMessage "Instalação Manual Necessária" "Warning"
        return $false
    }
}

# Função para executar o OnliDesk
function Start-OnliDesk {
    $exePath = Join-Path $PSScriptRoot $ExecutableName
    
    if (-not (Test-Path $exePath)) {
        Show-Message "❌ Arquivo $ExecutableName não encontrado!`n`nCertifique-se de que '$ExecutableName' está na mesma pasta que este launcher." "Arquivo Não Encontrado" "Error"
        return $false
    }
    
    try {
        Write-Host "🚀 Iniciando $AppName..."
        Start-Process -FilePath $exePath
        return $true
    }
    catch {
        Show-Message "❌ Erro ao executar $AppName!`n`nDetalhes: $($_.Exception.Message)" "Erro de Execução" "Error"
        return $false
    }
}

# Função principal
function Main {
    Write-Host "🔍 Verificando requisitos do $AppName..."
    
    # Verificar se o .NET está instalado
    if (Test-DotNetInstalled) {
        Write-Host "✅ .NET $DotNetVersion Desktop Runtime está instalado"
        
        # Executar OnliDesk
        if (Start-OnliDesk) {
            Write-Host "✅ $AppName iniciado com sucesso!"
            exit 0
        } else {
            exit 1
        }
    } else {
        Write-Host "⚠️ .NET $DotNetVersion Desktop Runtime não está instalado"
        
        # Tentar instalar o .NET
        if (Install-DotNetRuntime) {
            # Verificar novamente após instalação
            if (Test-DotNetInstalled) {
                Start-OnliDesk
                exit 0
            } else {
                Show-Message "❌ Instalação do .NET não foi detectada.`n`nReinicie o computador e tente novamente." "Reinicialização Necessária" "Warning"
                exit 1
            }
        } else {
            exit 1
        }
    }
}

# Executar função principal
Main
