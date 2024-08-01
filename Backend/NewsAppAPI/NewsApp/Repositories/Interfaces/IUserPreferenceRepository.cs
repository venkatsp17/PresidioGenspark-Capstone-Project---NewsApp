﻿using Microsoft.EntityFrameworkCore;
using NewsApp.Models;

namespace NewsApp.Repositories.Interfaces
{
    public interface IUserPreferenceRepository : IRepository<string, UserPreference, string>
    {
        Task<IEnumerable<UserPreference>> DeleteByUserID(string key);


        Task<IEnumerable<UserPreference>> DeleteByCategoryID(string key);
    }
}
