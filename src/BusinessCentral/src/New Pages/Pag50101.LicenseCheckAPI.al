page 50301 "License Check API"
{
    PageType = API;
    APIPublisher = 'microcloud360';
    APIGroup = 'licensing';
    APIVersion = 'v1.0';
    EntityName = 'licenseCheck';
    EntitySetName = 'licenseChecks';
    SourceTable = "ISV Subscription";
    InsertAllowed = false;
    ModifyAllowed = false;
    DeleteAllowed = false;
    ODataKeyFields = SystemId;

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
                field(subscriptionId; rec."Subscription Id")
                {
                    ApplicationArea = All;
                }
                field(aadTenantId; rec."AAD Tenant Id")
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
                field(planId; rec."Plan Id")
                {
                    ApplicationArea = All;
                }
                field(trial; rec.Trial)
                {
                    ApplicationArea = All;
                }
            }
        }
    }
}
