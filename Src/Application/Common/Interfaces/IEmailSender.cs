using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces;
public interface IEmailSender
{
    Task SendAsync(string toEmail,string subject, string htmlBody,string? textBody = null,CancellationToken ct = default);
}
