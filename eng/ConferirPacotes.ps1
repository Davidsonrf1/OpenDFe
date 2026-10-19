# Abre cada .nupkg e .snupkg e confere os arquivos de lib/ contra o esperado (PLANO.md 0.2):
# o OpenDFe.NFe leva as DLLs do modelo e do leiaute, e nenhum projeto vira pacote por engano.
param([Parameter(Mandatory)][string] $Pasta)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$esperado = @{
    'OpenDFe.Nucleo'        = @('OpenDFe.Nucleo')
    'OpenDFe.NFe'           = @('OpenDFe.Leiaute.NFe.v400', 'OpenDFe.NFe.Modelo')
    'OpenDFe.Impressao.Pdf' = @('OpenDFe.Impressao.Pdf')
}

function Ler-Lib([string] $arquivo) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($arquivo)
    try { $zip.Entries | Where-Object FullName -like 'lib/*' | ForEach-Object FullName | Sort-Object }
    finally { $zip.Dispose() }
}

$falhas = @()
$pacotes = Get-ChildItem $Pasta -Filter *.nupkg
foreach ($nupkg in $pacotes) {
    $id = $nupkg.BaseName -replace '\.\d+\.\d+\.\d+(-[\w.]+)?$', ''
    if (-not $esperado.ContainsKey($id)) { $falhas += "pacote inesperado: $($nupkg.Name)"; continue }

    $dlls = $esperado[$id]
    $conferir = @(
        @{ Arquivo = $nupkg.FullName; Extensoes = @('.dll', '.xml') },
        @{ Arquivo = [IO.Path]::ChangeExtension($nupkg.FullName, '.snupkg'); Extensoes = @('.pdb') }
    )
    foreach ($c in $conferir) {
        if (-not (Test-Path $c.Arquivo)) { $falhas += "faltou: $($c.Arquivo)"; continue }
        $quer = $dlls | ForEach-Object { $d = $_; $c.Extensoes | ForEach-Object { "lib/net10.0/$d$_" } } | Sort-Object
        $tem = @(Ler-Lib $c.Arquivo)
        $diferenca = Compare-Object $quer $tem
        if ($diferenca) {
            $falhas += "$([IO.Path]::GetFileName($c.Arquivo)): esperado [$($quer -join ', ')], encontrado [$($tem -join ', ')]"
        }
    }
}
foreach ($id in $esperado.Keys) {
    if (-not ($pacotes | Where-Object { $_.Name -like "$id.[0-9]*" })) { $falhas += "pacote ausente: $id" }
}

if ($falhas) { $falhas | ForEach-Object { Write-Error $_ -ErrorAction Continue }; exit 1 }
Write-Host "Pacotes conferidos: $($pacotes.Name -join ', ')"
