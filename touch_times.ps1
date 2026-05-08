# touch_times.ps1
# Назначение:
#   Проставить ВСЕМ файлам (и подпапкам) в каталоге, где лежит этот скрипт,
#   случайные значения LastWriteTime / CreationTime / LastAccessTime в течение
#   последнего часа от момента запуска. Время у каждого файла будет своё -
#   выглядит как естественная работа над проектом.
#
# Запуск:
#   powershell -ExecutionPolicy Bypass -File .\touch_times.ps1
#   ИЛИ просто двойной клик по .ps1 (если разрешено).
#
# Параметр:
#   -Minutes <int>  - окно назад от текущего времени (по умолчанию 60).

param(
    [int]$Minutes = 60
)

# Авто-определение каталога, где лежит сам скрипт (работает на любом ПК).
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $root) { $root = Get-Location }

Write-Host "Корневая папка: $root"
Write-Host "Окно: последние $Minutes минут от $(Get-Date -Format 'HH:mm:ss')"

$now = Get-Date
$rng = New-Object System.Random

# Берём все файлы и папки, кроме самого скрипта, чтобы не сломать ему хэш.
$selfPath = $MyInvocation.MyCommand.Path
$items = Get-ChildItem -Path $root -Recurse -Force -ErrorAction SilentlyContinue |
         Where-Object { $_.FullName -ne $selfPath }

$count = 0
foreach ($it in $items) {
    # Случайное смещение в секундах: от -Minutes*60 до 0.
    $offset = -1 * $rng.Next(1, $Minutes * 60)
    $stamp  = $now.AddSeconds($offset)
    try {
        $it.LastWriteTime  = $stamp
        $it.CreationTime   = $stamp
        $it.LastAccessTime = $stamp
        $count++
    } catch {
        Write-Warning "Не удалось обновить: $($it.FullName) - $($_.Exception.Message)"
    }
}

Write-Host "Готово. Обработано элементов: $count"
