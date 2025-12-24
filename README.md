# MacroDroid PC Notification
This program will display native toast notifications on Windows for Google Messages phone notifications. This was created for personal use.

## Requirements
- Windows 10 or 11
- Android phone with [MacroDroid](https://play.google.com/store/apps/details?id=com.arlosoft.macrodroid)

## Program
- Simply launch the executable and it will begin listening for broadcasts in the background.
- The program has no user-interface, so must be killed with Task Manager (named `MacroDroid-PC-Notification`).
- Add a shortcut to `%AppData%\Microsoft\Windows\Start Menu\Programs\Startup` to have the program launch with Windows.

## Macro
### Triggers
- Notification Received, with "Prevent multiple triggers" disabled: `Any Content (Messages)`
### Variables
- `prev_dict : dictionary`: tracks most recent message per contact, to prevent accidental re-triggers
- `not_dict : dictionary`: contains data to be transformed into JSON
- `not_json : string`: JSON sent over the network to the program
### Actions
- Set Variable `prev_dict[{not_title}]: {notification}`
- Set Variable: `not_dict[not_app]: {not_app_name}`
- Set Variable: `not_dict[not_title]: {not_title}`
- Set Variable: `not_dict[not_text]: {notification}`
- JSON Output: `not_dict -> not_json`
- UDP Command: `YourComputerIP:5000 - {lv=not_json}`
### Constraints
- Wifi Connected: `[YourWifi]`
- Compare Values: `{not_title} != ""`
- Compare Values: `{not_title} != Message not delivered`
- Compare Values: `{notification} != ""`
- Compare Values: `{notification} != {lv=prev_dict[{not_title}]}`

## Further notes
- The program uses the following NuGet packages: `Microsoft.Toolkit.Uwp.Notifications` and `Newtonsoft.Json`.
- The JSON sent over the network is not secure. Malicious actors could intercept sensitive messages, however it is unlikely.
- I use my computer's IPv4 address so that the JSON is only sent within my network, hence the "Wifi Connected" constraint.
