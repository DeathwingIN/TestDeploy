table 50302 "Webhook Log"
{
    DataClassification = SystemMetadata;
    Caption = 'Webhook Log';

    fields
    {
        field(1; "Operation Id"; Text[100])
        {
            DataClassification = SystemMetadata;
            Caption = 'Operation Id';
            Description = 'Maps to id in the webhook payload';
        }
        field(2; "Action"; Text[50])
        {
            DataClassification = SystemMetadata;
            Caption = 'Action';
            Description = 'ChangeQuantity, Suspend, Unsubscribe, etc.';
        }
        field(3; "Subscription Id"; Text[100])
        {
            DataClassification = SystemMetadata;
            Caption = 'Subscription Id';
        }
        field(4; "Publisher Id"; Text[100])
        {
            DataClassification = SystemMetadata;
            Caption = 'Publisher Id';
        }
        field(5; "Offer Id"; Text[100])
        {
            DataClassification = SystemMetadata;
            Caption = 'Offer Id';
        }
        field(6; "Plan Id"; Text[100])
        {
            DataClassification = SystemMetadata;
            Caption = 'Plan Id';
        }
        field(7; Quantity; Integer)
        {
            DataClassification = SystemMetadata;
            Caption = 'Quantity';
        }
        field(8; Status; Text[50])
        {
            DataClassification = SystemMetadata;
            Caption = 'Status';
        }
        field(9; Processed; Boolean)
        {
            DataClassification = SystemMetadata;
            Caption = 'Processed';
        }
        field(10; "Time Stamp"; DateTime)
        {
            DataClassification = SystemMetadata;
            Caption = 'Time Stamp';
        }
        field(11; "Received At"; DateTime)
        {
            DataClassification = SystemMetadata;
            Caption = 'Received At';
        }
        field(12; "Activity Id"; Text[100])
        {
            DataClassification = SystemMetadata;
            Caption = 'Activity Id';
            Description = 'Maps to activityId in the webhook payload';
        }
    }

    keys
    {
        key(PK; "Operation Id")
        {
            Clustered = true;
        }
    }

    trigger OnInsert()
    begin
        "Received At" := CurrentDateTime();
    end;
}
