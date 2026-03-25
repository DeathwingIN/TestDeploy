codeunit 50301 "Webhook Process Mgt."
{
    TableNo = "Webhook Log";

    trigger OnRun()
    begin
        ProcessWebhook(Rec);
    end;

    procedure ProcessWebhook(var WebhookLog: Record "Webhook Log")
    var
        ISVSubscription: Record "ISV Subscription";
        FulfillmentMgt: Codeunit "Fulfillment API Mgt.";
        Success: Boolean;
    begin
        if WebhookLog.Processed then
            exit;

        Success := true;

        // 2. Check if the subscription already exists in your table
        if not ISVSubscription.Get(WebhookLog."Subscription Id") then begin

            // If it's a new subscription, insert it
            if WebhookLog.Action = 'Subscribe' then begin
                ISVSubscription.Init();
                ISVSubscription."Subscription Id" := WebhookLog."Subscription Id";
                ISVSubscription."Plan Id" := WebhookLog."Plan Id";
                ISVSubscription.Quantity := WebhookLog.Quantity;

                // Set safely using the Enum directly (avoids the ALAL0122 error)
                ISVSubscription.Status := ISVSubscription.Status::Subscribed;

                ISVSubscription.Insert(true);
            end else begin
                // Skip processing modifications for non-existent subscriptions
                WebhookLog.Processed := true;
                WebhookLog.Modify(true);
                exit;
            end;

        end else begin

            // 3. Update existing subscription based on Microsoft's Action
            case WebhookLog.Action of
                'Subscribe':
                    begin
                        ISVSubscription.Status := ISVSubscription.Status::Subscribed;
                        ISVSubscription.Quantity := WebhookLog.Quantity;
                    end;
                'ChangeQuantity':
                    begin
                        ISVSubscription.Quantity := WebhookLog.Quantity;
                    end;
                'Suspend':
                    begin
                        ISVSubscription.Status := ISVSubscription.Status::Suspended;
                    end;
                'Unsubscribe':
                    begin
                        ISVSubscription.Status := ISVSubscription.Status::Unsubscribed;
                    end;
                'Reinstate':
                    begin
                        ISVSubscription.Status := ISVSubscription.Status::Subscribed;
                    end;
                else begin
                    // Fallback: If Action is empty but Microsoft sent a Status text
                    if WebhookLog.Status <> '' then
                        Evaluate(ISVSubscription.Status, WebhookLog.Status);
                end;
            end;

            ISVSubscription.Modify(true);
        end;

        ///TODO:Enable for prodcution - This is the call to Microsoft API to acknowledge the operation. You should implement this in your codeunit and call it here with the appropriate parameters. This is important to let Microsoft know that you have received and processed the webhook event successfully. If you don't acknowledge, Microsoft may retry sending the same event multiple times.
        // 4. Acknowledge operation to Microsoft so they know you received it
        // FulfillmentMgt.AcknowledgeOperation(
        //     WebhookLog."Subscription Id",
        //     WebhookLog."Operation Id",
        //     WebhookLog."Plan Id",
        //     WebhookLog.Quantity,
        //     Success
        // );

        // 5. Mark as fully processed so it doesn't run again
        WebhookLog.Processed := true;
        WebhookLog.Modify(true);
    end;
}