table 50303 "ISV Setup"
{
    DataClassification = CustomerContent;
    Caption = 'ISV Setup';

    fields
    {
        field(1; "Primary Key"; Code[10])
        {
            DataClassification = SystemMetadata;
            Caption = 'Primary Key';
        }
        field(2; "Client Id"; Text[250])
        {
            DataClassification = EndUserPseudonymousIdentifiers;
            Caption = 'Client Id';
        }
        field(3; "Client Secret Key"; Guid)
        {
            DataClassification = SystemMetadata;
            Caption = 'Client Secret Key';
            Description = 'Reference to Isolated Storage';
        }
        field(4; "Fulfillment API Base URL"; Text[250])
        {
            DataClassification = SystemMetadata;
            Caption = 'Fulfillment API Base URL';
            InitValue = 'https://marketplaceapi.microsoft.com/api';
        }
        field(5; "AAD Tenant Id"; Text[50])
        {
            DataClassification = SystemMetadata;
            Caption = 'AAD Tenant Id';
            Description = 'The ISVs Azure AD Tenant ID';
        }
    }

    keys
    {
        key(PK; "Primary Key")
        {
            Clustered = true;
        }
    }

    procedure SetClientSecret(NewSecret: Text)
    var
        IsolatedStorageManagement: Codeunit "Isolated Storage Management";
    begin
        if IsNullGuid("Client Secret Key") then begin
            "Client Secret Key" := CreateGuid();
            Modify();
        end;
        IsolatedStorage.Set("Client Secret Key", NewSecret, DataScope::Module);
    end;

    procedure GetClientSecret(): Text
    var
        SecretValue: Text;
    begin
        if IsNullGuid("Client Secret Key") then
            exit('');
        if IsolatedStorage.Get("Client Secret Key", DataScope::Module, SecretValue) then
            exit(SecretValue);
        exit('');
    end;
}
