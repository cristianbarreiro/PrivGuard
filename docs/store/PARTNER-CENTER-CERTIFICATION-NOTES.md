# PrivGuard 2.0 — Partner Center Certification Notes (Fase 8B.1D)

Pegar el siguiente texto en el campo **Notes for certification** dentro de la sección de envío de Microsoft Partner Center. El texto está redactado en inglés para facilitar la revisión del equipo internacional de certificación de aplicaciones de Microsoft:

---

```text
APPLICATION OVERVIEW:
PrivGuard is a local-first Windows desktop privacy utility packaged as an MSIX application using the Desktop Bridge. It runs entirely as a standard user process (asInvoker) with zero administrative elevation, zero background services, and zero network activity.

RESTRICTED CAPABILITY (runFullTrust):
The package declares only 'rescap:runFullTrust'. This is required to access native Windows COM APIs for audio volume control (Core Audio WASAPI / IAudioEndpointVolume) and WMI (Win32_PnPEntity) to detect hardware capture devices. The package does not declare 'allowElevation' or 'unvirtualizedResources'.

DELIMITATION OF STORE EDITION FUNCTIONALITY:
1. Camera Privacy:
   Under Microsoft Store architectural policies, PrivGuard Store Edition does NOT attempt elevated driver disabling or low-level registry mutations. Instead, the Camera tile provides a direct 'Settings' button that opens native Windows Camera Privacy Settings (ms-settings:privacy-webcam) for transparent user management.

2. Microphone Privacy Muting:
   Microphone privacy control silences active capture endpoints using the official Windows Core Audio WASAPI interface (IAudioEndpointVolume::SetMute). It applies to shared-mode streams. The UI transparently communicates hardware capabilities and limitations.

3. Uninstallation Advisory:
   Because the Windows MSIX deployment engine does not execute arbitrary app uninstallation code, PrivGuard displays an informational warning banner whenever the microphone is muted, advising the user to unmute their device prior to uninstalling the application.

STEP-BY-STEP TESTING INSTRUCTIONS FOR CERTIFICATION TESTERS:
1. Launch the Application:
   - Start PrivGuard from the Start Menu.
   - Observe that the application starts immediately with normal user privileges without triggering any User Account Control (UAC) elevation prompt.

2. Verify Camera Settings Link:
   - In the main window, locate the Camera section.
   - Click the 'Settings' button next to the camera status.
   - Verify that the Windows native Camera Privacy Settings page opens (ms-settings:privacy-webcam).

3. Verify Microphone Mute & Unmute:
   - In the Microphone section, click the toggle to silence the active capture endpoint.
   - Observe that the endpoint reflects the muted status via Windows Core Audio.
   - Notice the orange advisory banner informing the user to unmute before uninstalling.
   - Click the toggle again to unmute the microphone, restoring it to its original unmuted state.

4. Verify Lifecycle and Tray Integration:
   - Click the window close button ('X') in the top-right corner.
   - Observe that the application minimizes cleanly to the Windows Notification Area (System Tray) rather than abruptly terminating.
   - Right-click the PrivGuard shield icon in the System Tray and select 'Exit' to cleanly close the application.

CONTACT & REPOSITORY:
Source Repository: https://github.com/cristianbarreiro/PrivGuard
Support: https://github.com/cristianbarreiro/PrivGuard/issues
```
