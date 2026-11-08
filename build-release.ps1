# ==============================================================================
# nodesCatchNext 一键打包发行版脚本
# ==============================================================================
param (
    [string]$DependencySource = ""
)

$ErrorActionPreference = "Stop"

function Download-FileReliable {
    param (
        [string]$Url,
        [string]$OutFile
    )
    if (Get-Command curl.exe -ErrorAction SilentlyContinue) {
        & curl.exe -L -s -S --retry 3 --retry-delay 2 -o "$OutFile" "$Url"
        if ($LASTEXITCODE -eq 0 -and (Test-Path $OutFile) -and (Get-Item $OutFile).Length -gt 0) {
            return
        }
    }
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $wc = New-Object System.Net.WebClient
    $wc.Headers.Add("User-Agent", "PowerShell-nodesCatchNext")
    $wc.DownloadFile($Url, $OutFile)
}

# 自适应寻找本地已有依赖目录
if ([string]::IsNullOrWhiteSpace($DependencySource)) {
    if (Test-Path "C:\Applications\nodesCatchNext-windows-x64\mihomo-nodes.exe") {
        $DependencySource = "C:\Applications\nodesCatchNext-windows-x64"
    } elseif (Test-Path "C:\Users\qiuxtao\Downloads\nodesCatchNext-v4.0\mihomo-nodes.exe") {
        $DependencySource = "C:\Users\qiuxtao\Downloads\nodesCatchNext-v4.0"
    }
}

$repoRoot = $PSScriptRoot
$distRoot = Join-Path $repoRoot "dist"
$pkgName = "nodesCatchNext-windows-x64"
$pkgDir = Join-Path $distRoot $pkgName
$zipPath = Join-Path $distRoot "$pkgName.zip"
$tempDownloads = Join-Path $distRoot "temp_downloads"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " 开始构建 nodesCatchNext 发行版" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. 编译最新的 Release 版本
Write-Host "`n[1/4] 编译 Release 配置..." -ForegroundColor Yellow
dotnet build "$repoRoot\nodesCatchNext.csproj" -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "编译失败，请检查编译错误！"
    exit 1
}

# 2. 准备输出目录
Write-Host "`n[2/4] 初始化打包目录: $pkgDir" -ForegroundColor Yellow
if (Test-Path $pkgDir) {
    Remove-Item -Path $pkgDir -Recurse -Force
}
if (Test-Path $zipPath) {
    Remove-Item -Path $zipPath -Force
}
New-Item -ItemType Directory -Path $pkgDir -Force | Out-Null
New-Item -ItemType Directory -Path $tempDownloads -Force | Out-Null

# 3. 复制核心程序文件与依赖
Write-Host "`n[3/4] 组装发行版文件..." -ForegroundColor Yellow
$binRelease = Join-Path $repoRoot "bin\Release\net48"

# 复制主程序与依赖 DLL
Copy-Item "$binRelease\nodesCatchNext.exe" -Destination $pkgDir -Force
Copy-Item "$binRelease\nodesCatchNext.exe.config" -Destination $pkgDir -Force
Copy-Item "$binRelease\Newtonsoft.Json.dll" -Destination $pkgDir -Force

# 复制图标
if (Test-Path "$repoRoot\app.ico") {
    Copy-Item "$repoRoot\app.ico" -Destination $pkgDir -Force
}

# 复制或下载 Mihomo 核心
$mihomoCopied = $false
if (-not [string]::IsNullOrWhiteSpace($DependencySource)) {
    $localMihomo = Join-Path $DependencySource "mihomo-nodes.exe"
    if (Test-Path $localMihomo) {
        Copy-Item $localMihomo -Destination $pkgDir -Force
        Write-Host "  -> 已从本地复制 mihomo-nodes.exe" -ForegroundColor Green
        $mihomoCopied = $true
    }
}
if (-not $mihomoCopied) {
    Write-Host "  -> 正在从官方源下载 Mihomo 核心..." -ForegroundColor Cyan
    $mihomoZip = Join-Path $tempDownloads "mihomo.zip"
    $downloadUrl = "https://github.com/MetaCubeX/mihomo/releases/download/v1.19.12/mihomo-windows-amd64-v1.19.12.zip"
    try {
        $headers = @{ 'User-Agent' = 'PowerShell-nodesCatchNext' }
        if ($env:GITHUB_TOKEN) { $headers['Authorization'] = "Bearer $env:GITHUB_TOKEN" }
        $resp = Invoke-RestMethod -Uri "https://api.github.com/repos/MetaCubeX/mihomo/releases/latest" -Headers $headers -TimeoutSec 10
        $asset = $resp.assets | Where-Object { $_.name -match '^mihomo-windows-amd64-.*\.zip$' -and $_.name -notmatch 'compatible' } | Select-Object -First 1
        if ($asset -and $asset.browser_download_url) {
            $downloadUrl = $asset.browser_download_url
        }
    } catch { }

    Write-Host "     下载地址: $downloadUrl"
    Download-FileReliable -Url $downloadUrl -OutFile $mihomoZip
    $mihomoExtract = Join-Path $tempDownloads "mihomo_extract"
    Expand-Archive -Path $mihomoZip -DestinationPath $mihomoExtract -Force
    $exe = Get-ChildItem -Path $mihomoExtract -Filter "*.exe" -Recurse | Select-Object -First 1
    if ($exe) {
        Copy-Item $exe.FullName -Destination "$pkgDir\mihomo-nodes.exe" -Force
        Write-Host "  -> 已下载并配置 mihomo-nodes.exe" -ForegroundColor Green
    } else {
        Write-Warning "未能解压出 mihomo 二进制文件！"
    }
}

