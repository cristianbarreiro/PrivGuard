# PrivGvard 2.0 — Partner Center Submission Checklist

This document guides the developer through the manual steps in **Microsoft Partner Center** (`https://partner.microsoft.com/dashboard/products/windows/overview`).

> [!CAUTION]
> **Authorization Boundary:** Complete steps 1 through 6 locally and in draft mode. **Do NOT click "Submit for certification" (Step 7)** until you have reviewed the final submission summary and explicitly decided to publish.

---

## Step 1: Partner Center Sign-In & App Creation

1. Sign in to [Microsoft Partner Center](https://partner.microsoft.com/dashboard).
2. Navigate to **Apps and games** > **Windows & Xbox** > **Overview**.
3. Click **New product** > **MSIX or PWA app**.
4. **App Name Reservation:**
   - Enter `PrivGvard`.
   - Click **Check availability**.
   - If available, click **Reserve product name**.
   - *(Note: If `PrivGvard` is already reserved under your account, select it directly from your product list).*

---

## Step 2: Verify Package Identity

1. In the app sidebar, go to **Product management** > **Product Identity**.
2. Compare the values with the local configuration in `packaging/Package.appxmanifest`:
   - **Package Name:** `cdevstudios.PrivGvard`
   - **Publisher ID:** `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`
   - **Publisher display name:** `cdev studios`
   - **Package Family Name (PFN):** `cdevstudios.PrivGvard_tv7b4yqg10sw2`
3. If your Partner Center account has a different Publisher ID, re-run `scripts/build-msix.ps1` providing your official Publisher ID:
   ```powershell
   .\scripts\build-msix.ps1 -Architecture x64 -Publisher "CN=<YourPublisherID>" -PublisherDisplayName "<YourDisplayName>" -OutputDir "release\store"
   ```

---

## Step 3: Configure Submission Sections

Click **Start your submission** (or edit current draft submission).

### 3.1 Pricing and Availability
- **Markets:** All possible markets (or select preferred distribution regions).
- **Discoverability:** Make this product available in Microsoft Store (Public).
- **Schedule:** Release as soon as certification is approved (or specify date).
- **Pricing:** Free (Tier: Free).
- Click **Save**.

### 3.2 Properties
- **Category:** `Utilities & tools`
- **Subcategory:** `Security & privacy`
- **Privacy Policy URL:**
  `https://github.com/cristianbarreiro/PrivGuard/blob/main/docs/store/PRIVACY-POLICY.md`
- **Support Contact / Website:**
  `https://github.com/cristianbarreiro/PrivGuard/issues`
- **Hardware Requirements:** Leave default (touch, keyboard, mouse).
- Click **Save**.

### 3.3 Age Ratings (IARC Questionnaire)
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
- Click **Save and continue**.

### 3.4 Packages
1. Drag and drop the Store Release MSIX package:
   `release\store\PrivGvard-2.0.0.0-x64.msix`
   *(SHA-256: `0397F8C8918CD80E4D282CCDB12C6504B2E49F1E4B96C0C21CA8146A4963B542`)*
2. Wait for the upload and validation to complete.
3. Verify that Partner Center displays:
   - Version: `2.0.0.0`
   - Architecture: `x64`
   - Capabilities: `runFullTrust`, `allowElevation`, `unvirtualizedResources`
4. Click **Save**.

### 3.5 Store Listings

#### English (United States) — Default
- Copy text from [docs/store/STORE-LISTING-CONTENT.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/STORE-LISTING-CONTENT.md) Section 2:
  - Product title: `PrivGvard`
  - Subtitle / Short description
  - Full description
  - Features (bullet points)
  - Search keywords (up to 7 terms)
  - Copyright: `Copyright © 2026 cdev studios. All rights reserved.`
- **Upload Visual Assets:**
  - App tile icon / Store logo: `release\store\StoreLogo.png`
  - Screenshots (Desktop):
    1. `release\store\mainwindow_dark.png`
    2. `release\store\protection_overview.png`
    3. `release\store\tray_menu.png`

#### Spanish (Spain) — Add Language
1. Click **Add language** > Select **Spanish (Spain)**.
2. Copy text from [docs/store/STORE-LISTING-CONTENT.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/STORE-LISTING-CONTENT.md) Section 3:
  - Título: `PrivGvard`
  - Subtítulo
  - Descripción completa
  - Características principales
  - Palabras clave de búsqueda
3. Upload screenshots.
4. Click **Save**.

### 3.6 Submission Options & Certification Notes
1. Open the **Notes for certification** section.
2. Copy the entire contents of [docs/store/REVIEWER-CERTIFICATION-NOTES.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/REVIEWER-CERTIFICATION-NOTES.md).
3. In the **Restricted Capabilities Declaration** section:
   - Copy the justifications for `runFullTrust`, `allowElevation`, and `unvirtualizedResources` as detailed in [docs/store/MICROSOFT-STORE-JUSTIFICATION.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/MICROSOFT-STORE-JUSTIFICATION.md).
4. Click **Save**.

---

## Step 4: Submission Preflight Audit

Confirm all sections show a green checkmark or completed status:
- [ ] Pricing and availability: Saved
- [ ] Properties: Saved (Category & Privacy Policy URL verified)
- [ ] Age ratings: Completed (IARC Certificate generated)
- [ ] Packages: Validated (`2.0.0.0-x64`)
- [ ] Store listings: Completed (`en-us` and `es-es` with screenshots)
- [ ] Submission options: Reviewer notes and capability justifications pasted

---

## Step 5: Stop & Request User Authorization (Boundary for Stage C)

**DO NOT CLICK "Submit for certification".**  
Notify the user that the Partner Center submission draft is 100% prepared and ready for review.
When the user grants explicit authorization, proceed to Stage C.
