# End-to-End Testing Instructions: Partner Licensing Middleware

This document provides step-by-step instructions for testing the end-to-end flow of the Microsoft AppSource SaaS webhook integration and Business Central licensing.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Simulation via Postman (Mock Testing)](#simulation-via-postman-mock-testing)
3. [Component Testing (Sandbox)](#component-testing-sandbox)
4. [Customer Tenant Testing](#customer-tenant-testing)

---

## Prerequisites
- Both Business Central Extensions (Partner environment and Customer Environment `MC3GoogleAddressValidation`) must be published to their respective instances.
- The **Azure Function** `WebhookProxy` must be running either locally (`func start`) or deployed to Azure.
- Postman must be installed for sending the simulated webhook payloads.

---

## Simulation via Postman (Mock Testing)

We have provided a Postman collection to simulate webhook calls sent by Microsoft AppSource.

1. **Import the Collection:**
   - In Postman, click **Import**.
   - Select the `AppSource_Webhook_Simulation.postman_collection.json` file located in this `Testing/` directory.

2. **Configure Variables:**
   - In Postman, select the `AppSource Webhook Simulation` collection.
   - Go to the **Variables** tab.
   - Set `AzureFunctionUrl` to your local running Azure Function (e.g., `http://localhost:7071`) or your deployed Azure Function URL.
   - Set `Mock_JWT_Token` to a valid mock Microsoft Entra ID JWT generated for your ISV tenant to bypass the `[Authorize]` local validation. (Note: For local testing, you may need to temporarily mock/disable the token issuer check if you cannot generate a real Microsoft-issued token).

3. **Run Iterations:**
   - Open the **Simulate Subscribe** request and press **Send**.
   - Check the Azure Function logs to confirm the payload was received, the token validated, and successfully forwarded to Business Central.
   - Also simulate the `Suspend`, `Unsubscribe`, and `ChangeQuantity` actions using their respective requests.

---

## Component Testing (Sandbox)

### 1. Verify Partner Business Central Setup
- Open Business Central (Partner Tenant).
- Open the **ISV Setup** page. Ensure `Marketplace Webhook API Url`, `Fulfillment API Base URL`, `AAD Tenant ID`, `Client ID`, and `Client Secret` are populated with valid sandbox application credentials.

### 2. Verify Webhook Reception
- After triggering a Postman webhook simulation, open the **Webhook Logs** list page in Business Central.
- Confirm that new entries correspond to your Postman actions (`Subscribe`, `ChangeQuantity`, etc.).
- Ensure that the Background Task/Codeunit has successfully read the `Webhook Log` and updated the corresponding **ISV Subscription** table.

### 3. Creating Mock Subscriptions
- You can manually insert a record into the **ISV Subscription** table for testing.
- Assign it the AAD Tenant ID corresponding to your customer sandbox and set the Status to `Subscribed`.

---

## Customer Tenant Testing

This test simulates the AppSource end-user experience when they install your application.

1. **Get the Customer Tenant ID:**
   - Go to the Azure Portal of the Customer Sandbox or extract the Entra Tenant ID.
   - Example: `88888888-9999-0000-1111-222222222222`.

2. **Test App Registration:**
   - In the Customer's Business Central sandbox, install the `MC3GoogleAddressValidation` app.
   - Ensure the app is correctly authenticating and invoking the Partner BC API (`License Check API`).

3. **Validate Valid Subscription:**
   - Go to the Partner BC instance, create an **ISV Subscription** record linked to the Customer Tenant ID. Set Status = `Subscribed`, Active = `Yes`.
   - In the Customer BC sandbox, trigger an action that requires a license (e.g., opening the Address Validation page). The action should proceed successfully.

4. **Validate Expired/Invalid Subscription:**
   - In the Partner BC instance, change the **ISV Subscription** record status to `Suspended` or `Unsubscribed`.
   - In the Customer BC sandbox, trigger the same action. The app should now show an error indicating the license has expired or is invalid.
