# Terminal Animation Demo Requirements

This file documents what the terminal animation demo should showcase:

## Animation Content

**Environment**: Windows Terminal Preview on WSL Ubuntu session

**Sequence**:
1. Start with empty terminal showing PowerShell prompt
2. Type the command:
   ```powershell
   "<svg width='100' height='100'><circle cx='50' cy='50' r='40' fill='#ff6b6b' stroke='#333' stroke-width='3'/></svg>" | ConvertTo-Png | ConvertTo-Sixel -stream
   ```
3. Show the command execution
4. Display the rendered circle appearing directly in the terminal using Sixel graphics
5. Show the PowerShell prompt returning, demonstrating the pipeline completed
6. Optionally show a second example with the test.svg file:
   ```powershell
   ConvertTo-Png -Path "test.svg" | ConvertTo-Sixel -stream
   ```

## Technical Notes

- The animation should be saved as `terminal-demo.gif` and placed in repository assets
- Ideal duration: 10-15 seconds
- Should show clear terminal text and the rendered graphics
- Demonstrates the power of PwrSvg for terminal-based image rendering workflows

## Key Demonstration Points

1. **Pipeline Integration**: SVG string → PNG → Sixel rendering in one command
2. **Terminal Graphics**: Direct image rendering in terminal using Sixel protocol
3. **Zero File I/O**: Processing happens entirely in memory streams
4. **PowerShell Integration**: Native cmdlet integration with familiar PowerShell syntax

This showcases the unique value proposition of PwrSvg for terminal-based graphics workflows.