# 复制或下载 config 目录（Country.mmdb 与 config.yaml）
$dstConfig = Join-Path $pkgDir "config"
New-Item -ItemType Directory -Path $dstConfig -Force | Out-Null
$mmdbCopied = $false
if (-not [string]::IsNullOrWhiteSpace($DependencySource)) {
    $localMmdb = Join-Path $DependencySource "config\Country.mmdb"
    if (Test-Path $localMmdb) {
        Copy-Item $localMmdb -Destination $dstConfig -Force
        Write-Host "  -> 已从本地复制 Country.mmdb" -ForegroundColor Green
        $mmdbCopied = $true
    }
}
if (-not $mmdbCopied) {
    Write-Host "  -> 正在从官方源下载 Country.mmdb..." -ForegroundColor Cyan
    $mmdbUrl = "https://github.com/MetaCubeX/meta-rules-dat/releases/download/latest/country.mmdb"
    Download-FileReliable -Url $mmdbUrl -OutFile "$dstConfig\Country.mmdb"
    Write-Host "  -> 已下载 Country.mmdb" -ForegroundColor Green
}
Set-Content -Path "$dstConfig\config.yaml" -Value "mixed-port: 7890" -Encoding utf8
Write-Host "  -> 已生成纯净 config 目录" -ForegroundColor Green

# 复制或下载 subconverter 目录
$subconverterCopied = $false
if (-not [string]::IsNullOrWhiteSpace($DependencySource)) {
    $localSub = Join-Path $DependencySource "subconverter"
    if (Test-Path "$localSub\subconverter.exe") {
        Copy-Item $localSub -Destination $pkgDir -Recurse -Force
        Write-Host "  -> 已从本地复制 subconverter 引擎目录" -ForegroundColor Green
        $subconverterCopied = $true
    }
}
if (-not $subconverterCopied) {
    Write-Host "  -> 正在从官方源下载 subconverter..." -ForegroundColor Cyan
    $subUrl = "https://github.com/asdlokj1qpi233/subconverter/releases/download/v0.9.9/subconverter_win64.7z"
    try {
        $headers = @{ 'User-Agent' = 'PowerShell-nodesCatchNext' }
        if ($env:GITHUB_TOKEN) { $headers['Authorization'] = "Bearer $env:GITHUB_TOKEN" }
        $resp = Invoke-RestMethod -Uri "https://api.github.com/repos/asdlokj1qpi233/subconverter/releases/latest" -Headers $headers -TimeoutSec 10
        $asset = $resp.assets | Where-Object { $_.name -eq 'subconverter_win64.7z' } | Select-Object -First 1
        if ($asset -and $asset.browser_download_url) {
            $subUrl = $asset.browser_download_url
        }
    } catch { }

    Write-Host "     下载地址: $subUrl"
    $sub7z = Join-Path $tempDownloads "subconverter.7z"
    Download-FileReliable -Url $subUrl -OutFile $sub7z
    
    $sevenZip = "7z"
    if (-not (Get-Command 7z -ErrorAction SilentlyContinue)) {
        if (Test-Path "C:\Program Files\7-Zip\7z.exe") {
            $sevenZip = "C:\Program Files\7-Zip\7z.exe"
        }
    }
    $subExtract = Join-Path $tempDownloads "sub_extract"
    New-Item -ItemType Directory -Path $subExtract -Force | Out-Null
    & $sevenZip x $sub7z -o"$subExtract" -y | Out-Null
    
    if (Test-Path "$subExtract\subconverter\subconverter.exe") {
        Copy-Item "$subExtract\subconverter" -Destination $pkgDir -Recurse -Force
    } elseif (Test-Path "$subExtract\subconverter.exe") {
        $dstSubconverter = Join-Path $pkgDir "subconverter"
        New-Item -ItemType Directory -Path $dstSubconverter -Force | Out-Null
        Copy-Item "$subExtract\*" -Destination $dstSubconverter -Recurse -Force
    } else {
        Write-Warning "未能正确识别解压后的 subconverter 目录结构！"
    }
    Write-Host "  -> 已下载并解压 subconverter 目录" -ForegroundColor Green
}

# 清理临时下载目录
if (Test-Path $tempDownloads) {
    Remove-Item -Path $tempDownloads -Recurse -Force
}

# 4. 压缩为发布 ZIP 包（包裹一级目录 nodesCatchNext-windows-x64）
Write-Host "`n[4/4] 压缩生成 ZIP 发行包..." -ForegroundColor Yellow
Compress-Archive -Path $pkgDir -DestinationPath $zipPath -Force

Write-Host "`n==========================================" -ForegroundColor Green
Write-Host " 发行版打包成功！" -ForegroundColor Green
Write-Host " 输出文件: $zipPath" -ForegroundColor Green
Write-Host " 文件大小: $([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
