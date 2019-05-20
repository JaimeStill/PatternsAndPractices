using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UploadDemo.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace UploadDemo.Identity.Mock
{
    public class MockProvider : IUserProvider
    {
        public AdUser CurrentUser { get; set; }
        public bool Initialized { get; set; }
        public Task Create(HttpContext context, IConfiguration config) => throw new NotImplementedException("Use Create(string samAccountName) for MockProvider");

        public async Task Create(string samAccountName)
        {
            CurrentUser = await GetAdUser(samAccountName);
            Initialized = true;
        }

        public async Task AddIdentity(HttpContext context)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, CurrentUser.SamAccountName),
                new Claim(ClaimTypes.Email, CurrentUser.UserPrincipalName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
            };

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                props
            );
        }

        public Task<AdUser> GetAdUser(IIdentity identity) => throw new NotImplementedException("Use GetAdUser(string samAccountName) for MockProvider");

        public Task<AdUser> GetAdUser(string samAccountName) =>
            Task.Run(() =>
                AdUsers.FirstOrDefault(x =>
                    x.SamAccountName.ToLower().Equals(samAccountName.ToLower())
                )
            );

        public Task<AdUser> GetAdUser(Guid guid) =>
            Task.Run(() =>
                AdUsers.FirstOrDefault(x => guid.Equals(x.Guid.Value))
            );

        public Task<List<AdUser>> GetDomainUsers() =>
            Task.Run(() =>
                AdUsers.ToList()
            );

        public Task<List<AdUser>> FindDomainUser(string search)
        {
            return Task.Run(() =>
            {
                search = search.ToLower();

                var users = AdUsers
                    .Where(
                        x => x.SamAccountName.ToLower().Contains(search) ||
                        x.UserPrincipalName.ToLower().Contains(search) ||
                        x.DisplayName.ToLower().Contains(search)
                    )
                    .OrderBy(x => x.Surname)
                    .ToList();

                return users;
            });
        }

        private static string baseDn = "CN=Users,DC=Mock,DC=Net";

        private static IQueryable<AdUser> AdUsers = new List<AdUser>()
        {
            new AdUser
            {
                DisplayName = "Graham, Leanne",
                DistinguishedName = $"CN=lgraham,{baseDn}",
                EmailAddress = "lgraham@mock.net",
                Enabled = true,
                GivenName = "Leanne",
                Guid = Guid.Parse("c40bcced-28cd-406e-84c0-2d1d446b9a63"),
                SamAccountName = "lgraham",
                Surname = "Graham",
                UserPrincipalName = "lgraham@mock.net",
                VoiceTelephoneNumber = "555.555.0001"
            },
            new AdUser
            {
                DisplayName = "Howell, Ervin",
                DistinguishedName = $"CN=ehowell,{baseDn}",
                EmailAddress = "ehowell@mock.net",
                Enabled = true,
                GivenName = "Ervin",
                Guid = Guid.Parse("f16f6b21-c2d9-4dcf-a8d2-96906ca49872"),
                SamAccountName = "ehowell",
                Surname = "Howell",
                UserPrincipalName = "ehowell@mock.net",
                VoiceTelephoneNumber = "555.555.0002"
            },
            new AdUser
            {
                DisplayName = "Bauch, Clementine",
                DistinguishedName = $"CN=cbauch,{baseDn}",
                EmailAddress = "cbauch@mock.net",
                Enabled = true,
                GivenName = "Clementine",
                Guid = Guid.Parse("f6eaba45-1b3e-494e-95e7-89894c6c7c5e"),
                SamAccountName="cbauch",
                Surname = "Bauch",
                UserPrincipalName = "cbauch@mock.net",
                VoiceTelephoneNumber = "555.555.0003"
            },
            new AdUser
            {
                DisplayName = "Lebsack, Patricia",
                DistinguishedName = $"CN=plebsack,{baseDn}",
                EmailAddress = "plebsack@mock.net",
                Enabled = true,
                GivenName = "Patricia",
                Guid = Guid.Parse("f6d6fe1d-ae07-4e33-9ef5-f377e79f3a8b"),
                SamAccountName = "plebsack",
                Surname = "Lebsack",
                UserPrincipalName = "plebsack@mock.net",
                VoiceTelephoneNumber = "555.555.0004"
            },
            new AdUser
            {
                DisplayName = "Dietrich, Chelsey",
                DistinguishedName = $"CN=cdietrich,{baseDn}",
                EmailAddress = "cdietrch@mock.net",
                Enabled = true,
                GivenName = "Chelsey",
                Guid = Guid.Parse("710f21e9-ef86-45f5-95cc-339e73cb8e8a"),
                SamAccountName = "cdietrich",
                Surname = "Dietrich",
                UserPrincipalName = "cdietrich@mock.net",
                VoiceTelephoneNumber = "555.555.0005"
            },
            new AdUser
            {
                DisplayName = "Schulist, Dennis",
                DistinguishedName = $"CN=dschulist,{baseDn}",
                EmailAddress = "dschulist@mock.net",
                Enabled = true,
                GivenName = "Dennis",
                Guid = Guid.Parse("854c25c1-d0dc-454a-9152-3db2c246f591"),
                SamAccountName = "dschulist",
                Surname = "Schulist",
                UserPrincipalName = "dschulist@mock.net",
                VoiceTelephoneNumber = "555.555.0006"
            },
            new AdUser
            {
                DisplayName = "Weissnat, Kurtis",
                DistinguishedName = $"CN=kweissnat,{baseDn}",
                EmailAddress = "kweissnat@mock.net",
                Enabled = true,
                GivenName = "Kurtis",
                Guid = Guid.Parse("0f50dc7d-7ff9-4f18-be91-add0665e8078"),
                SamAccountName = "kweissnat",
                Surname = "Weissnat",
                UserPrincipalName = "kweissnat@mock.net",
                VoiceTelephoneNumber = "555.555.0007"
            }
        }.AsQueryable();
    }
}