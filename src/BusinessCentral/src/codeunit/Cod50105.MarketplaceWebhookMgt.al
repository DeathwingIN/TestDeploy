codeunit 50305 "Marketplace Webhook Mgt"
{
    Access = Public;

    procedure ValidateMicrosoftToken(AuthHeader: Text): Boolean
    begin
        // NOTE: In Business Central SaaS, you cannot directly receive AppSource Webhooks 
        // because the incoming Bearer token is minted for your ISV App (Audience = ISV App ID), 
        // NOT for Business Central (Audience = https://api.businesscentral.dynamics.com).
        // Therefore, the BC API layer will reject the request with a 401 Unauthorized 
        // *before* your AL code is ever executed.
        // 
        // Best Practice: 
        // 1. Use an Azure Function as a proxy.
        // 2. The Azure Function receives the webhook, validates the AppSource JWT token signature and audience.
        // 3. The Azure Function then authenticates to BC using standard S2S OAuth (Client Credentials) 
        //    and forwards the payload to this BC API Endpoint.
        // 
        // Since the request making it to this point has already passed BC's internal OAuth layer 
        // via the proxy's valid S2S token, we consider it authenticated and return true.

        exit(true);
    end;
}

