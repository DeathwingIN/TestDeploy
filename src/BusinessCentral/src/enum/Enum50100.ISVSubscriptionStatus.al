enum 50300 "ISV Subscription Status"
{
    Extensible = true;

    value(0; Subscribed)
    {
        Caption = 'Subscribed';
    }
    value(1; Suspended)
    {
        Caption = 'Suspended';
    }
    value(2; Unsubscribed)
    {
        Caption = 'Unsubscribed';
    }
    value(3; PendingFulfillmentStart)
    {
        Caption = 'PendingFulfillmentStart';
    }
}
