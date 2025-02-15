![](./Media/titlecard_hero01.png)

# Whisperer Voice SDK Demo
Whisperer is a Unity VR game experience using the *[Voice SDK](https://developer.oculus.com/documentation/unity/voice-sdk-overview/)* for interactions. This repository includes the complete buildable Unity project.

## Requirements
- Unity [2022.3.8f1](https://unity3d.com/unity/whats-new/2022.3.8) or newer
- Windows or Mac
- Meta Quest 2, Quest Pro (standalone) or Rift (PCVR)

## Getting Started

### Getting the code
Ensure you have [Git LFS](https://git-lfs.github.com/) installed:

```
git lfs install
```

Clone this repo or download it as a zip file.

```
git clone https://github.com/oculus-samples/voicesdk-samples-whisperer.git
```

All of the project files can be found in `Assets/Whisperer`. This folder includes all scripts and assets to run the experience. This project depends on [Voice SDK v59](https://developer.oculus.com/downloads/package/meta-voice-sdk/).

### Configuring Wit.ai

Using *Whisperer* reqiures a [Wit.ai](https://wit.ai) account.

1. Once logged in, on [wit.ai/apps](https://wit.ai/apps), click *New App* and import the [zipped app backup](https://github.com/wit-ai/voicesdk_samples_whisperer/blob/main/Assets/whisperer-wit-app.zip) included in this repo. The GitHub sample will be listed on the My Apps page.

2. Then find the `Server Access Token` in your wit.ai app setup under `Managment > Settings` from the left navigation panel. Go to Unity Editor, in the toolbar find `Meta > Voice SDK > Get Started`, select `Custom App`, and paste in your `Server Access Token`, click `Create` and choose a location to store the new app configuration, and wait until the Wit Configurations tab in the Voice Hub is fully populated.

3. Now go to the `Assets/Whisperer/Scenes/Loader` scene, find the `Management` game object under Hierarchy. Click on the `Management` game object under Inspector and find the `App Voice Experience (Script)` and  `TTS Wit (Script)`.

    * Expand `App Voice Experience (Script) > Wit Runtime Configuration`, select the wit.ai app configuration you just created.
    * Expand `TTS Wit (Script) > Request Settings`, select the same wit.ai app configuration.

For more information on setting up an App, check out the [Wit.ai Quickstart](https://wit.ai/docs/quickstart).

> **Note:** Wit.ai will need to train its model before it's ready to use. On Wit.ai, the current status of the training is indicated by the dot next to the app name.

### Run the game
You can run the game in-editor or on a Quest headset.

1. To run *Whisperer* in-editor, after configuring Wit.ai (see below), open the project in Unity. Then open the `Assets/Whisperer/Scenes/Loader` scene and press play.

2. To run *Whisperer* on the Quest headset, go to `File > Build Settings`, Choose `Android` Platform, click `Switch Platform`, making sure the headset is connected, then click `Build And Run`. Please consult [Set Up Development Environment and Headset](https://developer.oculus.com/documentation/unity/unity-env-device-setup/) for more details.

## How To Play
*Whisperer's* introduction will help guide you, through narrative instruction and visual prompts, how to interact with objects using your hands and voice.

- When you raise your hands in front of you, as if to speak through them, the microphone will be automatically activated and the voice SDK will listen to you. You can then speak to various objects, telling them to move, open, turn on, etc.

- The inset menu button on the left controller (`☰`) will open the in-game panel displaying an instruction card and demonstration video, as well as buttons to restart the current level or return the starting scene.

### Unity Editor Play Mode Navigation

- To move the camera around, use your mouse.

- To select an object, look towards it and hold down space key.

- To jump from one level to another, use 1, 2, 3, and 4 keys on the keyboard.

## Project Structure

The `Loader` scene contains two game objects that persist throughout the entire experience: `Player Rig` and `Management`.

The `Player Rig` is the XR Origin, and contains the necessary components for Unity's XR Interaction Toolkit, as well as the [`SpeakGestureWatcher.cs` ](Assets/Whisperer/Scripts/Voice/SpeakGestureWatcher.cs) component and any UI canvases.

Attached to the Management game object are [`AppVoiceExperience.cs`](https://developers.meta.com/horizon/reference/voice/latest/class_oculus_voice_app_voice_experience/) (part of Voice SDK) and [`LevelLoader.cs`](Assets/Whisperer/Scripts/Logic/LevelLoader.cs). LevelLoader additively loads the necessary Unity scenes for each level, unloading them when a level is completed.

#### Level Loader

Each level consists of two scenes additively loaded by the levelLoader -- a base scene containing all static geometry and non-interactable objects, and a level scene containing all scene logic, animated objects, and listenable objects for that particular level.

Every level contains a Level Manager prefab and a Listenables prefab. The Level Manager is responsible for that scene's logic and instantiates a VoiceUI prefab for any objects derived from Listenable.cs at [Start()](Assets/Whisperer/Scripts/Logic/LevelLoader.cs#L58).

#### App Voice Experience

AppVoiceExperience is the core component of the Voice SDK. It holds the reference to the Wit.ai App Config asset, sends data to Wit.ai for processing, and responds with the appropriate Unity Events. When an object derived from Listenable.cs is selected and deselected by the player, it subscribes and unsubscribes to the events on AppVoiceExperience.

## Voice SDK

*Whisperer* utilizes several different methods of handling responses from Wit.ai. Depending on the type of interaction (`action`) we're trying to resolve, we use either `intents`, `entities`, or manual parsing of the text transcription.

To determine when to activate and deactivate Wit.ai, the [`SpeakGestureWatcher.cs` ](Assets/Whisperer/Scripts/Voice/SpeakGestureWatcher.cs) component checks the position of the tracked hand controllers and raycasts for objects that contain the [`Listenable.cs`](Assets/Whisperer/Scripts/Voice/Listenable.cs) class. If the player's hands are in position and an object is found, `AppVoiceExperience.Activate()` is called. If at any time the player breaks the pose, Wit.ai is deactivated.

The `AppVoiceExperience` class itself is initated in the [`LevelManager.cs`](Assets/Whisperer/Scripts/Logic/LevelManager.cs) parent class which all subsequent levels inherit from.

If an object derived from [`Listenable.cs`](Assets/Whisperer/Scripts/Voice/Listenable.cs) is selected and the player says something, the *Whisperer* will wait for a response from Wit.ai, then read the ```WitResponseNode``` to determine the action to be taken.

> Example: If a [`ForceMovable.cs`](Assets/Whisperer/Scripts/Voice/Listenable%20Objects/ForceMovable.cs) is selected and the utterance "*Move right a lot*" is detected by Wit.ai, we read the intent and entities from the `WitResponseNode` to determine the direction and strength of move force applied. *Whisperer* reads the returned intent (`move`), direction entity (`right`) and strength entity (`strong`) and performs an appropriate action.

## Conduit Implementation

This code base uses [Conduit framework](https://developer.oculus.com/documentation/unity/voice-sdk-conduit/) from Voice SDK.

To use Conduit, simply annotate the callback method with the `MatchIntent` attribute. When changes are made to the callback method, such as adding, removing, or changing it, Unity generates a new manifest file. Please note that `Use Conduit` should be checked in your wit config asset file [as documented here](https://developer.oculus.com/documentation/unity/voice-sdk-conduit/#benefits-to-using-conduit).

For example, in [HeroPlant.cs](Assets/Whisperer/Scripts/Logic/HeroPlant.cs#L68) the `Move` method takes two parameters: a `ForceDirection` enum and a `WitResponseNode`.

In this code `Move` is decorated with the `MatchIntent` attribute, with intent value `move`. By using the [Conduit framework](https://developer.oculus.com/documentation/unity/voice-sdk-conduit/), these callback methods can be automatically registered without the need for manual registration.


```csharp
[MatchIntent("move")]
public override void Move(ForceDirection direction, WitResponseNode node)
{
    // method implementation
}
```


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
[The Oculus License](https://developer.oculus.com/licenses/oculussdk) applies to the SDK and supporting material.
The MIT licence applies to the files and assets in the Assets/Whisperer folder.
Otherwise, if an individual file does not indicate which license it is subject to, then the Oculus License applies.
