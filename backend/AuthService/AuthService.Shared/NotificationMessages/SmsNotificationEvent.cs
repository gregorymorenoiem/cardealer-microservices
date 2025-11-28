using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Shared.NotificationMessages;

public class SmsNotificationEvent : NotificationEvent
{
    public SmsNotificationEvent()
    {
        Type = "SMS";
    }
}
