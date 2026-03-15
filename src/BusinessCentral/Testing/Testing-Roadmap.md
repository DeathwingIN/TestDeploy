# Beginner-Friendly Testing Roadmap: Sandbox to Sandbox

This document outlines a clear, step-by-step roadmap for testing the AppSource SaaS licensing integration using your two Business Central sandbox environments, **before** publishing anything to AppSource.

---

## The Big Picture

You are acting as two different entities using two separate Business Central environments:

1. **The ISV (Microcloud 360)**: You will use your **Microcloud** Business Central sandbox. This is where you install the *Licensing App* (the one containing `ISV Subscription` tables, `Marketplace Webhook API`, and `License Check API`). Microsoft talks to this app to tell you who bought subscriptions. Let's call this the **Partner Tenant**.
2. **The Customer (Your MS Developer Account):** You will use your **MS Developer Account** Business Central sandbox. This person goes to your sandbox, installs your *Google Address Validation* app, and tries to use it. Let's call this the **Customer Tenant**.

The goal is to prove that when the Customer installs your Address Validation app in their sandbox, it connects across the internet back to the Partner Tenant to ask: *"Hey, does this customer actually have an active license?"*

---

## Phase 1: Setting up the Partner (Licensing) Tenant

**Goal:** Ensure your "Server" is ready to receive webhooks from Microsoft and answer license checks.

1. **Find the Microcloud BC Environment:**
   - Open VS Code to the project containing your Licensing App (the one we just built together).
   - Edit the `.vscode/launch.json` file to point to your **Microcloud** sandbox environment.
   
2. **Deploy the Licensing App:**
   - Press `F5` in VS Code to publish this extension to your **Microcloud** BC tenant.
   - This tenant is now your central licensing hub.

3. **Start the Azure Function Middleware (Local Testing):**
   - The Azure Function acts as the bouncer for your `MarketplaceWebhookAPI`. It checks Microsoft's ID badge before letting them talk to Business Central.
   - Open a terminal in the `WebhookProxy` folder and run `func start`.
   - Ensure `func start` shows no errors and the URL `http://localhost:7071/api/AppSourceWebhookProxy` is listening.
   - *(Note: Ensure `local.settings.json` has `SkipTokenValidation: true` for this local test, and the correct `BcWebhookEndpoint` pointing to the Microcloud tenant IDs).*

4. **Verify the Mock Webhook (Postman Simulation):**
   - We need to prove the Azure Function correctly pushes data into your Microcloud tenant.
   - Open the provided `AppSource_Webhook_Simulation.postman_collection.json` in Postman.
   - Ensure the Postman Variable `AzureFunctionUrl` is `http://localhost:7071`.
   - Run the **Simulate Subscribe** request.
   - *Checkpoint: Go to your Microcloud BC tenant. Search for the `Webhook Logs` page. You should see a new entry!*

---

## Phase 2: Setting up the Customer Tenant

**Goal:** Simulate a customer installing and trying to use your standalone product.

1. **Find the Customer's Tenant ID First:**
   - Log into the Azure Portal using your **MS Developer Account** credentials.
   - Go to **Microsoft Entra ID** -> **Overview**.
   - Copy the **Tenant ID**. You will need this to create their license later!

2. **Deploy the Address Validation App:**
   - Open a *different* VS Code window containing your existing **"Google Address Validation"** App project.
   - Edit the `.vscode/launch.json` file to point to your **MS Developer Account** sandbox environment.
   - *Crucial Step:* Ensure the code inside this app (specifically the `$MC3 License Check Mgt$` codeunit) has a URL hardcoded or configured to point to the **Microcloud Tenant's** `License Check API`.
      - *Example URL: `https://api.businesscentral.dynamics.com/v2.0/[MICROCLOUD_TENANT_ID]/Sandbox/api/microcloud360/licensing/v1.0/companies([MICROCLOUD_COMPANY_ID])/licenseCheck`*
   - Press `F5` in VS Code to publish the Address Validation app to the **MS Developer Account** BC tenant.

---

## Phase 3: The End-to-End Test (The Moment of Truth)

**Goal:** Prove that the Customer Tenant cannot use the feature without a valid subscription residing in the Partner Tenant.

### Test A: The Unlicensed Customer (Should Fail)
1. In the **Customer Tenant** (MS Developer Sandbox), open Business Central.
2. Try to use the Google Address Validation feature (e.g., validate an address on a Customer Card).
3. The app will secretly make an HTTP call to the Partner Tenant's `License Check API`, sending its Entra ID Tenant ID.
4. **Expected Result:** Because we haven't given this customer a subscription yet, the Partner Tenant will return `false`. The Customer Tenant should pop up an error saying: *"You do not have a valid license."*

### Test B: The Purchasing Customer (Should Succeed)
1. Switch hats back to the ISV. Go to the **Partner Tenant** (Microcloud Sandbox).
2. Search for and open the **ISV Subscriptions** page.
3. Manually create a new record:
   - **Subscription ID:** Any random GUID (e.g., `11111111-2222-3333-4444-555555555555`)
   - **AAD Tenant ID:** Paste the **Customer's Tenant ID** you copied in Phase 2, Step 1.
   - **Status:** set to `Subscribed`.
   - **Active:** set to `true` (or checked).
4. Now, switch hats back to the Customer. Go to the **Customer Tenant** (MS Developer Sandbox).
5. Try to validate an address again.
6. The app will call the Partner Tenant. The Partner Tenant will find the record you just manually created!
7. **Expected Result:** The address validation succeeds!

### Test C: The Cancelling Customer (Should Fail)
1. Go back to the **Partner Tenant** (Microcloud Sandbox).
2. Find the ISV Subscription record you just made and change the **Status** to `Unsubscribed` (or set `Active` to `false`).
3. Go back to the **Customer Tenant** (MS Developer Sandbox) and validate an address.
4. **Expected Result:** The validation should fail again with a license error.

---

## Next Steps Before AppSource Submission

Once you have completed Phase 3 and verified the logic works perfectly sandbox-to-sandbox, you are ready for AppSource!

When you publish to AppSource:
- You will host your Azure Function on Azure permanently.
- You will give Microsoft the production URL to your Azure Function.
- Microsoft will hit that URL automatically when *real* customers buy your app on the marketplace.
- The Azure Function will push that data into your Production Microcloud Tenant.
- The real Customer App will call your Production Microcloud Tenant, find the real subscription, and allow them to use the app!
