# PrivGuard 2.0 â€” Partner Center Submission Checklist

This document guides the developer through the manual configuration steps in **Microsoft Partner Center** (`https://partner.microsoft.com/dashboard/products/windows/overview`).

> [!CAUTION]
> **Authorization & Pre-Approval Boundary:** Complete steps 1 through 6 locally and in draft mode. **Do NOT click "Submit for certification" (Step 7)** until:
> 1. You have sent the pre-approval inquiry in [docs/store/MICROSOFT-ALLOWELEVATION-PREAPPROVAL.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/MICROSOFT-ALLOWELEVATION-PREAPPROVAL.md) to `reportapp@microsoft.com`.
> 2. Microsoft has provided written confirmation regarding `allowElevation` and `unvirtualizedResources` eligibility for your Individual Developer Account.

---

## Step 1: Partner Center Sign-In & Product Selection

1. Sign in to [Microsoft Partner Center](https://partner.microsoft.com/dashboard).
2. Navigate to **Apps and games** > **Windows & Xbox** > **Overview**.
3. Select your reserved product: **PrivGuard** (Store ID: `9NQ56N45XH5N`).
4. In the app sidebar, go to **Product management** > **Product Identity**.
5. Confirm the registered values match:
   - **Package Name:** `cdevstudios.PrivGuard`
   - **Publisher ID:** `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`
   - **Publisher display name:** `cdev studios`
   - **Package Family Name (PFN):** `cdevstudios.PrivGuard_tv7b4yqg10sw2`

---

## Step 2: Configure Submission Sections

Click **Start your submission** (or edit current draft submission).

### 2.1 Pricing and Availability
- **Markets:** All possible markets (or select preferred distribution regions).
- **Discoverability:** Make this product available in Microsoft Store (Public).
- **Schedule:** Release as soon as certification is approved (or specify date).
- **Pricing:** Free (Tier: Free).
- Click **Save**.

### 2.2 Properties
- **Primary Category:** `Security`
- **Subcategory:** `Personal security`
- **Secondary Category:** `Utilities & tools`
- **Privacy Policy URL:**
  `https://github.com/cristianbarreiro/PrivGuard/blob/main/docs/store/PRIVACY-POLICY.md`
- **Support Contact / Website:**
  `https://github.com/cristianbarreiro/PrivGuard/issues`
- **Hardware Requirements:**
  - Keyboard: Required
  - Mouse: Required
  - Touch: Optional
  - Camera: Required for camera protection
  - Microphone: Required for microphone protection
- **Declarations:**
  - Mixed Reality PC: **Disabled / Unchecked**
  - HoloLens: **Disabled / Unchecked**
  - App records or broadcasts user video/audio: **Unchecked** (PrivGuard does NOT capture, record, or broadcast media)
  - OneDrive automatic backup: **Unchecked** (Leave unchecked until WAL session restore behavior across OneDrive sync is validated)
  - Accessibility declaration: **Unchecked** (Unless formally audited)
  - Alternate drive installation: **Default (System Drive only)**
- Click **Save**.

### 2.3 Personal Information & Privacy Disclosure Form
- **Question:** *"Does your product access, collect, or transmit personal information?"*
- **Recommended Answer:** **Yes**
- **Explanation to Provide in Partner Center:**
  > *"PrivGuard queries the local Windows Security Identifier (SID) of the logged-in user strictly to construct kernel-level Access Control Lists (ACLs) for local inter-process Named Pipes between its standard user interface and an ephemeral on-demand worker. It also accesses local Plug and Play hardware device identifiers (device instance IDs and Core Audio endpoint GUIDs) to identify and restore device states. PrivGuard contains zero telemetry, zero cloud storage, declares no internet capabilities, and transmits zero data over the network."*
- Click **Save**.

### 2.4 Age Ratings (IARC Questionnaire)
- Enter your contact email.
- Category: **Utility, Productivity, Communication or Other**.
- Questionnaire answers:
  - Does the app contain violence? **No**.
  - Does the app contain sexual content or nudity? **No**.
  - Does the app contain profanity or crude humor? **No**.
  - Does the app promote drugs, alcohol, or tobacco? **No**.
  - Does the app allow users to communicate with others? **No**.
  - Does the app share user physical location? **No**.
  - Does the app allow digital goods purchases? **No**.
- Generate rating: Result will be **All Ages (PEGI 3, ESRB Everyone, USK 0)**.
- *Notice:* If the questionnaire was invalidated by recent Partner Center schema updates, re-generate and confirm the certificate.
- Click **Save and continue**.

### 2.5 Packages
1. Drag and drop the Store Release MSIX package:
   `release\store\PrivGuard-2.0.0.0-x64.msix`
   *(SHA-256: See latest release hash recorded in PHASE-7-MICROSOFT-STORE-REPORT.md)*
2. Wait for the upload and validation to complete.
3. Verify that Partner Center displays:
   - Version: `2.0.0.0`
   - Architecture: `x64`
   - Capabilities: `runFullTrust`, `allowElevation`, `unvirtualizedResources`
4. Click **Save**.

### 2.6 Store Listings

#### English (United States) â€” Default
- Copy text from [docs/store/STORE-LISTING-CONTENT.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/STORE-LISTING-CONTENT.md) Section 2:
  - Product title: `PrivGuard`
  - Subtitle / Short description
  - Full description
  - Features (bullet points)
  - Search keywords (7 terms)
  - Copyright: `Copyright Â© 2026 cdev studios. All rights reserved.`
- **Upload Visual Assets:**
  - App tile icon / Store logo: `release\store\StoreLogo.png`
  - Screenshots (Desktop):
    1. `release\store\mainwindow_dark.png`
    2. `release\store\protection_overview.png`
    3. `release\store\tray_menu.png`

#### Spanish (Spain) â€” Add Language
1. Click **Add language** > Select **Spanish (Spain)**.
2. Copy text from [docs/store/STORE-LISTING-CONTENT.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/STORE-LISTING-CONTENT.md) Section 3:
  - TÃ­tulo: `PrivGuard`
  - SubtÃ­tulo
  - DescripciÃ³n completa
  - CaracterÃ­sticas principales
  - Palabras clave de bÃºsqueda
3. Upload screenshots.
4. Click **Save**.

### 2.7 Submission Options & Certification Notes
1. Open the **Notes for certification** section.
2. Copy the entire contents of [docs/store/REVIEWER-CERTIFICATION-NOTES.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/REVIEWER-CERTIFICATION-NOTES.md).
3. In the **Restricted Capabilities Declaration** section:
   - Copy the justifications for `runFullTrust`, `allowElevation`, and `unvirtualizedResources` as detailed in [docs/store/MICROSOFT-STORE-JUSTIFICATION.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/MICROSOFT-STORE-JUSTIFICATION.md).
   - Reference the pre-approval inquiry sent to `reportapp@microsoft.com`.
4. Click **Save**.

---

## Step 3: Submission Preflight Checklist

Confirm all sections in Partner Center show a green checkmark:
- [ ] Pricing and availability: Saved (Free)
- [ ] Properties: Saved (Category: Security > Personal security, Declarations verified)
- [ ] Privacy questionnaire: Completed (Disclosed local SID/PnP, 0% network transmission)
- [ ] Age ratings: Completed (IARC Certificate confirmed)
- [ ] Packages: Validated (`PrivGuard-2.0.0.0-x64.msix`)
- [ ] Store listings: Completed (`en-us` and `es-es` with screenshots)
- [ ] Submission options: Reviewer notes and capability justifications pasted

---

## Step 4: STOP & Pre-Approval Hold (Boundary for Submission)

> [!CAUTION]
> **DO NOT CLICK "Submit for certification".**
> Send the pre-approval request in `docs/store/MICROSOFT-ALLOWELEVATION-PREAPPROVAL.md` to `reportapp@microsoft.com`.
> Wait for Microsoft Store's written guidance on capability approval for Individual Accounts before submitting.
