# ==============================================================================
# nodesCatchNext 一键打包发行版脚本
# ==============================================================================
param (
    [string]$Version = "v4.0.0",
    [string]$DependencySource = "C:\Users\qiuxtao\Downloads\nodesCatchNext-v4.0"
)

$ErrorActionPreference = "Stop"
$repoRoot = $PSScriptRoot
$distRoot = Join-Path $repoRoot "dist"
$pkgName = "nodesCatchNext-$Version-windows-x64"
$pkgDir = Join-Path $distRoot $pkgName
$zipPath = Join-Path $distRoot "$pkgName.zip"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " 开始构建 nodesCatchNext 发行版 [$Version]" -ForegroundColor Cyan
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

# 3. 复制核心程序文件
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

# 复制 Mihomo 核心
$mihomoPath = Join-Path $DependencySource "mihomo-nodes.exe"
if (Test-Path $mihomoPath) {
    Copy-Item $mihomoPath -Destination $pkgDir -Force
    Write-Host "  -> 已复制 mihomo-nodes.exe" -ForegroundColor Green
} else {
    Write-Warning "未找到 mihomo-nodes.exe，请确认依赖源路径: $DependencySource"
}

# 复制 config 目录（剔除临时缓存与临时文件）
$srcConfig = Join-Path $DependencySource "config"
$dstConfig = Join-Path $pkgDir "config"
if (Test-Path $srcConfig) {
    New-Item -ItemType Directory -Path $dstConfig -Force | Out-Null
    if (Test-Path "$srcConfig\Country.mmdb") {
        Copy-Item "$srcConfig\Country.mmdb" -Destination $dstConfig -Force
    }
    if (Test-Path "$srcConfig\config.yaml") {
        Copy-Item "$srcConfig\config.yaml" -Destination $dstConfig -Force
    } else {
        Set-Content -Path "$dstConfig\config.yaml" -Value "mixed-port: 7890" -Encoding utf8
    }
    Write-Host "  -> 已复制并初始化纯净 config 目录" -ForegroundColor Green
}

# 复制 subconverter 目录
$srcSubconverter = Join-Path $DependencySource "subconverter"
$dstSubconverter = Join-Path $pkgDir "subconverter"
if (Test-Path $srcSubconverter) {
    Copy-Item $srcSubconverter -Destination $dstSubconverter -Recurse -Force
    Write-Host "  -> 已复制 subconverter 引擎目录" -ForegroundColor Green
}

# 4. 压缩为发布 ZIP 包
Write-Host "`n[4/4] 压缩生成 ZIP 发行包..." -ForegroundColor Yellow
Compress-Archive -Path "$pkgDir\*" -DestinationPath $zipPath -Force

Write-Host "`n==========================================" -ForegroundColor Green
Write-Host " 发行版打包成功！" -ForegroundColor Green
Write-Host " 输出文件: $zipPath" -ForegroundColor Green
Write-Host " 文件大小: $([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
