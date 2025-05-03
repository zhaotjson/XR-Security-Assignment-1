# XR Security Assignment 1

## Overview

This is an AR spaceship game built in Unity. The player controls a ship that follows the device camera in an AR environment. The game features voice commands for shooting, a persistent high score system, and a dynamic sky dome that matches the real sky above the player's location.

## Features

- **Voice Commands (Text-to-Speech):**
  - Announces scores

- **High Scores API:**
  - High scores are stored and retrieved from a Node.js backend server.
  - Scores persist between sessions and can be viewed by all players.

- **Astronomy Sky from Location API:**
  - The game fetches a real star map image based on the player's GPS coordinates using Astronomy API.
  - This image is displayed on a dome above the AR world, so the sky matches the real sky above the player.

## How to Run

### 1. Start the Node.js High Score Server

```sh
cd server
npm install
node index.js
```
- The server will run on `http://localhost:3000` by default.

### 2. Start ngrok (to expose your local server to the internet)

```sh
.\ngrok.exe http 3000
```
- Copy the HTTPS forwarding URL provided by ngrok (e.g., `https://abcd1234.ngrok.io`).
- Update your Unity project or config to use this URL for high score API requests.

### 3. Build and Run the Unity Project

- Open the project in Unity.
- Assign your AstronomyAPI credentials in the `SkyboxFromLocation` script.
- Assign your hemisphere prefab for the sky dome.
- Build and run on your AR-capable device.

## API Details

### Text-to-Speech & Speech-to-Text

- Uses native platform APIs (iOS: SFSpeechRecognizer, Android: SpeechRecognizer).
- Allows hands-free control of shooting in the game.

### High Scores API

- Custom Node.js/Express backend.
- Stores and retrieves player high scores via REST endpoints.
- Accessible via ngrok for remote devices.

### Astronomy Sky from Location

- Uses [AstronomyAPI](https://astronomyapi.com/) to fetch a real-time star map image based on the player's GPS.
- The image is mapped onto a hemisphere above the AR scene.


## Notes

- Make sure to grant microphone, location, and speech recognition permissions on your device.
- For AstronomyAPI, set your application origin to match your development environment (e.g., `http://localhost` or your ngrok URL).
- The sky dome only covers the upper portion of the AR world, so you can still see your environment.

## Credits

- AstronomyAPI for sky images.
- Unity, AR Foundation, and platform-native speech APIs.
- Node.js and Express for the high score backend.

Extra Assets:
https://pixabay.com/sound-effects/laser-45816/

https://www.fontspace.com/pixelated-elegance-font-f126145

https://tools.wwwtyro.net/space-3d/index.html#animationSpeed=1&fov=80&nebulae=true&pointStars=true&resolution=1024&seed=5hpkc8epc7k0&stars=true&sun=true



