![](./Media/titlecard_hero01.png)

# Whisperer Voice SDK Demo
Whisperer is a Unity VR game experience using the *[Voice SDK](https://developer.oculus.com/documentation/unity/voice-sdk-overview/)* for interactions. This repository includes the complete buildable Unity project.

## Requirements
- Unity [2021.3.9f1](https://unity3d.com/unity/whats-new/2021.3.9)
- Windows or Mac
- Meta Quest 2 (standalone) or Rift (PCVR)

## Getting Started

Ensure you have Git LFS installed: 

```
git lfs install
```

Then, clone this repo using the "Code" button above, or with:
```
git clone git@github.com:wit-ai/voicesdk_samples_whisperer.git
```

All of the project files can be found in `Assets/Whisperer`. This folder includes all scripts and assets to run the experience, excluding those that are part of the Interaction SDK. The project includes v45 of the Voice SDK.

To run *Whisperer* in-editor, after configuring Wit.ai (see below), open the project in Unity [2021.3.9f1](https://unity3d.com/unity/whats-new/2021.3.9). Then open the `Assets/Scenes/Loader` scene and press play.

## Configuring Wit.ai

Using *Whisperer* reqiures a [Wit.ai](https://wit.ai) account.


1. Once logged in, on [wit.ai/apps](https://wit.ai/apps), click *New App* and import the [zipped app backup](https://github.com/wit-ai/voicesdk_samples_whisperer/blob/main/Assets/whisperer-wit-app.zip) included in this repo. 

2. Then find the `Server Access` and `Client Acess Tokens` your app setup under `Managment > Settings`. Enter these values in the appropriate fields on the Wit.ai App Config asset in the unity project. 

For more information on setting up an App, check out the [Wit.ai Quickstart](https://wit.ai/docs/quickstart).

> **Note:** Wit.ai will need to train it's model before it's ready to use. On Wit.ai, the current status of the training is indicated by the dot next to the app name.

## How To Play
*Whisperer's* introduction will help guide you, through narrative instruction and visual prompts, how to interact with objects using your hands and voice.

- When you raise your hands in front of you, as if to speak through them, the microphone will be automatically activated and the voice SDK will listen to you. You can then speak to various objects, telling them to move, open, turn on, etc.

- The inset menu button on the left controller (`☰`) will open the in-game panel displaying an instruction card and demonstration video, as well as buttons to restart the current level or return the starting scene.

## Voice SDK

*Whisperer* utilizes several different methods of handling responses from Wit.ai. Depending on the type of interaction (`action`) we're trying to resolve, we use either `intents`, `entities`, or manual parsing of the text transcription. 

To determine when to activate and deactivate Wit.ai, the [`SpeakGestureWatcher.cs` ](Assets/Whisperer/Scripts/Voice/SpeakGestureWatcher.cs) component checks the position of the tracked hand controllers and raycasts for objects that contain the [`Listenable.cs`](Assets/Whisperer/Scripts/Voice/Listenable.cs) class. If the player's hands are in position and an object is found, [`AppVoiceExperience.Activate()`](Assets/Oculus/Voice/Scripts/Runtime/Service/AppVoiceExperience.cs#L104-L113) is called. If at any time the player breaks the pose, Wit.ai is deactivated.

The AppVoiceExperience class itself is initated in the [`LevelManager.cs`](Assets/Whisperer/Scripts/Logic/LevelManager.cs) parent class which all subsequent levels inherit from.

If an object derived from [`Listenable.cs`](Assets/Whisperer/Scripts/Voice/Listenable.cs) is selected and the player says something, the *Whisperer* will wait for a response from Wit.ai, then read the ```WitResponseNode``` to determine the action to be taken.

> Example: If a [`ForceMovable.cs`](Assets/Whisperer/Scripts/Voice/Listenable Objects/ForceMovable.cs) is selected and the utterance "*Move right a lot*" is detected by Wit.ai, we read the intent and entities from the `WitResponseNode` to determine the direction and strength of move force applied. *Whisperer* reads the returned intent (`move`), direction entity (`right`) and strength entity (`strong`) and performs an appropriate action.


## Intents and Entities
These intents are used to move objects in the scene. The move, pull, push, and jump intents can be used with `strength` and `direction` entities. For example, "*Push away from me a little bit.*"

- `move`
- `pull`
- `push`
- `jump`
- `levitate`
- `drop`

The entity `direction` determines the direction an object is moved when accompanied with the move, pull, push, or jump intents.

- `right`
- `left`
- `up`
- `toward` (moves object toward the user)
- `away` (moves object away from the user)
- `wall` (moves object away from center of the room)
- `across` (moves object toward the center of the room)

The entity `move_strength` determies the strength of the force applied to an object when it's moved.

- `weak`
- `normal`
- `strong`

Generic intents for interacting with objects such as the radio, drawers, water hose or treasure chest:

- `open`
- `close`
- `turn_off`
- `turn_on`

Intents for interacting with specific objects, include:

- `turn_off_radio`
- `turn_on_radio`
- `change_station`
- `ask_bird_name`
- `bird_song`

The entity `color` is used to identify when the user has communicated a color selection to the bird in level 3.

- `yellow`
- `blue`
- `red`

## Licenses
The *Oculus Integration* package is released under the *[Oculus SDK License Agreement](https://developer.oculus.com/licenses/oculussdk)*.
The MIT licence applies to the files and assets in the Assets/Project folder.
Otherwise, if an individual file does not indicate which license it is subject to, then the Oculus License applies.
