page 50305 "ISV Subscription API"
{
    PageType = API;
    Caption = 'ISV Subscription API';

    // This creates the URL path: api/microcloud/licensing/v1.0/subscriptions
    APIPublisher = 'microcloud';
    APIGroup = 'licensing';
    APIVersion = 'v1.0';
    EntityName = 'isvSubscription';
    EntitySetName = 'isvSubscriptions';

    SourceTable = "ISV Subscription";
    DelayedInsert = true;

    // IMPORTANT: This allows Azure to update the record using ONLY the Subscription Id!
    ODataKeyFields = "Subscription Id";

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                // We only need to expose the fields we want to update
                field(subscriptionId; Rec."Subscription Id") { }
                field(aadTenantId; Rec."AAD Tenant Id") { }
                field(customerName; Rec."Customer Name") { }
            }
        }
    }
}