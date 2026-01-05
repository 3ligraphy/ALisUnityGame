# Fix iPad popup issues in all Gate scenes
# Issue 1: Panel scale is 0.29 (squished to 30% width)
# Issue 2: Submit button may be off-screen

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
        
        # Fix 1: Reset panel scale to normal (1, 1, 1) for proper iPad display
        # The popup panel was scaled to 0.29 width which makes it too narrow
        $content = $content -replace 'm_LocalScale: \{x: 0\.29\d*, y: 0\.98\d*, z: 0\.20\d*\}', 'm_LocalScale: {x: 1, y: 1, z: 1}'
        
        # Fix 2: Move Submit button up so it's not cut off on iPad
        # Change y position from -472 to -200 (more centered)
        $content = $content -replace 'm_AnchoredPosition: \{x: 0, y: -472\}', 'm_AnchoredPosition: {x: 0, y: -200}'
        
        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "  Fixed popup scale and button position in $file"
    } else {
        Write-Host "  Not found: $file"
    }
}

Write-Host ""
Write-Host "All Gate scenes fixed for iPad compatibility!"
