# Complete iPad UI Fix Script
# Fixes all remaining issues for Apple App Store submission
# 1. InputField scale (0.39 -> 1)
# 2. Submit button scale (0.80 -> 1)  
# 3. Element Y positions - Submit below InputField
# 4. Intro scene panel scale

$gateFiles = @(
    'Assets\Scenes\Map 1 Museum\Museum Gate.unity',
    'Assets\Scenes\Map 2 Wells\wells Gate.unity',
    'Assets\Scenes\Map 3 gemstones\gemstones Gate.unity',
    'Assets\Scenes\Map 4 Cinema\cinema Gate.unity'
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "iPad UI Complete Fix Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Fix 1: InputField scale in all Gate scenes
Write-Host "FIX 1: InputField Y Scale (0.39 -> 1)" -ForegroundColor Yellow
foreach ($file in $gateFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        
        # Fix InputField scale - match the pattern for InputField's LocalScale
        # The InputField has scale {x: 1, y: 0.39457, z: 1} - change y to 1
        $content = $content -replace 'm_LocalScale: \{x: 1, y: 0\.39\d+, z: 1\}', 'm_LocalScale: {x: 1, y: 1, z: 1}'
        
        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "  Fixed InputField scale in: $file" -ForegroundColor Green
    } else {
        Write-Host "  Not found: $file" -ForegroundColor Red
    }
}
Write-Host ""

# Fix 2: Submit button scale in all Gate scenes
Write-Host "FIX 2: Submit Button Y Scale (0.80 -> 1)" -ForegroundColor Yellow
foreach ($file in $gateFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        
        # Fix Submit button scale - it has {x: 1, y: 0.80009, z: 1}
        $content = $content -replace 'm_LocalScale: \{x: 1, y: 0\.80\d+, z: 1\}', 'm_LocalScale: {x: 1, y: 1, z: 1}'
        
        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "  Fixed Submit button scale in: $file" -ForegroundColor Green
    } else {
        Write-Host "  Not found: $file" -ForegroundColor Red
    }
}
Write-Host ""

# Fix 3: Swap Y positions - InputField at -280, Submit at -420 (button below input)
Write-Host "FIX 3: Element Y Positions (Submit below InputField)" -ForegroundColor Yellow
foreach ($file in $gateFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        
        # Current: InputField at y: -374, Submit at y: -200
        # We want: InputField at y: -280, Submit at y: -420
        # This puts the input field higher and button lower
        
        # Move InputField from -374 to -280 (higher up)
        $content = $content -replace 'm_AnchoredPosition: \{x: 0, y: -374\}', 'm_AnchoredPosition: {x: 0, y: -280}'
        
        # Move Submit button from -200 to -420 (lower down, below input)
        $content = $content -replace 'm_AnchoredPosition: \{x: 0, y: -200\}', 'm_AnchoredPosition: {x: 0, y: -420}'
        
        # Also fix the feedback text position (was at -354, move it between input and button)
        $content = $content -replace 'm_AnchoredPosition: \{x: 0, y: -354\}', 'm_AnchoredPosition: {x: 0, y: -350}'
        
        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "  Fixed element positions in: $file" -ForegroundColor Green
    } else {
        Write-Host "  Not found: $file" -ForegroundColor Red
    }
}
Write-Host ""

# Fix 4: Intro scene panel scale
Write-Host "FIX 4: Intro Scene Panel Scale (0.29 -> 1)" -ForegroundColor Yellow
$introFile = 'Assets\Scenes\Intro.unity'
if (Test-Path $introFile) {
    $content = Get-Content $introFile -Raw
    
    # Fix the panel scale from {x: 1, y: 0.29215, z: 1} to {x: 1, y: 1, z: 1}
    $content = $content -replace 'm_LocalScale: \{x: 1, y: 0\.29\d+, z: 1\}', 'm_LocalScale: {x: 1, y: 1, z: 1}'
    
    Set-Content -Path $introFile -Value $content -NoNewline
    Write-Host "  Fixed panel scale in: $introFile" -ForegroundColor Green
} else {
    Write-Host "  Not found: $introFile" -ForegroundColor Red
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "All iPad UI fixes applied!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary of fixes:" -ForegroundColor White
Write-Host "  1. InputField vertical scale normalized (was 39%)" -ForegroundColor White
Write-Host "  2. Submit button vertical scale normalized (was 80%)" -ForegroundColor White
Write-Host "  3. Element positions corrected (Submit now below InputField)" -ForegroundColor White
Write-Host "  4. Intro scene panel scale normalized (was 29%)" -ForegroundColor White
Write-Host ""
Write-Host "Please rebuild and test on iPad before resubmitting to Apple." -ForegroundColor Yellow


