page 50302 "Webhook Logs"
{
    PageType = List;
    ApplicationArea = All;
    UsageCategory = History;
    SourceTable = "Webhook Log";
    Caption = 'Webhook Logs';
    Editable = false;
    InsertAllowed = false;
    ModifyAllowed = false;
    DeleteAllowed = false;

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                field("Operation Id"; Rec."Operation Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Operation Id field.';
                }
                field("Activity Id"; Rec."Activity Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Activity Id field.';
                }
                field("Action"; Rec."Action")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Action field.';
                }
                field("Subscription Id"; Rec."Subscription Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Subscription Id field.';
                }
                field("Publisher Id"; Rec."Publisher Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Publisher Id field.';
                }
                field("Offer Id"; Rec."Offer Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Offer Id field.';
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
                field(Processed; Rec.Processed)
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Processed field.';
                }
                field("Time Stamp"; Rec."Time Stamp")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Time Stamp field.';
                }
                field("Received At"; Rec."Received At")
                {
                    ApplicationArea = All;
                    ToolTip = 'Specifies the value of the Received At field.';
                }
            }
        }
    }
}
