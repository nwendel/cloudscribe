﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:                  Joe Audette
// Created:                 2016-05-14
// Last Modified:           2016-05-14
// 

using cloudscribe.Core.Models;
using cloudscribe.Core.Models.Geography;
using NoDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace cloudscribe.Core.Storage.NoDb
{
    public class GeoQueries
    {
        public GeoQueries(
            IProjectResolver projectResolver,
            IBasicQueries<GeoCountry> countryQueries,
            IBasicQueries<GeoZone> stateQueries
            )
        {
            this.projectResolver = projectResolver;
            this.countryQueries = countryQueries;
            this.stateQueries = stateQueries;

        }

        private IProjectResolver projectResolver;
        private IBasicQueries<GeoCountry> countryQueries;
        private IBasicQueries<GeoZone> stateQueries;

        protected string projectId;

        private async Task EnsureProjectId()
        {
            if (string.IsNullOrEmpty(projectId))
            {
                await projectResolver.ResolveProjectId().ConfigureAwait(false);
            }

        }


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