page 50300 "Marketplace Webhook API"
{
    PageType = API;
    APIPublisher = 'microcloud360';
    APIGroup = 'licensing';
    APIVersion = 'v1.0';
    EntityName = 'marketplaceWebhook';
    EntitySetName = 'marketplaceWebhooks';
    SourceTable = "Webhook Log";
    InsertAllowed = true;
    ModifyAllowed = false;
    DeleteAllowed = false;
    ODataKeyFields = SystemId;
    DelayedInsert = true;

    layout
    {
        area(Content)
        {
            repeater(Group)
            {
                field(SystemId; Rec.SystemId)
                {
                    ApplicationArea = All;
                }
                field(id; rec."Operation Id")
                {
                    ApplicationArea = All;
                }
                field(activityId; rec."Activity Id")
                {
                    ApplicationArea = All;
                }
                field(action; rec.Action)
                {
                    ApplicationArea = All;
                }
                field(subscriptionId; rec."Subscription Id")
                {
                    ApplicationArea = All;
                }
                field(publisherId; rec."Publisher Id")
                {
                    ApplicationArea = All;
                }
                field(offerId; rec."Offer Id")
                {
                    ApplicationArea = All;
                }
                field(planId; rec."Plan Id")
                {
                    ApplicationArea = All;
                }
                field(quantity; rec.Quantity)
                {
                    ApplicationArea = All;
                }
                field(status; rec.Status)
                {
                    ApplicationArea = All;
                }
                field(timeStamp; rec."Time Stamp")
                {
                    ApplicationArea = All;
                }
            }
        }
    }

    trigger OnInsertRecord(BelowxRec: Boolean): Boolean
    var
        WebhookProcessMgt: Codeunit "Webhook Process Mgt.";
    begin
        //  save the Webhook Log record into the database first
        Rec.Insert(true);
        // process it to update the ISV Subscriptions table
        WebhookProcessMgt.ProcessWebhook(Rec);
        exit(false);

    end;
}
