# Script para executar testes com cobertura (Windows PowerShell)

param(
    [Parameter(Mandatory = $false)]
    [string]$TestProject = "Mercado.Produto.Tests",
    
    [Parameter(Mandatory = $false)]
    [string]$TestFilter = "",
    
    [Parameter(Mandatory = $false)]
    [bool]$GenerateReport = $true,
    
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "./coverage"
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Executando Testes com Cobertura" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Validar diretório do projeto
if (-not (Test-Path "$TestProject\$TestProject.csproj")) {
    Write-Host "Erro: Projeto $TestProject não encontrado!" -ForegroundColor Red
    exit 1
}

# Limpar cobertura antiga
Write-Host ""
Write-Host "Limpando cobertura anterior..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}

# Preparar diretório
New-Item -ItemType Directory -Path $OutputPath -Force > $null

# Construir comando de teste
$testCommand = @(
    "test",
    "$TestProject\$TestProject.csproj",
    "--no-build",
    "--logger", "console;verbosity=normal",
    "--collect:`"XPlat Code Coverage`"",
    "--results-directory=`"$OutputPath`""
)

# Adicionar filtro se fornecido
if ($TestFilter) {
    $testCommand += @("--filter", "`"$TestFilter`"")
}

Write-Host ""
Write-Host "Executando testes..." -ForegroundColor Yellow
Write-Host "Comando: dotnet $($testCommand -join ' ')" -ForegroundColor Gray

# Executar testes
& dotnet @testCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Erro ao executar testes!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Verificar se arquivo de cobertura foi gerado
$coverageFile = Get-ChildItem -Path $OutputPath -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1

if ($coverageFile) {
    Write-Host ""
    Write-Host "✓ Cobertura gerada com sucesso!" -ForegroundColor Green
    Write-Host "  Arquivo: $($coverageFile.FullName)" -ForegroundColor Gray
    
    if ($GenerateReport) {
        Write-Host ""
        Write-Host "Gerando relatório HTML..." -ForegroundColor Yellow
        
        # Verificar se reportgenerator está instalado
        $reportGenerator = dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"
        
        if (-not $reportGenerator) {
            Write-Host "ReportGenerator não está instalado. Instalando..." -ForegroundColor Yellow
            dotnet tool install -g dotnet-reportgenerator-globaltool
        }
        
        $reportPath = "$OutputPath/report"
        
        # Gerar relatório
        reportgenerator -reports:"$($coverageFile.FullName)" `
                       -targetdir:"$reportPath" `
                       -reporttypes:Html
        
        if (Test-Path "$reportPath/index.html") {
            Write-Host "✓ Relatório HTML gerado!" -ForegroundColor Green
            Write-Host "  Caminho: $reportPath" -ForegroundColor Gray
            Write-Host ""
            Write-Host "Abrindo relatório no navegador..." -ForegroundColor Yellow
            
            Invoke-Item "$reportPath/index.html"
        }
    }
}
else {
    Write-Host ""
    Write-Host "⚠ Nenhum arquivo de cobertura encontrado" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Execução Concluída!" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
