# KtMControl

KtMControl is a small Windows tray utility for controlling the mouse with the keyboard.
It lets you quickly "drill down" into the screen by repeatedly selecting one of 9 regions with the numpad, then perform a mouse click without touching the mouse.

The main goal we're trying to achieve here is next:
Many interfaces allow keyboard navigation, but sometimes there is that special button that is completely unselectable without grabbing the mouse or it's unstable/unusable.
Sometimes it could even be that the button is on the same place on the screen, if it's a part of some fixed UI - that would be the best scenario for this app.
You'll have to remember once the path to it, something like ctrl-9-5-6-7-/ and then it would land in your memory the same way any other keyboard combination does.

## How it works

### Navigation mode
- Hold **Ctrl**
- Press one of the numpad keys **1–9**
- You'll be navigated to the corresponding zone and see a drilldown help - keep pressing until you reach your goal
- Then press numpad / for left click, numpad \* for middle click, numpad - for right click
- If you need to change the active screen, press numpad 0
- Release Ctrl when you want to end or restart the navigation

## Details

The current active area is divided into a **3 × 3 grid**:

7 8 9
4 5 6
1 2 3

When you press a numpad key:
- the cursor moves to the **center** of the chosen region
- that region becomes the new **active area**
- red guide lines are shown to visualize the current 3 × 3 split

You can keep pressing numpad keys while still holding **Ctrl** to continue narrowing down the target area.

### Active area reset
- While **Ctrl is held**, the current active area stays active
- When **Ctrl is released**, navigation mode ends:
  - the guide overlay is hidden
  - the active area resets to the screen containing the mouse cursor

### Mouse clicks
After positioning the cursor, while still holding **Ctrl** you can click using:

- **Ctrl + NumPad /** → left click
- **Ctrl + NumPad \*** → middle click
- **Ctrl + NumPad -** → right click

When a click is performed:
- the app temporarily releases Ctrl for the injected click
- performs the mouse click
- restores the previously held Ctrl key state
- resets navigation mode immediately

This helps avoid target applications interpreting the action as **Ctrl+Click**.

## Visual guidance
The app draws a temporary transparent overlay on the active monitor area with red guide lines showing the current 3 × 3 split.

The overlay stays visible **while Ctrl is held**.

## App behavior
- Runs as a **tray application**
- Registers global hotkeys so it works even when not focused
- Shows a tray icon with a single menu item:
  - **Exit**

## Requirements
- Windows
- .NET 10
- WinForms

## Notes
- If a hotkey cannot be registered, another application may already be using it
- The app must be running for the hotkeys to work
- The utility currently targets numpad-based navigation

