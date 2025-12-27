#!/usr/bin/env python3
"""
Fix Unity UI Button Anchors for iPad Compatibility

This script finds all UI elements anchored to screen edges (x: 1)
and centers them horizontally for proper iPad scaling.
"""

import re
from pathlib import Path

def fix_button_anchors(file_path):
    """Fix right-edge anchored buttons to center in Unity scene file"""
    
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    changes = []
    
    # Pattern 1: Fix horizontal right-edge min anchor (x: 1) → (x: 0.5)
    pattern1 = r'm_AnchorMin: \{x: 1, y: ([\d.]+)\}'
    replacement1 = r'm_AnchorMin: {x: 0.5, y: \1}'
    content, count1 = re.subn(pattern1, replacement1, content)
    if count1 > 0:
        changes.append(f"  - Fixed {count1} AnchorMin values (x: 1 → x: 0.5)")
    
    # Pattern 2: Fix horizontal right-edge max anchor (x: 1) → (x: 0.5)
    pattern2 = r'm_AnchorMax: \{x: 1, y: ([\d.]+)\}'
    replacement2 = r'm_AnchorMax: {x: 0.5, y: \1}'
    content, count2 = re.subn(pattern2, replacement2, content)
    if count2 > 0:
        changes.append(f"  - Fixed {count2} AnchorMax values (x: 1 → x: 0.5)")
    
    # Pattern 3: Reset horizontal position to 0 for centered elements
    # Find elements with negative X position (offset from right edge)
    # Change to x: 0 (centered)
    pattern3 = r'm_AnchoredPosition: \{x: -[\d.]+, y: ([\d.-]+)\}'
    
    def replace_position(match):
        y_val = match.group(1)
        return f'm_AnchoredPosition: {{x: 0, y: {y_val}}}'
    
    content, count3 = re.subn(pattern3, replace_position, content)
    if count3 > 0:
        changes.append(f"  - Reset {count3} AnchoredPosition X values to 0 (centered)")
    
    # Only write file if changes were made
    if content != original_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True, changes
    
    return False, []

def fix_canvas_scalers(file_path):
    """Fix Canvas Scaler match mode for better iPad scaling"""
    
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Fix match mode: 0 → 0.5 for balanced scaling
    pattern = r'm_MatchWidthOrHeight: 0\n'
    replacement = r'm_MatchWidthOrHeight: 0.5\n'
    content, count = re.subn(pattern, replacement, content)
    
    if content != original_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True, count
    
    return False, 0

def main():
    """Main function to process all Unity scene files"""
    
    scenes_dir = Path(r"c:\Users\amoha\Desktop\game kw 2026\ALisUnityGame-main\ALisUnityGame-main\Assets\Scenes")
    
    if not scenes_dir.exists():
        print(f"❌ Error: Scenes directory not found: {scenes_dir}")
        return
    
    print("=" * 60)
    print("Unity Button Anchor Fix Script")
    print("=" * 60)
    print()
    
    # Find all .unity files
    unity_files = list(scenes_dir.rglob("*.unity"))
    print(f"Found {len(unity_files)} Unity scene files\n")
    
    total_anchor_fixes = 0
    total_scaler_fixes = 0
    fixed_files = []
    
    for unity_file in sorted(unity_files):
        relative_path = unity_file.relative_to(scenes_dir)
        
        # Fix button anchors
        anchor_fixed, anchor_changes = fix_button_anchors(unity_file)
        
        # Fix canvas scalers
        scaler_fixed, scaler_count = fix_canvas_scalers(unity_file)
        
        if anchor_fixed or scaler_fixed:
            print(f"✅ {relative_path}")
            
            if anchor_changes:
                for change in anchor_changes:
                    print(change)
                total_anchor_fixes += len(anchor_changes)
            
            if scaler_fixed:
                print(f"  - Fixed Canvas Scaler match mode (0 → 0.5)")
                total_scaler_fixes += scaler_count
            
            print()
            fixed_files.append(str(relative_path))
    
    print("=" * 60)
    print("Summary")
    print("=" * 60)
    print(f"Files processed: {len(unity_files)}")
    print(f"Files modified: {len(fixed_files)}")
    print(f"Total anchor fixes: {total_anchor_fixes}")
    print(f"Total scaler fixes: {total_scaler_fixes}")
    print()
    
    if fixed_files:
        print("Modified files:")
        for file in fixed_files:
            print(f"  - {file}")
    
    print()
    print("✅ All fixes applied successfully!")
    print()

if __name__ == "__main__":
    main()
