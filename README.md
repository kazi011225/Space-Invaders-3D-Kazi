# Space Invaders 3D - Assignment 1

CSCI 381, Video Game Design and Development, Fall 2026
Kazi Anwar

Made with Unity 6.6 (6000.6.7f1). Open `Assets/Scenes/SampleScene` and press Play.

## How to play
- Left and right arrow keys to move
- Space to shoot
- Space to start, Space to go to the next level, R to start over
- Beat all 3 levels to win

## Required features
- **Aliens speed up** as they are destroyed. The fleet goes from a start speed to a max speed based on how many aliens are left. (`AlienFleet.AlienDestroyed`)
- **Game over when aliens get too low.** If any alien reaches the line just above the ship, the game can't be won anymore, so it ends. (`AlienFleet.FixedUpdate`)
- **Ship can't go offscreen.** The ship's x position is clamped to the camera's view, so it works on any window size. (`Spaceship.Update`)

## Code issues I fixed
1. **Pointless GetComponent call** (`Alien.cs`). `fleet` was already an `AlienFleet`, but the code called `fleet.GetComponent<AlienFleet>()` to get the same thing again. Now it uses `fleet` directly.
2. **UI text rebuilt every frame** (`AlienFleet.Update` and `AlienCounter.Update`). The counter made a new string 60+ times a second even when nothing changed. Now the text only updates when an alien dies or the player loses a life.
3. **Empty Start() and Update() methods** (`Alien`, `AlienBullet`, `Dart`, `Spaceship`). Unity still calls empty ones every frame for every object, so I removed them.
4. **Edge check bug** (`AlienFleet.FixedUpdate`). `left` and `right` started at 0 instead of real alien positions, so the check was wrong when all aliens were on one side of the screen. Now they start at `float.MaxValue` / `float.MinValue`.
5. **Invoke with a string name** (`AlienFleet`). `Invoke("startSidewaysMotion")` breaks with no error if the method gets renamed. Now it uses `nameof(StartSidewaysMotion)`, and the method name follows C# naming style.
6. **Two loops over the same list** (`AlienFleet.FixedUpdate`). Moving the aliens and finding the edges are now done in one loop.
7. **Unused using lines** removed from `AlienFleet.cs` and `AlienCounter.cs`.
8. **Alien counter was offscreen on smaller windows.** It was placed at a fixed spot from the center. I anchored it to the top left corner so it shows on any screen size.
9. **Ship got knocked crooked when hit.** The original code destroyed the ship on the first hit, so this never showed. With lives, the ship survives, and the bullet's physics push turned it. Now the ship is kinematic with rotation frozen, so only our code moves it. (`Spaceship.Start`)

## Extra features (picked 6, 4 needed)
- **Balance:** player can't spam shots anymore (cooldown 0.05s to 0.4s), aliens shoot 2x more often, alien bullets are faster, and the fleet moves faster. Still beatable with 3 lives.
- **Lives:** player has 3 lives shown at the top of the screen. After a hit, the ship blinks and is safe for 1.5 seconds, so a group of bullets cannot take all your lives at once. (`Spaceship.Hit`)
- **Start and end screen:** the game waits on a start screen until Space is pressed. The end screen shows YOU WIN or GAME OVER with how many aliens were destroyed, and R restarts. (`GameScreens.cs`)
- **New background:** Helix Nebula image from NASA (images.nasa.gov), on an unlit quad behind the game.
- **Additional levels:** 3 levels. Each level adds a row of aliens (3, 4, then 5 rows), makes the fleet 20% faster, and changes the alien color (green, yellow, red). Clearing a level shows a LEVEL CLEAR screen. The level is a static value so it survives the scene reload, and it resets to 1 every time Play is pressed. (`GameScreens.cs`, `AlienFleet.Start`)
- **Collision animations:** when an alien is destroyed, it bursts into small pieces in its own color. When the ship gets hit, it gives an orange burst, and a bigger one on the last life. The pieces fly out, slow down, and shrink away. Their colliders are turned off right away, so they can't hit anything. (Unity doesn't allow `DestroyImmediate` during a collision, so the collider is disabled first and deleted at the end of the frame.) (`Explosion.cs`)
