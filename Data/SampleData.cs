using EventManagerWebRazorPage.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace EventManagerWebRazorPage.Data
{
    public static class SampleData
    {
        public static async Task Seed(IServiceProvider service)
        {
            using (var scope = service.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = new[] { "Organizer", "Participant" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            using(var scope = service.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var hasher = new PasswordHasher<User>();

                string organizerEmail = "organizer@gmail.com";
                User organizer = new User { Id = "5ad4b80c-4389-4f1e-b61c-8c8418408f39", Email = organizerEmail, NormalizedEmail = organizerEmail.ToUpper(), UserInternalId = "1001", UserName = "Organizer" };
                organizer.PasswordHash = hasher.HashPassword(organizer, "Abc123");

                if(await userManager.FindByEmailAsync(organizerEmail) == null)
                {
                    await userManager.CreateAsync(organizer);
                }
                if(!await userManager.IsInRoleAsync(organizer,"Organizer"))
                {
                    await userManager.AddToRoleAsync(organizer, "Organizer");
                }

                string userEmail = "user@gmail.com";
                User user = new User { Id = "7cca0811-e2ab-49fa-992b-3c622914e564", Email = userEmail, NormalizedEmail = userEmail.ToUpper(), UserInternalId = "4321", UserName = "User A", CreditScore = 70, PhoneNumber = "0123456789" };
                user.PasswordHash = hasher.HashPassword(user, "Abc123");

                if (await userManager.FindByEmailAsync(userEmail) == null)
                {
                    await userManager.CreateAsync(user);
                }
                if (!await userManager.IsInRoleAsync(user, "Participant"))
                {
                    await userManager.AddToRoleAsync(user, "Participant");
                }

                string user2Email = "c@gmail.com";
                User user2 = new User { Id = "abcd-efgh-ijkl-mnob-123445", Email = user2Email, NormalizedEmail = user2Email.ToUpper(), UserInternalId = "4329", UserName = "User C", CreditScore = 70, PhoneNumber = "0123456789" };
                user2.PasswordHash = hasher.HashPassword(user2, "Abc123");

                if (await userManager.FindByEmailAsync(user2Email) == null)
                {
                    await userManager.CreateAsync(user2);
                }
                if (!await userManager.IsInRoleAsync(user2, "Participant"))
                {
                    await userManager.AddToRoleAsync(user2, "Participant");
                }

                string user3Email = "d@gmail.com";
                User user3 = new User { Id = "dcef-1234-5656-2342-123445", Email = user3Email, NormalizedEmail = user3Email.ToUpper(), UserInternalId = "3425", UserName = "User D", CreditScore = 70, PhoneNumber = "0123456789" };
                user3.PasswordHash = hasher.HashPassword(user3, "Abc123");

                if (await userManager.FindByEmailAsync(user3Email) == null)
                {
                    await userManager.CreateAsync(user3);
                }
                if (!await userManager.IsInRoleAsync(user3, "Participant"))
                {
                    await userManager.AddToRoleAsync(user3, "Participant");
                }
            }
        }
    }
}
