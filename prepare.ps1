# prepare.ps1
# Назначение:
#   Подготовить только что скачанную папку проекта к работе:
#   1) удалить служебные .vs / bin / obj (если есть);
#   2) удалить любую существующую привязку к Git (.git);
#   3) создать ЧИСТЫЙ локальный git-репо и закоммитить ВСЁ,
#      чтобы Visual Studio в Solution Explorer не показывала
#      бейджи "Возвращено / Добавлено / Изменено" рядом с файлами.
#
#   Скрипт сам находит свою папку - кладите в корень проекта,
#   запускайте на любом ПК.
#
# Запуск:
#   powershell -ExecutionPolicy Bypass -File .\prepare.ps1
#
# Параметры:
#   -NoGit    - не создавать новый .git, просто удалить старый.
#               (тогда Solution Explorer вообще не будет показывать source-control)

param(
    [switch]$NoGit
)

# Авто-поиск каталога, где лежит сам скрипт.
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $root) { $root = (Get-Location).Path }
Set-Location $root

Write-Host "Корневая папка: $root"

# 1) Чистим временные/служебные папки.
foreach ($name in @('.vs', 'bin', 'obj')) {
    Get-ChildItem -Path $root -Recurse -Force -Directory -ErrorAction SilentlyContinue `
        | Where-Object { $_.Name -eq $name } `
        | ForEach-Object {
            try {
                Remove-Item $_.FullName -Recurse -Force -ErrorAction Stop
                Write-Host "  удалено: $($_.FullName)"
            } catch {
                Write-Warning "  не удалось удалить $($_.FullName) (возможно, открыт в VS)"
            }
        }
}

# 2) Удаляем существующий .git, если он есть.
if (Test-Path "$root\.git") {
    try {
        # снимаем атрибут "скрытый/только чтение", чтобы Remove-Item не споткнулся
        attrib -h -r -s "$root\.git" /S /D 2>$null | Out-Null
        Remove-Item "$root\.git" -Recurse -Force -ErrorAction Stop
        Write-Host "  удалена старая привязка .git"
    } catch {
        Write-Warning "  не удалось удалить .git: $($_.Exception.Message)"
    }
}

# 3) Если хотим - создаём чистый локальный репо с одним коммитом.
if ($NoGit) {
    Write-Host "Параметр -NoGit: новый репозиторий не создаётся."
} else {
    $gitExe = Get-Command git -ErrorAction SilentlyContinue
    if (-not $gitExe) {
        Write-Warning "git не найден в PATH - пропускаю инициализацию."
    } else {
        # минимальный .gitignore, чтобы потом bin/obj/.vs не висели как "новые"
        $ignore = @'
bin/
obj/
.vs/
*.user
*.suo
'@
        Set-Content -Path "$root\.gitignore" -Value $ignore -Encoding UTF8

        & git init -q -b main 2>&1 | Out-Null
        & git -c user.email=local@local -c user.name=local add -A 2>&1 | Out-Null
        & git -c user.email=local@local -c user.name=local commit -q -m "initial" 2>&1 | Out-Null
        Write-Host "  создан локальный git-репо (main, 1 коммит) - все файлы будут "чистыми"."
    }
}

Write-Host ""
Write-Host "Готово. Закройте и снова откройте решение в Visual Studio,"
Write-Host "чтобы Solution Explorer перечитал состояние файлов."
