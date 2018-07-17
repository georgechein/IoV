using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class CustomAuth
    {
        public static ClaimsPrincipal GetFakeUser()
        {
            var userIdentity = new ClaimsIdentity("Identity");
            userIdentity.Label = "Identity";
            userIdentity.AddClaim(new Claim(ClaimTypes.Name, "Demo User"));
            userIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
            userIdentity.AddClaim(new Claim(ClaimTypes.Country, "TW"));
            userIdentity.AddClaim(new Claim(ClaimTypes.Email, "demouser@iov.tsti.local"));
            userIdentity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
            userIdentity.AddClaim(new Claim(ClaimTypes.Role, "User"));
            return new ClaimsPrincipal(userIdentity);
        }
        public static List<Permission> GetFullPermission()
        {
            var fullPermission = new List<Permission>();
            fullPermission.Add(Permission.ViewRules);
            fullPermission.Add(Permission.ViewDevices);
            fullPermission.Add(Permission.ViewDeviceSecurityKeys);
            fullPermission.Add(Permission.AddDevices);
            fullPermission.Add(Permission.EditRules);
            fullPermission.Add(Permission.RemoveDevices);
            fullPermission.Add(Permission.DeleteRules);
            fullPermission.Add(Permission.EditDeviceMetadata);
            fullPermission.Add(Permission.ViewDeviceSecurityKeys);
            fullPermission.Add(Permission.DisableEnableDevices);
            fullPermission.Add(Permission.SendCommandToDevices);
            fullPermission.Add(Permission.ViewActions);
            fullPermission.Add(Permission.ViewTelemetry);
            fullPermission.Add(Permission.AssignAction);
            return fullPermission;
        }
    }
}