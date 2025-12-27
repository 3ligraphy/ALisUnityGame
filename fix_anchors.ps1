$files = @(
    'Assets\Scenes\Menu.unity',
    'Assets\Scenes\Map 4 Cinema\cinema.unity',
    'Assets\Scenes\Map 4 Cinema\cinema Gate.unity',
    'Assets\Scenes\Map 3 gemstones\gemstones Gate.unity',
    'Assets\Scenes\Map 2 Wells\wells Gate.unity',
    'Assets\Scenes\Map 1 Museum\Museum Gate.unity'
)

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        $newContent = $content -replace 'm_AnchorMax: \{x: 0\.5, y: 1\}', 'm_AnchorMax: {x: 1, y: 1}'
        Set-Content -Path $file -Value $newContent -NoNewline
        Write-Host "Fixed: $file"
    } else {
        Write-Host "Not found: $file"
    }
}

Write-Host "`nAll panel anchors fixed!"
