page 50303 "MC3 ISV Subscriptions"
{
    PageType = List;
    ApplicationArea = All;
    UsageCategory = Lists;
    SourceTable = "ISV Subscription";
    Caption = 'ISV Subscriptions';
    Editable = true;

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                field("Subscription Id"; Rec."Subscription Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Subscription Id field.';
                }
                field("AAD Tenant Id"; Rec."AAD Tenant Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the AAD Tenant Id field.';
                }
                field("Customer Name"; Rec."Customer Name")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Customer Name field.';
                }
                field("Plan Id"; Rec."Plan Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Plan Id field.';
                }
                field(Quantity; Rec.Quantity)
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Quantity field.';
                }
                field(Status; Rec.Status)
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Status field.';
                }

                field(Trial; Rec.Trial)
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Trial field.';
                }
            }
        }
    }
}
