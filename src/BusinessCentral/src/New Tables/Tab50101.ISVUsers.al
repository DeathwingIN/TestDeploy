table 50301 "ISV Users"
{
    DataClassification = EndUserIdentifiableInformation;
    Caption = 'ISV Users';

    fields
    {
        field(1; "Tenant Id"; Text[50])
        {
            DataClassification = SystemMetadata;
            Caption = 'Tenant Id';
        }
        field(2; "User Object Id"; Text[50])
        {
            DataClassification = EndUserPseudonymousIdentifiers;
            Caption = 'User Object Id';
            Description = 'Entra ID Object ID';
        }
        field(3; Email; Text[100])
        {
            DataClassification = EndUserIdentifiableInformation;
            Caption = 'Email';
        }
        field(4; Active; Boolean)
        {
            DataClassification = SystemMetadata;
            Caption = 'Active';
        }
        field(5; "Last Login"; DateTime)
        {
            DataClassification = SystemMetadata;
            Caption = 'Last Login';
        }
    }

    keys
    {
        key(PK; "Tenant Id", "User Object Id")
        {
            Clustered = true;
        }
    }
}
