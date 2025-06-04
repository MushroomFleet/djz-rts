# ⚠️ Null Command RTS - v0.0.1 EXPERIMENTAL

> **WARNING: This is an extremely early experimental prototype that barely works. Expect crashes, bugs, and incomplete features.**

![Version](https://img.shields.io/badge/version-v0.0.1-red.svg)
![Status](https://img.shields.io/badge/status-experimental-orange.svg)
![.NET](https://img.shields.io/badge/.NET-6.0--windows-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)

A proof-of-concept real-time strategy game prototype using placeholder colored squares. This is NOT a playable game yet.

## 🚨 Current State (Brutal Honesty)

### What "Works" (Sort Of)
- ✅ You can place a green square (hospital) with Shift+Click
- ✅ Green square slowly changes color (construction "animation")
- ✅ Clicking the hospital spawns red squares (soldiers)
- ✅ Red squares move around randomly
- ✅ Blue squares spawn from screen edges and wander toward your stuff
- ✅ F12 hides the window (boss mode - the most reliable feature)

### What Doesn't Work (Most Things)
- ❌ Combat is barely implemented
- ❌ Squad formation is hit-or-miss
- ❌ Selection system is wonky
- ❌ AI is extremely basic
- ❌ Health/regeneration may or may not work
- ❌ No sound, no real graphics, no polish
- ❌ Probably crashes if you look at it wrong

### Known Issues
- May not start on some systems
- Click detection is unreliable
- Units get stuck or disappear
- Memory leaks likely
- Performance is terrible
- Global mouse hooks may interfere with other apps

## 🎯 The Vision (What It's Supposed to Become)

Eventually this might become a proper RTS game where you:
- Build a medical facility base
- Spawn and command soldier squads
- Fight against enemy waves
- Use actual graphics instead of colored squares

But right now it's just colored rectangles moving around.

## 🔧 Installation (Good Luck)

### Requirements
- Windows 10/11
- .NET 6.0 Runtime
- Patience
- Low expectations

### To Run (If It Runs)
```bash
cd null-command
dotnet run
```

Or try the batch file:
```
run-game.bat
```

### If It Doesn't Work
1. Install .NET 6.0 Runtime
2. Pray to the debugging gods
3. Try running as Administrator
4. Check if Windows Defender quarantined it
5. Give up and wait for v0.0.2

## 🎮 Controls (Theoretical)

| Input | What Should Happen | What Actually Happens |
|-------|-------------------|---------------------|
| Shift + LMB | Place hospital | Sometimes works |
| Shift + LMB on hospital | Spawn soldier | May spawn, may not |
| Shift + LMB on soldier | Select unit | Selection is unpredictable |
| LMB | Move selected unit | Unit might move somewhere |
| F12 | Hide window | Actually works reliably |

## 🧪 What's Actually Implemented

### Game Objects
- **Hospital**: 64x64 green rectangle that changes shades
- **Player Soldiers**: 16x16 red rectangles that move around
- **Enemy Soldiers**: 16x16 blue rectangles from screen edges
- **Selection**: Yellow border (when it works)

### Systems That Exist
- Canvas rendering system
- Basic entity management
- Mouse input handling (unreliable)
- Timer-based game loop
- Placeholder animation system

### Systems That Don't Exist Yet
- Proper collision detection
- Reliable AI pathfinding
- Combat mechanics
- Sound system
- Save/load functionality
- Anything resembling game balance

## 🐛 Debugging This Mess

### Common Issues
- **Nothing happens when I click**: Mouse detection is flaky
- **Game window disappears**: You probably hit F12 by accident
- **Red squares won't move**: Selection system is broken
- **Performance is awful**: Everything is inefficient
- **Crashes randomly**: Welcome to v0.0.1

### Debug Info
- Check console output for exceptions
- Task Manager to kill if it hangs
- System tray might have orphaned processes

## 🚧 Development Status

### Barely Working
- [x] Basic WPF application structure
- [x] Canvas-based rendering
- [x] Global mouse hooks (somewhat)
- [x] Entity positioning system
- [x] Basic timer loops

### Not Working
- [ ] Reliable input handling
- [ ] Proper game logic
- [ ] Combat system
- [ ] AI behaviors
- [ ] Performance optimization
- [ ] Error handling
- [ ] User experience

### Pipe Dreams
- [ ] Actual graphics
- [ ] Sound effects
- [ ] Multiple unit types
- [ ] Real RTS mechanics
- [ ] Stability
- [ ] Fun gameplay

## 🎨 Future Plans (If This Gets Fixed)

This repository exists to:
1. Prove the basic concept could work
2. Test WPF for overlay games
3. Experiment with RTS mechanics
4. Eventually add real art assets
5. Maybe become an actual game someday

See `ART-HANDOFF.txt` for completely premature art specifications.

## 🤝 Contributing

If you're brave enough to work on this broken prototype:
1. Fork the repo
2. Try to make something work better
3. Submit a PR with fixes
4. Marvel at how many things are broken

### Priority Fixes Needed
- [ ] Reliable mouse input detection
- [ ] Fix entity selection system
- [ ] Implement actual combat mechanics
- [ ] Add error handling everywhere
- [ ] Optimize performance
- [ ] Make it not crash

## 📝 License

MIT License - Feel free to use this code as a learning example of how NOT to structure a game initially.

## ⚠️ Disclaimer

This software is provided "as is" with absolutely no warranties. It may:
- Crash your system
- Interfere with other applications
- Consume excessive resources
- Not work at all
- Cause frustration and confusion

Use at your own risk. This is a prototype, not production software.

---

**If you got this far and it actually ran, congratulations! You've experienced the full extent of Null Command RTS v0.0.1.**

*Next goal: Make it crash less often.*
