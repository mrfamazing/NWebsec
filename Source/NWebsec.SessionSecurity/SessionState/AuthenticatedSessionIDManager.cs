﻿// Copyright (c) André N. Klingsheim. See License.txt in the project root for license information.

using System;
using System.Web;

namespace NWebsec.SessionSecurity.SessionState
{
    public class AuthenticatedSessionIDManager : System.Web.SessionState.SessionIDManager
    {
        private const string SessionIdContextKey = "NwebsecSessionIdCreated";
        internal const int SessionIdComponentLength = 16; //Length in bytes
        internal const int TruncatedMacLength = 16; //Length in bytes
        internal const int Base64SessionIdLength = 44; //Length in base64 characters
        
        private readonly HttpContextBase mockContext;
        private readonly IAuthenticatedSessionIDHelper sessionIdHelper;

        private HttpContextBase CurrentContext
        {
            get { return mockContext ?? new HttpContextWrapper(HttpContext.Current); }
        }

        public AuthenticatedSessionIDManager()
        {
            sessionIdHelper = AuthenticatedSessionIDHelper.Instance;
        }

        internal AuthenticatedSessionIDManager(HttpContextBase context, IAuthenticatedSessionIDHelper helper)
        {
            mockContext = context;
            sessionIdHelper = helper;
        }

        public override string CreateSessionID(HttpContext context)
        {
            var currentIdentity = CurrentContext.User.Identity;
            if (currentIdentity.IsAuthenticated && !String.IsNullOrEmpty(currentIdentity.Name))
            {
                var id = sessionIdHelper.Create(currentIdentity.Name);
                CurrentContext.Items[SessionIdContextKey] = id;
                return id;
            }
            return base.CreateSessionID(context);
        }

        public override bool Validate(string id)
        {
            var currentIdentity = CurrentContext.User.Identity;
            if (currentIdentity.IsAuthenticated && !String.IsNullOrEmpty(currentIdentity.Name))
            {
                return sessionIdHelper.Validate(currentIdentity.Name, id);
            }
            return base.Validate(id);
        }

        internal static bool AuthenticatedSessionCreated(HttpContextBase context)
        {
            var id = context.Items[SessionIdContextKey] as string;
            return  !String.IsNullOrEmpty(id) && context.Session != null && id.Equals(context.Session.SessionID);
        }
    }
}