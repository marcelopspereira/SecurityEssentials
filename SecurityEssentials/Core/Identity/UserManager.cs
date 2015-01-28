﻿using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using SecurityEssentials.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;

namespace SecurityEssentials.Core.Identity
{

    public class MyUserManager : IUserManager, IDisposable
    {

        #region Declarations

        private readonly UserStore<User> UserStore;
        private readonly SEContext Context;
        private readonly string PasswordValidityRegex = @"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z0-9]).*$";
        private readonly string PasswordValidityMessage = "Your password must consist of 8 characters, digits or special characters and must contain at least 1 uppercase, 1 lowercase and 1 numeric value";

        #endregion

        #region Properties

		private static IAuthenticationManager AuthenticationManager
		{
			get
			{
				return HttpContext.Current.GetOwinContext().Authentication;
			}
		}

        #endregion

        #region Constructor

        public MyUserManager()
        {
            Context = new SEContext();
            UserStore = new UserStore<User>(Context);
        }

        #endregion

        #region Create

        public async Task<TMIdentityResult> CreateAsync(string userName, string password, string passwordConfirmation, string email)
        {
            var user = await UserStore.FindByNameAsync(userName);

            if (Regex.Matches(password, PasswordValidityRegex).Count == 0)
            {
                return new TMIdentityResult(PasswordValidityMessage);
            }

            if (user == null)
            {
                user = new User();
                user.UserName = userName;
                var securedPassword = new SecuredPassword(password);
                try
                {
                    user.Enabled = false;
                    user.PasswordHash = Convert.ToBase64String(securedPassword.Hash);
                    user.Salt = Convert.ToBase64String(securedPassword.Salt);
                    user.Email = email;
                    await UserStore.CreateAsync(user);
                }
                catch (Exception ex)
                {
                    return new TMIdentityResult(ex.Message);
                }

                return new TMIdentityResult();
            }

            return new TMIdentityResult("Username already registered");
        }

        #endregion

        #region IDisposable Implemented Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (this.Context != null)
                {
                    this.Context.Dispose();
                }
                if (this.UserStore != null)
                {
                    this.UserStore.Dispose();
                }
            }
        }

        #endregion

        #region Find

        public async Task<User> FindById(int userId)
        {
            return await UserStore.FindByIdAsync(userId).ConfigureAwait(false);
        }

        public async Task<User> FindAsync(string userName, string password)
        {
            return await UserStore.FindAsync(userName, password).ConfigureAwait(false);
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            return await UserStore.FindByEmailAsync(email).ConfigureAwait(false);
        }

        #endregion

        #region SignInOut

        public async Task SignInAsync(string userName, bool isPersistent)
        {
            var user = await UserStore.FindByNameAsync(userName);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserStore.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        public void SignOut()
        {
            AuthenticationManager.SignOut();
        }

        #endregion

        #region Change

        public async Task<TMIdentityResult> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            if (Regex.Matches(newPassword, PasswordValidityRegex).Count == 0)
            {
                return new TMIdentityResult(PasswordValidityMessage);
            }

            await UserStore.ChangePasswordAsync(userId, oldPassword, newPassword);
            return new TMIdentityResult();
        }

        public async Task<TMIdentityResult> ChangePasswordFromTokenAsync(int userId, string oldPassword, string newPassword)
        {
            if (Regex.Matches(newPassword, PasswordValidityRegex).Count == 0)
            {
                return new TMIdentityResult(PasswordValidityMessage);
            }
            return await UserStore.ChangePasswordFromTokenAsync(userId, oldPassword, newPassword);
        }

        public async Task<int> GeneratePasswordResetTokenAsync(int userId)
        {
            return await UserStore.GeneratePasswordResetTokenAsync(userId);
        }

        #endregion

    }
}