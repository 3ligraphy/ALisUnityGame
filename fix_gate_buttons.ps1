$gateFiles = @(
    'Assets\Scenes\Map 1 Museum\Museum Gate.unity',
    'Assets\Scenes\Map 2 Wells\wells Gate.unity',
    'Assets\Scenes\Map 3 gemstones\gemstones Gate.unity',
    'Assets\Scenes\Map 4 Cinema\cinema Gate.unity'
)

foreach ($file in $gateFiles) {
    if (Test-Path $file) {
        Write-Host "Processing: $file"
        $content = Get-Content $file -Raw
        
        # Fix Submit button size - make it MUCH larger for touch
        $content = $content -replace 'm_SizeDelta: \{x: 600, y: 42\.7941\}', 'm_SizeDelta: {x: 700, y: 120}'
        $content = $content -replace 'm_SizeDelta: \{x: 600, y: 42\}', 'm_SizeDelta: {x: 700, y: 120}'
        $content = $content -replace 'm_SizeDelta: \{x: 600, y: 43\}', 'm_SizeDelta: {x: 700, y: 120}'
        $content = $content -replace 'm_SizeDelta: \{x: 600, y: 45\}', 'm_SizeDelta: {x: 700, y: 120}'
        $content = $content -replace 'm_SizeDelta: \{x: 600, y: 50\}', 'm_SizeDelta: {x: 700, y: 120}'
        $content = $content -replace 'm_SizeDelta: \{x: 600, y: 75\}', 'm_SizeDelta: {x: 700, y: 120}'
        
        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "  Fixed Submit button size in $file"
    } else {
        Write-Host "  Not found: $file"
    }
}

Write-Host ""
Write-Host "All Gate scene buttons resized for better touch detection!"
