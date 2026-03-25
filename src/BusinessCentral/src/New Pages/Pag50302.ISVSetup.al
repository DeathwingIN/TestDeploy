page 50302 ISVSetup
{
    PageType = Card;
    ApplicationArea = All;
    UsageCategory = Administration;
    SourceTable = "ISV Setup";
    Caption = 'ISV Setup';

    // We only ever need one setup record, so we block inserting and deleting
    InsertAllowed = false;
    DeleteAllowed = false;

    layout
    {
        area(Content)
        {
            group(General)
            {
                Caption = 'Microsoft AppSource Configuration';

                field("AAD Tenant Id"; Rec."AAD Tenant Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'The Azure AD Tenant ID of your Partner environment.';
                }
                field("Client Id"; Rec."Client Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'The Application (Client) ID of your Azure App Registration.';
                }

                // We use a variable for the secret so it stays hidden and secure
                field(ClientSecretText; ClientSecretText)
                {
                    ApplicationArea = All;
                    Caption = 'Client Secret';
                    ExtendedDatatype = Masked;
                    ToolTip = 'The Client Secret of your Azure App Registration.';

                    trigger OnValidate()
                    begin
                        Rec.SetClientSecret(ClientSecretText);
                    end;
                }
                field("Fulfillment API Base URL"; Rec."Fulfillment API Base URL")
                {
                    ApplicationArea = All;
                }
            }
        }
    }

    var
        ClientSecretText: Text;

    // This makes sure a blank record is automatically created the very first time you open the page
    trigger OnOpenPage()
    begin
        if not Rec.Get() then begin
            Rec.Init();
            Rec.Insert(true);
        end;
    end;

    // This safely loads the secret from Isolated Storage when the page opens
    trigger OnAfterGetRecord()
    begin
        ClientSecretText := Rec.GetClientSecret();
        if ClientSecretText <> '' then
            ClientSecretText := '********'; // Mask the real password with stars
    end;
}
