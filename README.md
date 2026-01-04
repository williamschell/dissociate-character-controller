# Dissociate Character Controller

A modular first-person parkour focused character controller for my Unity game Dissociate. It features climbing, wall-running, wall-jumping, dashing, sliding and many more mechanics.
Designed for flexibility and extensibility, this system works for prototyping, games or any personal projects. It is easy to remove features and add any new features you'd like!

---

## Features
- **Movement:** Walking, sprinting, acceleration in the way you are looking for easy direction changes.
- **Jumping:** Standard jumping, implemented with both coyote time and jump buffering.
- **Dashing:** Slope-aware sliding that is impacted by friction and speed caps, gives the player a customizable boost.
- **Wall Climbing:** Climbs walls with ledge vaulting (gives a boost if sprinting) and climbing limitations. Also a wall jumping feature, which turns the player 180 degrees.
- **Wall Running:** Run along walls with custom gravity changes, tilts the camera and allows for wall jumping (useful for the player jumping between wall runs).
- **Camera:** Uses a first person cinemachine camera with adjustable sensitivity and dynamic changes in FOV.

- **EXTENSIBLE:** I designed this so that new movement mechanics can easily be plugged into the system.

## Installation
1. Clone or download this repository
2. Copy the scripts into your unity project
3. Attach the scripts to your Player GameObject
4. Make sure the player has a CharacterController object
5. Customize the script parameters in the inspector
6. now you have a kickass character controller

## License
This project is licensed under the MIT License See the [LICENSE](LICENSE) file for details.
