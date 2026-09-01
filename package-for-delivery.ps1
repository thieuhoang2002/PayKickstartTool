# Script tự động đóng gói tool để bàn giao
$ErrorActionPreference = "Stop"

# Tên folder đích
$packageName = "PayKickstart-Tool"
$packagePath = ".\$packageName"

Write-Host "=== BẮT ĐẦU ĐÓNG GÓI ===" -ForegroundColor Cyan

# Xóa folder cũ nếu có
if (Test-Path $packagePath) {
    Write-Host "Xóa folder cũ..." -ForegroundColor Yellow
    Remove-Item $packagePath -Recurse -Force
}

# Tạo folder mới
Write-Host "Tạo folder $packageName..." -ForegroundColor Green
New-Item -ItemType Directory -Path $packagePath | Out-Null

# Copy file exe
Write-Host "Copy file .exe..." -ForegroundColor Green
Copy-Item ".\publish\tool-create-account-paykickstart.exe" -Destination $packagePath

# Copy folder Data
Write-Host "Copy folder Data/..." -ForegroundColor Green
Copy-Item ".\Data" -Destination $packagePath -Recurse

# Copy folder Extensions
Write-Host "Copy folder Extensions/..." -ForegroundColor Green
Copy-Item ".\Extensions" -Destination $packagePath -Recurse

# Tạo folder Results rỗng
Write-Host "Tạo folder Results/..." -ForegroundColor Green
New-Item -ItemType Directory -Path "$packagePath\Results" | Out-Null

# Copy README
Write-Host "Copy README.md..." -ForegroundColor Green
Copy-Item ".\README.md" -Destination $packagePath

# Thống kê
Write-Host "`n=== HOÀN TẤT ===" -ForegroundColor Cyan
$totalSize = (Get-ChildItem $packagePath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Đã tạo folder: $packagePath" -ForegroundColor Green
Write-Host "Tổng dung lượng: $([math]::Round($totalSize, 2)) MB" -ForegroundColor Green

# Liệt kê nội dung
Write-Host "`nNội dung:" -ForegroundColor Yellow
Get-ChildItem $packagePath -Recurse -Depth 1 | Select-Object @{Name="Path";Expression={$_.FullName.Replace($PWD.Path + "\$packageName\", "")}}, @{Name="Type";Expression={if($_.PSIsContainer){"Folder"}else{"File"}}}, @{Name="Size";Expression={if(!$_.PSIsContainer){[math]::Round($_.Length/1MB,2).ToString() + " MB"}else{"-"}}} | Format-Table -AutoSize

# Hỏi có muốn nén thành zip không
$compress = Read-Host "`nBạn có muốn nén thành file ZIP không? (y/n)"
if ($compress -eq "y") {
    $zipPath = ".\$packageName.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Write-Host "Đang nén..." -ForegroundColor Cyan
    Compress-Archive -Path $packagePath -DestinationPath $zipPath
    $zipSize = (Get-Item $zipPath).Length / 1MB
    Write-Host "✅ Đã tạo file: $zipPath ($([math]::Round($zipSize, 2)) MB)" -ForegroundColor Green
}

Write-Host "`n✅ SẴN SÀNG BÀN GIAO!" -ForegroundColor Green
