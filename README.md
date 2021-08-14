# InputSync
An Android + Windows app to use an Android device as a mouse and keyboard.

The InputSync Android app can be used to send characters and mouse controls to a Windows PC that is running the InputSync.Cli program.

## Using the Programs

The latest prebuilt binaries can be grabbed from the release tab, or the projects can be built from scratch.

To build from scratch, make sure you have the .NET 4.7.2 and Xamarin Android components installed in Visual Studio, then you can just build the solution like normal.

## Desktop App

The desktop app (InputSync.Cli) runs a simple UDP server that listens for input from the Android app.

The windows app accepts a couple of arguments when it's started. They can either be passed in as arguments when starting it from the command line or defined in a file called `config.json`.

Usage:

```bat
InputSync.Cli.exe [option]*
```

### Command Line Arguments

| Option | Value | Description |
| --- | --- | --- |
| p\|port= | int | The port value to run the server on. |
| m\|mouse_sensitivity | double | Change the virtual mouse sensitivity using a scale value. |
| h\|help | | Shows a help message. |


#### Example Usage

```bat
InputSync.Cli.exe --port=45321 -m 1.5
```

### Config File

The `config.json` needs to be placed in the same directory as the program, and it will be used to set the run arguments. Any CLI arguments will overwrite the config values.

```json
{
  "Port": 45321,
  "MouseSensitivity": 1.5
}
```

## InputSync App

The InputSync app sends keypresses, mouse controls, and volume controls to the host PC.

### Connect

The InputSync app starts on a Connect screen. Enter the Host PC address and the port that the InputSync.Cli app is running on. Check the Save Settings checkbox to save the connection settings in between settings.

Click Connect when the information has been filled in.

### Input

The Input screen is where the magic happens.

The Keyboard Input textbox will send any characters typed in it to the host PC. The textbox will be cleared as soon as the value is sent. It is able to handle UTF-8 characters (e.g. emojis) and backspaces in addition to normal characters.

The Mouse Area works similarly to a laptop touchpad.

* Tapping the touch area will send a mouse click.
* Double tapping will send a double click.
* Pressing down and moving will move the mouse.
* Pressing with two fingers and moving up or down will scroll in the respective direction on the PC.

The Left and Right buttons on the bottom of the screen act as left and right mouse buttons. They can be held down to simulate holding the mouse button down, or quickly tapped to simulate a mouse click.

The volume buttons on the Android device can be used to adjust the volume on the host PC.
