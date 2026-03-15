codeunit 50301 "Webhook Process Mgt."
{
    TableNo = "Webhook Log";

    trigger OnRun()
    begin
        ProcessWebhook(Rec);
    end;

    local procedure ProcessWebhook(var WebhookLog: Record "Webhook Log")
    var
        ISVSubscription: Record "ISV Subscription";
        FulfillmentMgt: Codeunit "Fulfillment API Mgt.";
        Success: Boolean;
    begin
        if WebhookLog.Processed then
            exit;

        Success := true;

        if not ISVSubscription.Get(WebhookLog."Subscription Id") then begin
            // If it's a new subscription, insert it
            if WebhookLog.Action = 'Subscribe' then begin
                ISVSubscription.Init();
                ISVSubscription."Subscription Id" := WebhookLog."Subscription Id";
                ISVSubscription."Plan Id" := WebhookLog."Plan Id";
                ISVSubscription.Quantity := WebhookLog.Quantity;
                ISVSubscription.Status := ISVSubscription.Status::Subscribed;
                // Note: The rest of the fields like AAD Tenant ID might not be in the webhook payload directly.
                ISVSubscription.Insert();
            end else begin
                WebhookLog.Processed := true;
                WebhookLog.Modify();
                exit; // Skip processing modifications for non-existent subscriptions
            end;
        end else begin
            // Update existing subscription
            case WebhookLog.Action of
                'Subscribe':
                    begin
                        ISVSubscription.Status := ISVSubscription.Status::Subscribed;
                        ISVSubscription.Quantity := WebhookLog.Quantity;
                        ISVSubscription.Modify();
                    end;
                'ChangeQuantity':
                    begin
                        ISVSubscription.Quantity := WebhookLog.Quantity;
                        ISVSubscription.Modify();
                    end;
                'Suspend':
                    begin
                        ISVSubscription.Status := ISVSubscription.Status::Suspended;
                        ISVSubscription.Modify();
                    end;
                'Unsubscribe':
                    begin
                        ISVSubscription.Status := ISVSubscription.Status::Unsubscribed;
                        ISVSubscription.Modify();
                    end;
                'Reinstate':
                    begin
                        ISVSubscription.Status := ISVSubscription.Status::Subscribed;
                        ISVSubscription.Modify();
                    end;
            end;
        end;

        // Acknowledge operation to Microsoft
        FulfillmentMgt.AcknowledgeOperation(
            WebhookLog."Subscription Id",
            WebhookLog."Operation Id",
            WebhookLog."Plan Id",
            WebhookLog.Quantity,
            Success
        );

        WebhookLog.Processed := true;
        WebhookLog.Modify();
    end;
}
