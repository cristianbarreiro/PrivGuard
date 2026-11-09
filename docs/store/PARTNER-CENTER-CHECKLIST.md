# PrivGuard 2.0 — Partner Center Submission Checklist

This document guides the developer through the manual configuration steps in **Microsoft Partner Center** (`https://partner.microsoft.com/dashboard/products/windows/overview`).

> [!CAUTION]
> **Authorization & Pre-Submission Boundary:** Complete steps 1 through 3 locally and in draft mode. **Do NOT click "Submit for certification" (Step 4)** until:
> 1. Real physical storage of the Full Trust MSIX package (`LocalCache\Local` vs native AppData) is measured and validated in a clean VM environment.
> 2. The dual-edition packaging pipeline (Phase 8B) is implemented and the Store MSIX package passes Windows App Certification Kit (WACK) with zero errors.
> 3. The developer grants explicit authorization to proceed with submission.

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
  - Microphone: Required for microphone muting features
  - Camera: Optional (for inspection and settings navigation)
- **Declarations:**
  - Mixed Reality PC: **Disabled / Unchecked**
  - HoloLens: **Disabled / Unchecked**
  - App records or broadcasts user video/audio: **Unchecked** (PrivGuard does NOT capture, record, or broadcast media)
  - OneDrive automatic backup: **Unchecked** (Leave unchecked until cloud sync behavior of session files is validated)
  - Accessibility declaration: **Unchecked** (Unless formally audited)
  - Alternate drive installation: **Default (System Drive only)**
- Click **Save**.

### 2.3 Personal Information & Privacy Disclosure Form
- **Question:** *"Does your product access, collect, or transmit personal information?"*
- **Recommended Answer:** **Yes**
- **Explanation to Provide in Partner Center:**
  > *"PrivGuard inspects local Core Audio endpoint GUIDs and device identifiers strictly to display device status and manage microphone muting via official Windows COM APIs. PrivGuard contains zero telemetry, zero analytics, declares no network capabilities, and transmits zero data over the internet."*
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
2. Wait for the upload and validation to complete.
3. Verify that Partner Center displays:
   - Version: `2.0.0.0`
   - Architecture: `x64`
   - Capabilities: `runFullTrust` (Sole capability declared for Store Edition per D-01)
4. Click **Save**.

### 2.6 Store Listings

#### English (United States) — Default
- Copy text from [docs/store/STORE-LISTING-CONTENT.md](STORE-LISTING-CONTENT.md) Section 2:
  - Product title: `PrivGuard`
  - Subtitle / Short description
  - Full description
  - Features (bullet points)
  - Search keywords (7 terms)
  - Copyright: `Copyright © 2026 cdev studios. All rights reserved.`
- **Upload Visual Assets:**
  - App tile icon / Store logo: `release\store\StoreLogo.png`
  - Screenshots (Desktop):
    1. `release\store\mainwindow_dark.png`
    2. `release\store\protection_overview.png`
    3. `release\store\tray_menu.png`

#### Spanish (Spain) — Add Language
1. Click **Add language** > Select **Spanish (Spain)**.
2. Copy text from [docs/store/STORE-LISTING-CONTENT.md](STORE-LISTING-CONTENT.md) Section 3:
   - Título: `PrivGuard`
   - Subtítulo
   - Descripción completa
   - Características principales
   - Palabras clave de búsqueda
3. Upload screenshots.
4. Click **Save**.

### 2.7 Submission Options & Certification Notes
1. Open the **Notes for certification** section.
2. Copy the entire contents of [docs/store/REVIEWER-CERTIFICATION-NOTES.md](REVIEWER-CERTIFICATION-NOTES.md).
3. In the **Capabilities Declaration** section:
   - Paste the justification for `runFullTrust` from [docs/store/MICROSOFT-STORE-JUSTIFICATION.md](MICROSOFT-STORE-JUSTIFICATION.md).
4. Click **Save**.

---

## Step 3: Submission Preflight Checklist

Confirm all sections in Partner Center show a green checkmark:
- [ ] Pricing and availability: Saved (Free)
- [ ] Properties: Saved (Category: Security > Personal security, Declarations verified)
- [ ] Privacy questionnaire: Completed (Disclosed local device identifiers, 0% network transmission)
- [ ] Age ratings: Completed (IARC Certificate confirmed)
- [ ] Packages: Validated (`PrivGuard-2.0.0.0-x64.msix` with `runFullTrust`)
- [ ] Store listings: Completed (`en-us` and `es-es` with screenshots)
- [ ] Submission options: Reviewer notes and capability justification pasted

---

## Step 4: STOP & Submission Hold (Boundary for Submission)

> [!CAUTION]
> **DO NOT CLICK "Submit for certification".**
> Maintain submission on **CONDITIONAL HOLD** until:
> 1. Storage validation in VM confirms clean isolation for Desktop Bridge Full Trust.
> 2. Phase 8B builds the final Store package with WACK passing cleanly.
> 3. Final review and authorization is provided by the project lead.
