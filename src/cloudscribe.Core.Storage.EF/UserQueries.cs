﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-11-16
// Last Modified:			2016-05-18
// 


using cloudscribe.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace cloudscribe.Core.Storage.EF
{
    public class UserQueries : IUserQueries
    {
        public UserQueries(CoreDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private CoreDbContext dbContext;

        #region User
        
        //public int GetCount(Guid siteId)
        //{
        //    return dbContext.Users.Count<SiteUser>(x => x.SiteId == siteId);
        //}
        
        public async Task<ISiteUser> Fetch(
            Guid siteId,
            Guid userId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item
                = await dbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.SiteId == siteId && x.Id == userId
                    , cancellationToken)
                    .ConfigureAwait(false);

            return item;
        }


        public async Task<ISiteUser> Fetch(
            Guid siteId,
            string email,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            //string loweredEmail = email.ToLowerInvariant();
            var item = await dbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.SiteId == siteId && x.NormalizedEmail == email
                    , cancellationToken)
                    .ConfigureAwait(false);

            return item;
        }

        public async Task<ISiteUser> FetchByLoginName(
            Guid siteId,
            string userName,
            bool allowEmailFallback,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            //string loweredUserName = userName.ToLowerInvariant();

            var item = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(
                    x => x.SiteId == siteId
                    && (
                    (x.NormalizedUserName == userName)
                    || (allowEmailFallback && x.NormalizedEmail == userName)
                    ),
                    cancellationToken
                    )
                    .ConfigureAwait(false);

            return item;
        }

        public async Task<List<IUserInfo>> GetByIPAddress(
            Guid siteId,
            string ipv4Address,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from x in dbContext.Users
                        join y in dbContext.UserLocations
                        on x.SiteId equals y.SiteId
                        where x.Id == y.UserId && y.IpAddress == ipv4Address
                        select x
                        ;

            var items = await query
                .AsNoTracking()
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);

            return items;

        }

        public async Task<List<IUserInfo>> GetCrossSiteUserListByEmail(
            string email,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            //string loweredEmail = email.ToLowerInvariant();

            var query = from c in dbContext.Users
                        where c.NormalizedEmail == email
                        orderby c.DisplayName ascending
                        select c;

            var items = await query
                .AsNoTracking()
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);

            return items;

        }

        public async Task<int> CountUsers(
            Guid siteId,
            string userNameBeginsWith,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dbContext.Users.CountAsync<SiteUser>(
                x =>
                (
                    x.SiteId == siteId
                    && x.IsDeleted == false
                    && x.AccountApproved == true
                    && (
                    userNameBeginsWith == string.Empty
                    || x.DisplayName.StartsWith(userNameBeginsWith)
                    )
                )
                , cancellationToken
                )
                .ConfigureAwait(false);

        }

        public async Task<List<IUserInfo>> GetPage(
            Guid siteId,
            int pageNumber,
            int pageSize,
            string userNameBeginsWith,
            int sortMode,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            //sortMode: 0 = DisplayName asc, 1 = JoinDate desc, 2 = Last, First

            int offset = (pageSize * pageNumber) - pageSize;

            //IQueryable<IUserInfo> query 
            //    = from x in dbContext.Users

            //            where 
            //            (
            //                x.SiteId == siteId
            //                && x.IsDeleted == false
            //                && x.AccountApproved == true
            //                && (
            //                userNameBeginsWith == string.Empty
            //                || x.DisplayName.StartsWith(userNameBeginsWith)
            //                )
            //            )
            //           // orderby x.DisplayName
            //            select new UserInfo
            //            {
            //                AvatarUrl = x.AvatarUrl,
            //                AccountApproved = x.AccountApproved,
            //                Country = x.Country,
            //                CreatedUtc = x.CreatedUtc,
            //                DateOfBirth = x.DateOfBirth,
            //                DisplayInMemberList = x.DisplayInMemberList,
            //                DisplayName = x.DisplayName,
            //                Email = x.Email,
            //                FirstName = x.FirstName,
            //                Gender = x.Gender,
            //                IsDeleted = x.IsDeleted,
            //                IsLockedOut = x.IsLockedOut,
            //                LastActivityDate = x.LastActivityDate,
            //                LastLoginDate = x.LastLoginDate,
            //                LastName= x.LastName,
            //                PhoneNumber = x.PhoneNumber,
            //                PhoneNumberConfirmed = x.PhoneNumberConfirmed,
            //                SiteGuid = x.SiteGuid,
            //                SiteId = x.SiteId,
            //                State = x.State,
            //                TimeZoneId = x.TimeZoneId,
            //                Trusted = x.Trusted,
            //                UserGuid = x.UserGuid,
            //                UserId = x.UserId,
            //                UserName = x.UserName,
            //                WebSiteUrl = x.WebSiteUrl

            //            };


            //switch (sortMode)
            //{
            //    case 2:
            //        query = query.OrderBy(sl => sl.LastName).ThenBy(s2 => s2.FirstName).AsQueryable();
            //        break;
            //    case 1:
            //        query = query.OrderByDescending(sl => sl.CreatedUtc).AsQueryable();
            //        break;

            //    case 0:
            //    default:
            //        query = query.OrderBy(sl => sl.DisplayName).AsQueryable();
            //        break;
            //}

            //this is pretty ugly, surely there is a better way to dynamically set the order by

            IQueryable<IUserInfo> query;
            switch (sortMode)
            {
                case 2:
                    //query = query.OrderBy(sl => sl.LastName).ThenBy(s2 => s2.FirstName).AsQueryable();
                    query
                = from x in dbContext.Users

                  where
                  (
                      x.SiteId == siteId
                      && x.IsDeleted == false
                      && x.AccountApproved == true
                      && (
                      userNameBeginsWith == string.Empty
                      || x.DisplayName.StartsWith(userNameBeginsWith)
                      )
                  )
                  orderby x.LastName, x.FirstName
                  select new UserInfo
                  {
                      Id = x.Id,
                      AvatarUrl = x.AvatarUrl,
                      AccountApproved = x.AccountApproved,
                      Country = x.Country,
                      CreatedUtc = x.CreatedUtc,
                      DateOfBirth = x.DateOfBirth,
                      DisplayInMemberList = x.DisplayInMemberList,
                      DisplayName = x.DisplayName,
                      Email = x.Email,
                      FirstName = x.FirstName,
                      Gender = x.Gender,
                      IsDeleted = x.IsDeleted,
                      IsLockedOut = x.IsLockedOut,
                      LastLoginDate = x.LastLoginDate,
                      LastName = x.LastName,
                      PhoneNumber = x.PhoneNumber,
                      PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                      SiteId = x.SiteId,
                      State = x.State,
                      TimeZoneId = x.TimeZoneId,
                      Trusted = x.Trusted,
                      UserName = x.UserName,
                      WebSiteUrl = x.WebSiteUrl

                  };



                    break;
                case 1:
                    //query = query.OrderByDescending(sl => sl.CreatedUtc).AsQueryable();

                    query
                = from x in dbContext.Users

                  where
                  (
                      x.SiteId == siteId
                      && x.IsDeleted == false
                      && x.AccountApproved == true
                      && (
                      userNameBeginsWith == string.Empty
                      || x.DisplayName.StartsWith(userNameBeginsWith)
                      )
                  )
                  orderby x.CreatedUtc descending
                  select new UserInfo
                  {
                      Id = x.Id,
                      AvatarUrl = x.AvatarUrl,
                      AccountApproved = x.AccountApproved,
                      Country = x.Country,
                      CreatedUtc = x.CreatedUtc,
                      DateOfBirth = x.DateOfBirth,
                      DisplayInMemberList = x.DisplayInMemberList,
                      DisplayName = x.DisplayName,
                      Email = x.Email,
                      FirstName = x.FirstName,
                      Gender = x.Gender,
                      IsDeleted = x.IsDeleted,
                      IsLockedOut = x.IsLockedOut,
                      LastLoginDate = x.LastLoginDate,
                      LastName = x.LastName,
                      PhoneNumber = x.PhoneNumber,
                      PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                      SiteId = x.SiteId,
                      State = x.State,
                      TimeZoneId = x.TimeZoneId,
                      Trusted = x.Trusted,
                      UserName = x.UserName,
                      WebSiteUrl = x.WebSiteUrl

                  };


                    break;

                case 0:
                default:
                    //query = query.OrderBy(sl => sl.DisplayName).AsQueryable();

                    query
                = from x in dbContext.Users

                  where
                  (
                      x.SiteId == siteId
                      && x.IsDeleted == false
                      && x.AccountApproved == true
                      && (
                      userNameBeginsWith == string.Empty
                      || x.DisplayName.StartsWith(userNameBeginsWith)
                      )
                  )
                  orderby x.DisplayName
                  select new UserInfo
                  {
                      Id = x.Id,
                      AvatarUrl = x.AvatarUrl,
                      AccountApproved = x.AccountApproved,
                      Country = x.Country,
                      CreatedUtc = x.CreatedUtc,
                      DateOfBirth = x.DateOfBirth,
                      DisplayInMemberList = x.DisplayInMemberList,
                      DisplayName = x.DisplayName,
                      Email = x.Email,
                      FirstName = x.FirstName,
                      Gender = x.Gender,
                      IsDeleted = x.IsDeleted,
                      IsLockedOut = x.IsLockedOut,
                      LastLoginDate = x.LastLoginDate,
                      LastName = x.LastName,
                      PhoneNumber = x.PhoneNumber,
                      PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                      SiteId = x.SiteId,
                      State = x.State,
                      TimeZoneId = x.TimeZoneId,
                      Trusted = x.Trusted,
                      UserName = x.UserName,
                      WebSiteUrl = x.WebSiteUrl

                  };



                    break;
            }


            return await query
                .AsNoTracking()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);


        }

        public async Task<int> CountUsersForAdminSearch(
            Guid siteId,
            string searchInput,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dbContext.Users.CountAsync<SiteUser>(
                x =>
                (
                    x.SiteId == siteId
                    && (
                    searchInput == string.Empty
                    || x.Email.Contains(searchInput)
                    || x.UserName.Contains(searchInput)
                    || x.FirstName.Contains(searchInput)
                    || x.LastName.Contains(searchInput)
                    || x.DisplayName.Contains(searchInput)
                    )
                )
                , cancellationToken
                )
                .ConfigureAwait(false);
        }

        public async Task<List<IUserInfo>> GetUserAdminSearchPage(
            Guid siteId,
            int pageNumber,
            int pageSize,
            string searchInput,
            int sortMode,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            //sortMode: 0 = DisplayName asc, 1 = JoinDate desc, 2 = Last, First

            int offset = (pageSize * pageNumber) - pageSize;

            IQueryable<IUserInfo> query
                = from x in dbContext.Users

                  where
                  (
                      x.SiteId == siteId
                        && (
                        searchInput == string.Empty
                        || x.Email.Contains(searchInput)
                        || x.UserName.Contains(searchInput)
                        || x.FirstName.Contains(searchInput)
                        || x.LastName.Contains(searchInput)
                        || x.DisplayName.Contains(searchInput)
                        )
                  )
                  select new UserInfo
                  {
                      Id = x.Id,
                      AvatarUrl = x.AvatarUrl,
                      AccountApproved = x.AccountApproved,
                      Country = x.Country,
                      CreatedUtc = x.CreatedUtc,
                      DateOfBirth = x.DateOfBirth,
                      DisplayInMemberList = x.DisplayInMemberList,
                      DisplayName = x.DisplayName,
                      Email = x.Email,
                      FirstName = x.FirstName,
                      Gender = x.Gender,
                      IsDeleted = x.IsDeleted,
                      IsLockedOut = x.IsLockedOut,
                      LastLoginDate = x.LastLoginDate,
                      LastName = x.LastName,
                      PhoneNumber = x.PhoneNumber,
                      PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                      SiteId = x.SiteId,
                      State = x.State,
                      TimeZoneId = x.TimeZoneId,
                      Trusted = x.Trusted,
                      UserName = x.UserName,
                      WebSiteUrl = x.WebSiteUrl

                  };


            switch (sortMode)
            {
                case 2:
                    query = query.OrderBy(sl => sl.LastName).ThenBy(s2 => s2.FirstName).AsQueryable();
                    break;
                case 1:
                    query = query.OrderByDescending(sl => sl.CreatedUtc).AsQueryable();
                    break;

                case 0:
                default:
                    query = query.OrderBy(sl => sl.DisplayName).AsQueryable();
                    break;
            }

            return await query
                .AsNoTracking()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);


        }

        public async Task<int> CountLockedByAdmin(
            Guid siteId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dbContext.Users.CountAsync<SiteUser>(
                x => x.SiteId == siteId && x.IsLockedOut == true,
                cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<IUserInfo>> GetPageLockedByAdmin(
            Guid siteId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            int offset = (pageSize * pageNumber) - pageSize;

            IQueryable<IUserInfo> query
                = from x in dbContext.Users

                  where
                  (
                      x.SiteId == siteId
                      && x.IsLockedOut == true
                  )
                  orderby x.DisplayName
                  select new UserInfo
                  {
                      Id = x.Id,
                      AvatarUrl = x.AvatarUrl,
                      AccountApproved = x.AccountApproved,
                      Country = x.Country,
                      CreatedUtc = x.CreatedUtc,
                      DateOfBirth = x.DateOfBirth,
                      DisplayInMemberList = x.DisplayInMemberList,
                      DisplayName = x.DisplayName,
                      Email = x.Email,
                      FirstName = x.FirstName,
                      Gender = x.Gender,
                      IsDeleted = x.IsDeleted,
                      IsLockedOut = x.IsLockedOut,
                      LastLoginDate = x.LastLoginDate,
                      LastName = x.LastName,
                      PhoneNumber = x.PhoneNumber,
                      PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                      SiteId = x.SiteId,
                      State = x.State,
                      TimeZoneId = x.TimeZoneId,
                      Trusted = x.Trusted,
                      UserName = x.UserName,
                      WebSiteUrl = x.WebSiteUrl

                  };


            return await query
                .AsNoTracking()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);

        }

        public async Task<int> CountFutureLockoutEndDate(
            Guid siteId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dbContext.Users.CountAsync<SiteUser>(
                x => x.SiteId == siteId
                && x.LockoutEndDateUtc.HasValue
                && x.LockoutEndDateUtc.Value > DateTime.UtcNow
                ,
                cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<IUserInfo>> GetPageFutureLockoutEndDate(
            Guid siteId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            int offset = (pageSize * pageNumber) - pageSize;

            IQueryable<IUserInfo> query
                = from x in dbContext.Users

                  where
                  (
                      x.SiteId == siteId
                        && x.LockoutEndDateUtc.HasValue
                        && x.LockoutEndDateUtc.Value > DateTime.UtcNow
                  )
                  orderby x.DisplayName
                  select new UserInfo
                  {
                      Id = x.Id,
                      AvatarUrl = x.AvatarUrl,
                      AccountApproved = x.AccountApproved,
                      Country = x.Country,
                      CreatedUtc = x.CreatedUtc,
                      DateOfBirth = x.DateOfBirth,
                      DisplayInMemberList = x.DisplayInMemberList,
                      DisplayName = x.DisplayName,
                      Email = x.Email,
                      FirstName = x.FirstName,
                      Gender = x.Gender,
                      IsDeleted = x.IsDeleted,
                      IsLockedOut = x.IsLockedOut,
                      LastLoginDate = x.LastLoginDate,
                      LastName = x.LastName,
                      PhoneNumber = x.PhoneNumber,
                      PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                      SiteId = x.SiteId,
                      State = x.State,
                      TimeZoneId = x.TimeZoneId,
                      Trusted = x.Trusted,
                      UserName = x.UserName,
                      WebSiteUrl = x.WebSiteUrl

                  };


            return await query
                .AsNoTracking()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);

        }

        public async Task<int> CountUnconfirmedEmail(
            Guid siteId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dbContext.Users.CountAsync<SiteUser>(
                x => x.SiteId == siteId
                && x.EmailConfirmed == false
                ,
                cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<IUserInfo>> GetPageUnconfirmedEmailUsers(
            Guid siteId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            int offset = (pageSize * pageNumber) - pageSize;

            IQueryable<IUserInfo> query
                = from x in dbContext.Users

                  where
                  (
                      x.SiteId == siteId
                      && x.EmailConfirmed == false
                  )
                  orderby x.DisplayName
                  select new UserInfo
                  {
                      Id = x.Id,
                      AvatarUrl = x.AvatarUrl,
                      AccountApproved = x.AccountApproved,
                      Country = x.Country,
                      CreatedUtc = x.CreatedUtc,
                      DateOfBirth = x.DateOfBirth,
                      DisplayInMemberList = x.DisplayInMemberList,
                      DisplayName = x.DisplayName,
                      Email = x.Email,
                      FirstName = x.FirstName,
                      Gender = x.Gender,
                      IsDeleted = x.IsDeleted,
                      IsLockedOut = x.IsLockedOut,
                      LastLoginDate = x.LastLoginDate,
                      LastName = x.LastName,
                      PhoneNumber = x.PhoneNumber,
                      PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                      SiteId = x.SiteId,
                      State = x.State,
                      TimeZoneId = x.TimeZoneId,
                      Trusted = x.Trusted,
                      UserName = x.UserName,
                      WebSiteUrl = x.WebSiteUrl

                  };


            return await query
                .AsNoTracking()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);

        }

        public async Task<int> CountUnconfirmedPhone(
            Guid siteId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dbContext.Users.CountAsync<SiteUser>(
                x => x.SiteId == siteId
                && x.PhoneNumberConfirmed == false
                ,
                cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<IUserInfo>> GetPageUnconfirmedPhoneUsers(
            Guid siteId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            int offset = (pageSize * pageNumber) - pageSize;

            IQueryable<IUserInfo> query
                = from x in dbContext.Users

                  where
                  (
                      x.SiteId == siteId
                      && x.PhoneNumberConfirmed == false
                  )
                  orderby x.DisplayName
                  select new UserInfo
                  {
                      Id = x.Id,
                      AvatarUrl = x.AvatarUrl,
                      AccountApproved = x.AccountApproved,
                      Country = x.Country,
                      CreatedUtc = x.CreatedUtc,
                      DateOfBirth = x.DateOfBirth,
                      DisplayInMemberList = x.DisplayInMemberList,
                      DisplayName = x.DisplayName,
                      Email = x.Email,
                      FirstName = x.FirstName,
                      Gender = x.Gender,
                      IsDeleted = x.IsDeleted,
                      IsLockedOut = x.IsLockedOut,
                      LastLoginDate = x.LastLoginDate,
                      LastName = x.LastName,
                      PhoneNumber = x.PhoneNumber,
                      PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                      SiteId = x.SiteId,
                      State = x.State,
                      TimeZoneId = x.TimeZoneId,
                      Trusted = x.Trusted,
                      UserName = x.UserName,
                      WebSiteUrl = x.WebSiteUrl

                  };


            return await query
                .AsNoTracking()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);

        }

        public async Task<int> CountNotApprovedUsers(
            Guid siteId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dbContext.Users.CountAsync<SiteUser>(
                x => x.SiteId == siteId && x.AccountApproved == false,
                cancellationToken)
                .ConfigureAwait(false);

        }

        public async Task<List<IUserInfo>> GetNotApprovedUsers(
            Guid siteId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            int offset = (pageSize * pageNumber) - pageSize;

            IQueryable<IUserInfo> query
                = from x in dbContext.Users
                  where
                  (
                      x.SiteId == siteId
                      && x.AccountApproved == false
                  )
                  orderby x.DisplayName
                  select new UserInfo
                  {
                      Id = x.Id,
                      AvatarUrl = x.AvatarUrl,
                      AccountApproved = x.AccountApproved,
                      Country = x.Country,
                      CreatedUtc = x.CreatedUtc,
                      DateOfBirth = x.DateOfBirth,
                      DisplayInMemberList = x.DisplayInMemberList,
                      DisplayName = x.DisplayName,
                      Email = x.Email,
                      FirstName = x.FirstName,
                      Gender = x.Gender,
                      IsDeleted = x.IsDeleted,
                      IsLockedOut = x.IsLockedOut,
                      LastLoginDate = x.LastLoginDate,
                      LastName = x.LastName,
                      PhoneNumber = x.PhoneNumber,
                      PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                      SiteId = x.SiteId,
                      State = x.State,
                      TimeZoneId = x.TimeZoneId,
                      Trusted = x.Trusted,
                      UserName = x.UserName,
                      WebSiteUrl = x.WebSiteUrl

                  };


            return await query
                .AsNoTracking()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);

        }

        public async Task<bool> EmailExistsInDB(
            Guid siteId,
            string email,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var found = await Fetch(siteId, email);
            if (found == null) { return false; }
            return true;

        }

        public async Task<bool> EmailExistsInDB(
            Guid siteId,
            Guid userId,
            string email,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var found = await Fetch(siteId, email);
            if (found == null) { return false; }
            if (found.Id != userId) { return false; }
            return true;

        }

        public async Task<bool> LoginExistsInDB(
            Guid siteId, 
            string loginName,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            var item = await dbContext.Users.SingleOrDefaultAsync(
                    x => x.SiteId == siteId
                    && x.UserName == loginName
                    ).ConfigureAwait(false);

            if (item == null) { return false; }
            return true;
        }


        /// <summary>
        /// available only if the found user matches the passed in one
        /// or if not found
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="userId"></param>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public async Task<bool> LoginIsAvailable(
            Guid siteId,
            Guid userId,
            string loginName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var found = await FetchByLoginName(siteId, loginName, false, cancellationToken);
            if (found == null) { return true; }
            if (found.Id == userId) { return true; }
            return false;
        }


        public async Task<string> GetUserNameFromEmail(
            Guid siteId,
            string email,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var found = await Fetch(siteId, email, cancellationToken);
            if (found == null) { return string.Empty; }
            return found.UserName;
        }


        #endregion

        #region Roles

        
        public async Task<bool> RoleExists(
            Guid siteId,
            string roleName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            int count = await dbContext.Roles.CountAsync<SiteRole>(
                r => r.SiteId == siteId && r.NormalizedRoleName == roleName
                , cancellationToken)
                .ConfigureAwait(false);

            return count > 0;
        }

        public async Task<ISiteRole> FetchRole(
            Guid roleId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SiteRole item
                = await dbContext.Roles.SingleOrDefaultAsync(
                    x => x.Id == roleId
                    , cancellationToken)
                    .ConfigureAwait(false);

            return item;
        }

        public async Task<ISiteRole> FetchRole(
            Guid siteId,
            string roleName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SiteRole item
                = await dbContext.Roles.SingleOrDefaultAsync(
                    x => x.SiteId == siteId && x.RoleName == roleName
                    , cancellationToken)
                    .ConfigureAwait(false);

            return item;

        }

        public async Task<List<string>> GetUserRoles(
            Guid siteId,
            Guid userId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from x in dbContext.Roles
                        join y in dbContext.UserRoles
                        on x.Id equals y.RoleId
                        where y.UserId == userId
                        orderby x.RoleName
                        select x.RoleName
                        ;
            return await query
                .AsNoTracking()
                .ToListAsync<string>(cancellationToken)
                .ConfigureAwait(false);

        }

        public async Task<int> CountOfRoles(
            Guid siteId,
            string searchInput,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dbContext.Roles.CountAsync<SiteRole>(
                x => x.SiteId.Equals(siteId)
                && (
                 (searchInput == "")
                        || x.RoleName.Contains(searchInput)
                        || x.NormalizedRoleName.Contains(searchInput)
                ),
                cancellationToken
                ).ConfigureAwait(false);

        }

        public async Task<IList<ISiteRole>> GetRolesBySite(
            Guid siteId,
            string searchInput,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            int offset = (pageSize * pageNumber) - pageSize;

            var listQuery = from x in dbContext.Roles
                            where (
                            x.SiteId.Equals(siteId) &&
                            (searchInput == "" || x.RoleName.Contains(searchInput) || x.NormalizedRoleName.Contains(searchInput))
                            )
                            orderby x.NormalizedRoleName ascending
                            select new SiteRole
                            {
                                //RoleId = x.RoleId,
                                Id = x.Id,
                                // SiteId = x.SiteId,
                                SiteId = x.SiteId,
                                NormalizedRoleName = x.NormalizedRoleName,
                                RoleName = x.RoleName,
                                MemberCount = dbContext.UserRoles.Count<UserRole>(u => u.RoleId == x.Id)
                            };

            return await listQuery
                .AsNoTracking()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync<ISiteRole>(cancellationToken)
                .ConfigureAwait(false);

        }


        public async Task<int> CountUsersInRole(
            Guid siteId,
            Guid roleId,
            string searchInput,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from x in dbContext.Users
                        join y in dbContext.UserRoles
                        on x.Id equals y.UserId
                        where (
                            (x.SiteId.Equals(siteId) && y.RoleId.Equals(roleId))
                            && (
                                (searchInput == "")
                                || x.Email.Contains(searchInput)
                                || x.DisplayName.Contains(searchInput)
                                || x.UserName.Contains(searchInput)
                                || x.FirstName.Contains(searchInput)
                                || x.LastName.Contains(searchInput)
                            )
                            )
                        //select x.UserGuid
                        select 1
                        ;

            return await query.CountAsync<int>(cancellationToken).ConfigureAwait(false);

        }


        public async Task<IList<IUserInfo>> GetUsersInRole(
            Guid siteId,
            Guid roleId,
            string searchInput,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            int offset = (pageSize * pageNumber) - pageSize;

            var query = from x in dbContext.Users
                        join y in dbContext.UserRoles
                        on x.Id equals y.UserId
                        orderby x.DisplayName
                        where (
                            (x.SiteId.Equals(siteId) && y.RoleId.Equals(roleId))
                            && (
                                (searchInput == "")
                                || x.Email.Contains(searchInput)
                                || x.DisplayName.Contains(searchInput)
                                || x.UserName.Contains(searchInput)
                                || x.FirstName.Contains(searchInput)
                                || x.LastName.Contains(searchInput)
                            )
                            )
                        select x
                        ;


            return await query
                .AsNoTracking()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);



        }

        public async Task<IList<ISiteUser>> GetUsersInRole(
            Guid siteId,
            string roleName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from x in dbContext.Users
                        join y in dbContext.UserRoles
                        on x.Id equals y.UserId
                        join z in dbContext.Roles
                        on y.RoleId equals z.Id
                        orderby x.DisplayName
                        where
                            (x.SiteId.Equals(siteId) && z.NormalizedRoleName.Equals(roleName))

                        select x
                        ;

            var items = await query
                .AsNoTracking()
                .ToListAsync<ISiteUser>(cancellationToken)
                .ConfigureAwait(false);

            return items;
        }

        public async Task<int> CountUsersNotInRole(
            Guid siteId,
            Guid roleId,
            string searchInput,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from u in dbContext.Users
                        from r in dbContext.Roles
                        join ur in dbContext.UserRoles
                        on new { RoleId = r.Id, UserId = u.Id } equals new { ur.RoleId, ur.UserId } into t
                        from t2 in t.DefaultIfEmpty()
                        where (
                        u.SiteId == siteId
                        && r.SiteId == siteId
                        && r.Id == roleId
                        && (
                                (searchInput == "")
                                || u.Email.Contains(searchInput)
                                || u.DisplayName.Contains(searchInput)
                                || u.UserName.Contains(searchInput)
                                || u.FirstName.Contains(searchInput)
                                || u.LastName.Contains(searchInput)
                            )

                        && t2 == null
                        )

                        //select u.UserId;
                        select 1;

            return await query.CountAsync<int>(cancellationToken).ConfigureAwait(false);

        }

        public async Task<IList<IUserInfo>> GetUsersNotInRole(
            Guid siteId,
            Guid roleId,
            string searchInput,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            int offset = (pageSize * pageNumber) - pageSize;
            // it took me a lot of tries and googling to figure out how to get this query to work as intended
            // it works but is still logging a warning DefaultIfEmpty() could not be translated and will be evaluated locally

            var query = from u in dbContext.Users
                        from r in dbContext.Roles
                        join ur in dbContext.UserRoles
                        on new { RoleId = r.Id, UserId = u.Id } equals new { ur.RoleId, ur.UserId } into t
                        from t2 in t.DefaultIfEmpty()
                        where (
                        u.SiteId == siteId
                        && r.SiteId == siteId
                        && r.Id == roleId
                        && (
                                (searchInput == "")
                                || u.Email.Contains(searchInput)
                                || u.DisplayName.Contains(searchInput)
                                || u.UserName.Contains(searchInput)
                                || u.FirstName.Contains(searchInput)
                                || u.LastName.Contains(searchInput)
                            )

                        && t2 == null
                        )
                        orderby u.DisplayName
                        select u;

            return await query
                .AsNoTracking()
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync<IUserInfo>(cancellationToken)
                .ConfigureAwait(false);

        }


        #endregion

        #region Claims
        
        public async Task<IList<IUserClaim>> GetClaimsByUser(
            Guid siteId,
            Guid userId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from l in dbContext.UserClaims
                        where l.SiteId == siteId && l.UserId == userId

                        select l;
            var items = await query
                .AsNoTracking()
                .ToListAsync<IUserClaim>(cancellationToken)
                .ConfigureAwait(false);
            return items;

        }

        public async Task<IList<ISiteUser>> GetUsersForClaim(
            Guid siteId,
            string claimType,
            string claimValue,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from x in dbContext.Users
                        join y in dbContext.UserClaims
                        on x.Id equals y.UserId
                        where x.SiteId == siteId
                        orderby x.DisplayName
                        select x
                        ;

            var items = await query
                .AsNoTracking()
                .ToListAsync<ISiteUser>(cancellationToken)
                .ConfigureAwait(false);

            return items;
        }


        #endregion

        #region Logins
        
        public async Task<IUserLogin> FindLogin(
            Guid siteId,
            string loginProvider,
            string providerKey,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from l in dbContext.UserLogins
                        where (
                        l.SiteId == siteId
                        && l.LoginProvider == loginProvider
                        && l.ProviderKey == providerKey
                        )
                        select l;

            var items = await query
                .AsNoTracking()
                .SingleOrDefaultAsync<IUserLogin>(cancellationToken)
                .ConfigureAwait(false);

            return items;
        }
        
        public async Task<IList<IUserLogin>> GetLoginsByUser(
            Guid siteId,
            Guid userId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = from l in dbContext.UserLogins
                        where (
                        l.SiteId == siteId
                        && l.UserId == userId
                        )
                        select l;

            var items = await query
                .AsNoTracking()
                .ToListAsync<IUserLogin>(cancellationToken)
                .ConfigureAwait(false);

            return items;

        }

        #endregion

        #region UserLocation

        public async Task<IUserLocation> FetchLocationByUserAndIpv4Address(
            Guid userId,
            long ipv4AddressAsLong,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = from x in dbContext.UserLocations
                        where x.UserId == userId
                        && x.IpAddressLong == ipv4AddressAsLong
                        select x
                        ;

            return await query
                .AsNoTracking()
                .FirstOrDefaultAsync<UserLocation>(cancellationToken)
                .ConfigureAwait(false);

        }
        
        public Task<int> CountUserLocationsByUser(
            Guid userId,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            cancellationToken.ThrowIfCancellationRequested();

            return dbContext.UserLocations.CountAsync<UserLocation>(cancellationToken);


        }

        public async Task<IList<IUserLocation>> GetUserLocationsByUser(
            Guid userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            cancellationToken.ThrowIfCancellationRequested();

            int offset = (pageSize * pageNumber) - pageSize;

            var query = dbContext.UserLocations
                .OrderBy(x => x.IpAddressLong)
                .Select(p => p)
                .Skip(offset)
                .Take(pageSize)
                .Where(x => x.UserId == userId)
                ;


            return await query
                .AsNoTracking()
                .ToListAsync<IUserLocation>(cancellationToken)
                .ConfigureAwait(false);

        }

        #endregion

        #region IDisposable Support

        private void ThrowIfDisposed()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SiteRoleStore() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
