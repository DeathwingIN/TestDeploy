codeunit 50300 "Fulfillment API Mgt."
{
    Access = Public;

    var
        ISVSetup: Record "ISV Setup";
        OAuthAuthorityUrlTxt: Label 'https://login.microsoftonline.com/%1/oauth2/v2.0/token', Locked = true;
        FulfillmentResourceTxt: Label '20e940b3-4c77-4b0c-9a53-9e16a1b010a7/.default', Locked = true;
        ApiVersionTxt: Label 'api-version=2018-08-31', Locked = true;
        SetupRead: Boolean;

    local procedure GetSetup()
    begin
        if SetupRead then
            exit;
        ISVSetup.Get();
        ISVSetup.TestField("Client Id");
        ISVSetup.TestField("AAD Tenant Id");
        SetupRead := true;
    end;

    local procedure GetAccessToken(): Text
    var
        Client: HttpClient;
        RequestMessage: HttpRequestMessage;
        ResponseMessage: HttpResponseMessage;
        Headers: HttpHeaders;
        Content: HttpContent;
        ContentHeaders: HttpHeaders;
        ResponseText: Text;
        JObj: JsonObject;
        JToken: JsonToken;
        BodyTxt: Text;
    begin
        GetSetup();

        BodyTxt := 'grant_type=client_credentials';
        BodyTxt += '&client_id=' + ISVSetup."Client Id";
        BodyTxt += '&client_secret=' + ISVSetup.GetClientSecret();
        BodyTxt += '&scope=' + FulfillmentResourceTxt;

        Content.WriteFrom(BodyTxt);
        Content.GetHeaders(ContentHeaders);
        ContentHeaders.Remove('Content-Type');
        ContentHeaders.Add('Content-Type', 'application/x-www-form-urlencoded');

        RequestMessage.SetRequestUri(StrSubstNo(OAuthAuthorityUrlTxt, ISVSetup."AAD Tenant Id"));
        RequestMessage.Method('POST');
        RequestMessage.Content := Content;

        if not Client.Send(RequestMessage, ResponseMessage) then
            Error('Could not connect to Microsoft Identity platform.');

        if not ResponseMessage.IsSuccessStatusCode() then begin
            ResponseMessage.Content().ReadAs(ResponseText);
            Error('Failed to get access token: %1 \%2', ResponseMessage.HttpStatusCode(), ResponseText);
        end;

        ResponseMessage.Content().ReadAs(ResponseText);
        if JObj.ReadFrom(ResponseText) then
            if JObj.Get('access_token', JToken) then
                exit(JToken.AsValue().AsText());

        Error('Unexpected response from Microsoft Identity platform.');
    end;

    procedure ResolveToken(MarketplaceToken: Text; var SubscriptionId: Text; var PlanId: Text; var Quantity: Integer; var TenantId: Text)
    var
        Client: HttpClient;
        ResponseMessage: HttpResponseMessage;
        Headers: HttpHeaders;
        ResponseText: Text;
        JObj: JsonObject;
        JToken: JsonToken;
    begin
        GetSetup();

        Headers := Client.DefaultRequestHeaders();
        Headers.Add('Authorization', 'Bearer ' + GetAccessToken());
        Headers.Add('x-ms-marketplace-token', MarketplaceToken);

        if not Client.Get(ISVSetup."Fulfillment API Base URL" + '/saas/subscriptions/resolve?' + ApiVersionTxt, ResponseMessage) then
            Error('Resolve call failed.');

        ResponseMessage.Content().ReadAs(ResponseText);

        if not ResponseMessage.IsSuccessStatusCode() then
            Error('Fulfillment API returned %1: %2', ResponseMessage.HttpStatusCode(), ResponseText);

        if JObj.ReadFrom(ResponseText) then begin
            if JObj.Get('id', JToken) then SubscriptionId := JToken.AsValue().AsText();
            if JObj.Get('planId', JToken) then PlanId := JToken.AsValue().AsText();
            if JObj.Get('quantity', JToken) then Quantity := JToken.AsValue().AsInteger();

            // The customer tenant ID is usually in purchaser.tenantId object
            if JObj.Get('purchaser', JToken) then
                if JToken.AsObject().Get('tenantId', JToken) then
                    TenantId := JToken.AsValue().AsText();
        end;
    end;

    procedure AcknowledgeOperation(SubscriptionId: Text; OperationId: Text; PlanId: Text; Quantity: Integer; Success: Boolean)
    var
        Client: HttpClient;
        RequestMessage: HttpRequestMessage;
        ResponseMessage: HttpResponseMessage;
        Headers: HttpHeaders;
        Content: HttpContent;
        ContentHeaders: HttpHeaders;
        JObj: JsonObject;
        PayloadText: Text;
        ResponseText: Text;
    begin
        GetSetup();

        JObj.Add('planId', PlanId);
        JObj.Add('quantity', Quantity);
        if Success then
            JObj.Add('status', 'Success')
        else
            JObj.Add('status', 'Failure');

        JObj.WriteTo(PayloadText);
        Content.WriteFrom(PayloadText);
        Content.GetHeaders(ContentHeaders);
        ContentHeaders.Remove('Content-Type');
        ContentHeaders.Add('Content-Type', 'application/json');

        RequestMessage.SetRequestUri(ISVSetup."Fulfillment API Base URL" + '/saas/subscriptions/' + SubscriptionId + '/operations/' + OperationId + '?' + ApiVersionTxt);
        RequestMessage.Method('PATCH'); // or POST depending on the endpoint spec
        RequestMessage.Content := Content;

        RequestMessage.GetHeaders(Headers);
        Headers.Add('Authorization', 'Bearer ' + GetAccessToken());

        if not Client.Send(RequestMessage, ResponseMessage) then
            Error('Could not send Operation Acknowledge.');

        if not ResponseMessage.IsSuccessStatusCode() then begin
            ResponseMessage.Content().ReadAs(ResponseText);
            Error('Acknowledge failed %1: %2', ResponseMessage.HttpStatusCode(), ResponseText);
        end;
    end;
}
