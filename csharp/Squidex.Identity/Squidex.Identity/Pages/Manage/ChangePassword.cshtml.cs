// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Identity.Extensions;
using Squidex.Identity.Model;

namespace Squidex.Identity.Pages.Manage
{
    public class ChangePasswordModel : PageModelBase<ChangePasswordModel>
    {
        [BindProperty]
        public ChangePasswordModelInputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public UserEntity UserInfo { get; set; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            UserInfo = await GetUserAsync();

            await next();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!await UserManager.HasPasswordAsync(UserInfo))
            {
                return RedirectToPage("./SetPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var changePasswordResult = await UserManager.ChangePasswordAsync(UserInfo, Input.OldPassword, Input.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                ModelState.AddModelErrors(changePasswordResult);

                return Page();
            }

            await SignInManager.SignInAsync(UserInfo, false);

            StatusMessage = T["PasswordChanged"];

            return RedirectToPage();
        }
    }

    public sealed class ChangePasswordModelInputModel
    {
        [Required]
        public string OldPassword { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Compare(nameof(NewPassword), ErrorMessage = "PasswordsNotSame")]
        public string ConfirmPassword { get; set; }
    }
}