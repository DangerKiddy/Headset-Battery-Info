# Headset Battery Info

Application for monitoring your VR headset's battery level and state via external overlays (such as [OVR Toolkit](https://store.steampowered.com/app/1068820/OVR_Toolkit/) or [XSOverlay](https://store.steampowered.com/app/1173510/XSOverlay/)).\
Originally planned to work with [APK on headset](https://github.com/DangerKiddy/Headset-Battery-Info-Sender), but also works with Pico's Streaming Assistant.\
![image](https://github.com/DangerKiddy/Headset-Battery-Info/assets/42438297/0e4fdd75-ea4f-433e-9507-a63c0cdd61f1)\
You can find external DLL module for OVR Toolkit [here](https://github.com/DangerKiddy/Headset-Battery-Info-OVRToolkit)\
For XSOverlay there is no feature for adding custom modules yet. 

## How to use
- Download latest release from [this page](https://github.com/DangerKiddy/Headset-Battery-Info/releases) and extract archive anywhere you want to.
- Launch Headset Battery Info
- Choose one of the battery tracking method down bellow

### APK on the headset
If you're not using Pico's Streaming Assistant, then you can install APK on your headset and pair it with your PC to keep track of headset's battery state. For that you should follor [this tutorial](https://github.com/DangerKiddy/Headset-Battery-Info-Sender)

### Pico's Streaming Assistant battery tracking
This method allows you to track your battery state without installing APK on your headset if you're using Pico 4/Pro and Pico's Streaming Assistant.\
Make sure to change battery track method in settings:\
![1](https://github.com/DangerKiddy/Headset-Battery-Info/assets/42438297/7824f2f4-a15b-4993-a75d-aa1db275b1be)\
And then you can open your Streaming Assistant and pair your headset (if you didn't do that before launching Headset Battery Info).\
![2](https://github.com/DangerKiddy/Headset-Battery-Info/assets/42438297/06581a82-6766-4a53-9551-79f9150c6f6a)

# OSC
## Outputs
Application sends OSC messages with current battery info to VRChat (port 9000 by default)
| Address | Animator Parameter | Value Type |
| ------- | ------------------ | ---------- |
| /avatar/parameters/controllerLeftBatteryLevel | controllerLeftBatteryLevel | float `0.0 - 1.0` |
| /avatar/parameters/controllerRightBatteryLevel | controllerRightBatteryLevel | float `0.0 - 1.0` |
| /avatar/parameters/headsetBatteryLevel | headsetBatteryLevel | float `0.0 - 1.0` |
| /avatar/parameters/isHeadsetCharging | isHeadsetCharging | bool `False / True` |

## Inputs
Application can also receive OSC messages on 28092 port, currently available only one option
| Address | Description | Value Type |
| ------- | ----------- | ---------- |
| /hbi/requestUpdate | Forces HBI to re-send current battery info to VRChat(port 9000 by default) and overlay application (such as OVR Toolkit) | None |
