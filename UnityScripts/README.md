### Unity Source Files

This folder contains the C# scripts used to support the Unity Engine for Raspberry Putt. 
There are other aspects to the Unity package, such as textures, initial values of objects, etc. 
However, these files are too large to include in the GitHub folder, so we only include the functional scripts.

# ArrowController.cs
Applies the necessary matrix transformations to the arrow to position it in front of the ball.
Oscillates the arrow.

# CameraManager.cs
Applies the necessary matrix transformations to the camera to position it above and behind the ball.

# ChatboxManager.cs
Controller for the chatbox.

# HotkeyManager.cs
Allows the user to quit the game with the 'esc' key. Will implement any other hotkey definitions (pause, etc.)

# StrokeManager.cs
The main controller for the game. Holds and modifies the game's state, and handles MQTT messages (might separate these functions into two separate controllers later).
Applies force to the ball.

# VoiceRecognizer.cs
Uses the KeywordRecognizer Unity API and user microphone to populate the chatbox.

