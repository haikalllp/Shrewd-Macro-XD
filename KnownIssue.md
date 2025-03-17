# Known Issues
- Plateaued on settings from 3 - 5 (Speed and strength from 3-5 is same)

- Macro config format need to be fixed, current:
```json
{
  "JitterStrength": 0,
  "RecoilReductionStrength": 0,
  "AlwaysJitterMode": false,
  "AlwaysRecoilReductionMode": false,
  "JitterEnabled": false,
  "RecoilReductionEnabled": false,
  "MinimizeToTray": false,
  "MacroKey": 20,
  "SwitchKey": 81,
  "ToggleType": 0
}
```
- Fix config name to settings.json
- fix macro key to actual name
- fix switch key to actual name
- allow for mix of keyboard and mouse for binding, e.g. (meaning macro key can be keyboard, switch key can be mouse. and vice versa.)

- make default settings to be:
```json
{
  "JitterStrength": 3,
  "RecoilReductionStrength": 1,
  "AlwaysJitterMode": false,
  "AlwaysRecoilReductionMode": false,
  "JitterEnabled": false,
  "RecoilReductionEnabled": false,
  "MinimizeToTray": false,
  "MacroKey": Capital,
  "SwitchKey": Q
}
```

- Ensure program create config in same location as exe
- Ensure JSON config created upon first time running app
- Ensure app updates config after every change is made
- Ensure config is actually updated and saved for next loading

- Repetition of implementation in InputSimulator.cs and JitterManager.cs
- Repetition of implementation in InputSimulator.cs and RecoilReductionManager.cs