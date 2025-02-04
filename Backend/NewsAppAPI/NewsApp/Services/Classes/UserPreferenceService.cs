﻿using NewsApp.DTOs;
using NewsApp.Exceptions;
using NewsApp.Models;
using NewsApp.Repositories.Interfaces;
using NewsApp.Services.Interfaces;

namespace NewsApp.Services.Classes
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly IUserPreferenceRepository _userPreferenceRepository;
        public UserPreferenceService(IUserPreferenceRepository userPreferenceRepository) { 
            _userPreferenceRepository = userPreferenceRepository;
        }
        public async Task<IEnumerable<UserPreferenceReturnDTO>> AddPreferences(UserPreferenceDTO userPreferenceDTO)
        {
            IList<UserPreference> userPreferences = new List<UserPreference>();
            var deletedpreferences = await _userPreferenceRepository.DeleteByUserID(userPreferenceDTO.UserID.ToString());
            foreach (var item in userPreferenceDTO.preferences)
            {
 
                var newUserPreference = new UserPreference()
                {
                    UserID = userPreferenceDTO.UserID,
                    CategoryID = item.Key,
                    preference = item.Value
                };

                var userPreference = await _userPreferenceRepository.Add(newUserPreference);

                if (userPreference == null || userPreference.UserPreferenceID == 0)
                {
                    throw new UnableToAddItemException();
                }
                userPreferences.Add(userPreference);
               
            }
            return userPreferences.Select(x=> new UserPreferenceReturnDTO() { ID=x.UserPreferenceID, CategoryID=x.CategoryID, preference=x.preference, UserID=x.UserID });
        }

        public async Task<IEnumerable<UserPreferenceReturnDTO>> GetUserPreferences(int userid)
        {
            var preferences = await _userPreferenceRepository.GetAll("UserID",userid.ToString());

            if (!preferences.Any())
            {
                throw new NoAvailableItemException();
            }

            return preferences.Select(x => new UserPreferenceReturnDTO() { ID = x.UserPreferenceID, CategoryID = x.CategoryID, preference = x.preference, UserID = x.UserID });
        }
    }
}
