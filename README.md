# 3D Basketball Shooting Game

A 3D basketball shooting game built in Unity where the player throws a basketball into a hoop with a 60-second timer challenge.

## How to Play

- **Press R** to start the game
- **Click** on the ball to pick it up
- **Drag mouse** to aim (trajectory line shows where the ball will go)
- **Release mouse** to throw
- **Right click** to cancel aim
- **Press R** to restart after game over

## Core Mechanics

### Throw System
- Click-drag aiming with real-time trajectory preview
- Ball launch velocity calculated using projectile physics (kinematic equations)
- The throw animation syncs with the ball release via Animation Events
- Arc height adjusts based on distance to hoop for natural-feeling shots

### Dribble System
- Custom dribble simulation that follows the character's hand bone
- Ball bounces with configurable gravity and bounce speed
- Synced with Mixamo dribble animation
- Bounce sound plays on each ground hit

### Score Detection
- Trigger collider inside the net detects successful shots
- Swish detection: ball scores without hitting rim colliders = 3 points
- Normal score (ball hits rim first) = 2 points
- Rim colliders use Unity tags for efficient collision checking

### Game Loop
- 60-second timer challenge mode
- Score, streak, and total shots tracking
- Best streak recorded per session
- Ball auto-resets after scoring or missing (with smooth arc lerp back to hand)

## Juice & Polish

### Camera Effects
- Slow-motion zoom toward the hoop on every score
- Camera smoothly returns to original position after the effect

### UI Feedback
- Score popup text with shake animation on score
- Random encouraging messages on score: "NICE!", "BUCKET!", "SWISH!", "CASH!"
- Funny miss messages: "SKILL ISSUE!", "NOOB!", "GO PLAY CANDYCRUSH!", "BRUH..."
- Timer flashes red in the last 3 seconds
- Game over screen shows final stats (score, accuracy %, best streak)

### Pity Mode
- Miss 3 times in a row and the hoop moves closer to the player
- Shows "BRUH, STOP" message
- Arc height increases for easier shots
- Hoop resets to original position after scoring

### Animation System
- Dribble idle animation (looping)
- Throw animation with Animation Event for ball release timing
- After-throw idle animation
- Smooth transitions between states via Animator Controller triggers
- Root motion baked into pose to prevent rotation glitches

### Particle Effects
- Golden particle burst at the hoop on every score
- Particles created programmatically with sphere mesh rendering

### Audio
- Basketball bounce sound on ground/rim collision
- Bounce sound during dribble at lowest point
- Crowd cheering on score
- Crowd laughing when pity mode activates
- Centralized AudioManager subscribing to game events

## Architecture

### Scripts

| Script | Responsibility |
|--------|---------------|
| `GameManager.cs` | Singleton managing game state, score, timer, pity mode, and events |
| `BallController.cs` | Ball states (Dribble/Aim/WindUp/InFlight/Reset), throw physics, collision detection |
| `DribbleBall.cs` | Simulated dribble bounce following hand bone, independent of Rigidbody |
| `TrajectoryRenderer.cs` | Real-time trajectory arc preview using LineRenderer |
| `PlayerAnimator.cs` | Controls Animator triggers and relays Animation Events |
| `CameraController.cs` | Score camera zoom with slow-motion effect |
| `UIManager.cs` | All UI updates, prompt animations, timer display |
| `AudioManager.cs` | Centralized audio playback subscribing to game events |

### Event-Driven Design
Scripts communicate through C# events on GameManager:
- `OnScore` - score registered
- `OnMiss` - shot missed
- `OnThrow` - shot taken
- `OnGameStart` / `OnGameOver` - game state changes
- `OnPityMode` - pity mode activated
- `OnBallReset` - ball returned to player

This keeps scripts decoupled - UI, camera, and audio systems subscribe to events without direct references to each other.

### Physics Approach
- Ball uses Rigidbody for in-flight physics, switches to kinematic during dribble/aim
- Throw velocity calculated using projectile motion equations:
  - `peakY = max(startY, targetY) + arcHeight`
  - `timeUp = sqrt(2 * upDist / gravity)`
  - `vy = gravity * timeUp`
  - `vh = horizontalDistance / totalTime`
- Rim colliders are small spheres arranged in a circle with gaps for swish detection
- Physics materials control bounce behavior on ball, ground, and rim

## Tech Stack
- Unity 6 (6000.3.11f1)
- C# with Unity's legacy Input System
- Mixamo animations (Big Vegas character)
- No third-party interaction packages

## Controls Summary
| Input | Action |
|-------|--------|
| Left Click + Drag | Pick up ball and aim |
| Release Left Click | Throw |
| Right Click | Cancel aim |
| R | Start / Restart game |
