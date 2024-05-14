﻿using Vigig.DAL.Interfaces;
using Vigig.Domain.Models;

namespace Vigig.DAL.Implementations;

public class VigigUserRepository : GenericRepository<VigigUser>, IVigigUserRepository 
{
    public VigigUserRepository(IAppDbContext dbContext) : base(dbContext)
    {
    }
}