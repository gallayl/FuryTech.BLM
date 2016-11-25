﻿using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using BLM.Authorization;

namespace BLM.EF6
{
    public class EfContextInfo : IContextInfo
    {

        private readonly DbContext _dbcontext;

        public EfContextInfo(IIdentity identity, DbContext ctx)
        {
            _dbcontext = ctx;
            Identity = identity;
        }

        public IIdentity Identity { get; }
        public IQueryable<T> GetFullEntitySet<T>() where T : class
        {
            return _dbcontext.Set<T>();
        }

        public IQueryable<T> GetAuthorizedEntitySet<T>() where T: class
        {
            return AuthorizerManager.GetAuthorizer<T>().AuthorizeCollection(_dbcontext.Set<T>(), new EfContextInfo(Identity, _dbcontext));
        }
    }
}
