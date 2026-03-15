codeunit 50302 "Licensing Mgt."
{
    Access = Public;

    procedure IsLicenseValid(TenantId: Text): Boolean
    var
        ISVSubscription: Record "ISV Subscription";
    begin
        ISVSubscription.SetRange("AAD Tenant Id", TenantId);
        ISVSubscription.SetRange(Status, ISVSubscription.Status::Subscribed);

        if ISVSubscription.FindFirst() then
            exit(true);

        exit(false);
    end;

    procedure GetAvailableQuantity(TenantId: Text): Integer
    var
        ISVSubscription: Record "ISV Subscription";
        Quantity: Integer;
    begin
        Quantity := 0;
        ISVSubscription.SetRange("AAD Tenant Id", TenantId);
        ISVSubscription.SetRange(Status, ISVSubscription.Status::Subscribed);

        if ISVSubscription.FindSet() then
            repeat
                Quantity += ISVSubscription.Quantity;
            until ISVSubscription.Next() = 0;

        exit(Quantity);
    end;
}
