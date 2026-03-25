page 50304 "Webhook History Lo"
{

    PageType = List;
    ApplicationArea = All;
    UsageCategory = History;
    SourceTable = "Webhook Log";
    Caption = 'Webhook History Log';

    Editable = false;
    InsertAllowed = false;
    ModifyAllowed = false;
    DeleteAllowed = false;

    layout
    {
        area(Content)
        {
            repeater(Group)
            {
                field("Operation Id"; Rec."Operation Id") { ApplicationArea = All; }
                field(Action; Rec.Action) { ApplicationArea = All; }
                field("Subscription Id"; Rec."Subscription Id") { ApplicationArea = All; }
                field(Status; Rec.Status) { ApplicationArea = All; }
                field("Time Stamp"; Rec."Time Stamp") { ApplicationArea = All; }
            }
        }
    }
}
