#include <windows.h>
#include <shellapi.h>
#include <string>
#include <iostream>
#include <filesystem>

// Função para verificar se o .NET 8 está instalado
bool CheckDotNetInstalled() {
    // Verificar via registro do Windows
    HKEY hKey;
    LONG result = RegOpenKeyExA(HKEY_LOCAL_MACHINE, 
        "SOFTWARE\\dotnet\\Setup\\InstalledVersions\\x64\\sharedhost", 
        0, KEY_READ, &hKey);
    
    if (result == ERROR_SUCCESS) {
        RegCloseKey(hKey);
        return true;
    }
    
    // Verificar via comando dotnet --version
    STARTUPINFOA si = { sizeof(si) };
    PROCESS_INFORMATION pi;
    si.dwFlags = STARTF_USESHOWWINDOW;
    si.wShowWindow = SW_HIDE;
    
    char cmdLine[] = "dotnet --version";
    BOOL success = CreateProcessA(NULL, cmdLine, NULL, NULL, FALSE, 
        CREATE_NO_WINDOW, NULL, NULL, &si, &pi);
    
    if (success) {
        WaitForSingleObject(pi.hProcess, 5000); // Aguardar 5 segundos
        DWORD exitCode;
        GetExitCodeProcess(pi.hProcess, &exitCode);
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
        return (exitCode == 0);
    }
    
    return false;
}

// Função para baixar e instalar o .NET Runtime
void InstallDotNetRuntime() {
    int result = MessageBoxA(NULL, 
        "OnliDesk requer o .NET 8 Runtime para funcionar.\n\n"
        "Deseja baixar e instalar automaticamente?\n\n"
        "• Tamanho: ~55MB\n"
        "• Tempo: 2-3 minutos\n"
        "• Necessário apenas uma vez",
        "OnliDesk - .NET Runtime Necessário", 
        MB_YESNO | MB_ICONQUESTION);
    
    if (result == IDYES) {
        // URL do .NET 8 Runtime para Windows x64
        const char* downloadUrl = "https://download.microsoft.com/download/6/0/f/60fc8ea7-d5d1-4c7b-8b9b-1e8d0a5c1e5a/windowsdesktop-runtime-8.0.8-win-x64.exe";
        
        // Mostrar progresso
        MessageBoxA(NULL, 
            "Iniciando download do .NET 8 Runtime...\n\n"
            "• O download será aberto no seu navegador\n"
            "• Execute o instalador baixado\n"
            "• Reinicie o OnliDesk após a instalação",
            "OnliDesk - Instalando .NET", 
            MB_OK | MB_ICONINFORMATION);
        
        // Abrir URL no navegador padrão
        ShellExecuteA(NULL, "open", downloadUrl, NULL, NULL, SW_SHOWNORMAL);
        
        // Mostrar instruções
        MessageBoxA(NULL, 
            "Instruções de instalação:\n\n"
            "1. Execute o arquivo baixado (windowsdesktop-runtime-8.0.8-win-x64.exe)\n"
            "2. Siga as instruções do instalador\n"
            "3. Reinicie o OnliDesk após a instalação\n\n"
            "O OnliDesk será fechado agora.",
            "OnliDesk - Instruções de Instalação", 
            MB_OK | MB_ICONINFORMATION);
    } else {
        MessageBoxA(NULL, 
            "OnliDesk não pode funcionar sem o .NET 8 Runtime.\n\n"
            "Para instalar manualmente:\n"
            "1. Acesse: https://dotnet.microsoft.com/download/dotnet/8.0\n"
            "2. Baixe 'Desktop Runtime' para Windows x64\n"
            "3. Execute o instalador\n"
            "4. Reinicie o OnliDesk",
            "OnliDesk - Instalação Manual", 
            MB_OK | MB_ICONWARNING);
    }
}

// Função para executar o OnliDesk
void LaunchOnliDesk() {
    // Verificar se o executável existe
    if (!std::filesystem::exists("OliAcessoRemoto.exe")) {
        MessageBoxA(NULL, 
            "Arquivo OnliDesk não encontrado!\n\n"
            "Certifique-se de que 'OliAcessoRemoto.exe' está na mesma pasta.",
            "OnliDesk - Erro", 
            MB_OK | MB_ICONERROR);
        return;
    }
    
    // Executar o OnliDesk
    STARTUPINFOA si = { sizeof(si) };
    PROCESS_INFORMATION pi;
    
    char cmdLine[] = "OliAcessoRemoto.exe";
    BOOL success = CreateProcessA(NULL, cmdLine, NULL, NULL, FALSE, 
        0, NULL, NULL, &si, &pi);
    
    if (success) {
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    } else {
        MessageBoxA(NULL, 
            "Erro ao executar OnliDesk!\n\n"
            "Verifique se o arquivo não está corrompido.",
            "OnliDesk - Erro de Execução", 
            MB_OK | MB_ICONERROR);
    }
}

// Função principal
int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow) {
    // Verificar se o .NET está instalado
    if (!CheckDotNetInstalled()) {
        InstallDotNetRuntime();
        return 0;
    }
    
    // .NET está instalado, executar OnliDesk
    LaunchOnliDesk();
    return 0;
}

// Função main para console (fallback)
int main() {
    return WinMain(GetModuleHandle(NULL), NULL, GetCommandLineA(), SW_SHOWNORMAL);
}
