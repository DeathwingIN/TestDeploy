table 50300 "ISV Subscription"
{
    DataClassification = CustomerContent;
    Caption = 'ISV Subscription';

    fields
    {
        field(1; "Subscription Id"; Text[100])
        {
            DataClassification = SystemMetadata;
            Caption = 'Subscription Id';
            Description = 'Unique identifier for the SaaS subscription.';
        }
        field(2; "AAD Tenant Id"; Text[50])
        {
            DataClassification = SystemMetadata;
            Caption = 'AAD Tenant Id';
            Description = 'The Azure Active Directory Tenant ID of the customer.';
        }
        field(3; "Customer Name"; Text[100])
        {
            DataClassification = CustomerContent;
            Caption = 'Customer Name';
        }
        field(4; "Plan Id"; Text[50])
        {
            DataClassification = SystemMetadata;
            Caption = 'Plan Id';
        }
        field(5; Quantity; Integer)
        {
            DataClassification = SystemMetadata;
            Caption = 'Quantity';
        }
        field(6; Status; Enum "ISV Subscription Status")
        {
            DataClassification = SystemMetadata;
            Caption = 'Status';
        }
        field(8; Trial; Boolean)
        {
            DataClassification = SystemMetadata;
            Caption = 'Trial';
        }
    }

    keys
    {
        key(PK; "Subscription Id")
        {
            Clustered = true;
        }
        key(Tenant; "AAD Tenant Id")
        {
        }
    }
}
