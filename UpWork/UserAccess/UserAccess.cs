﻿using System;
using System.Collections.Generic;
using System.Linq;
using UpWork.Abstracts;
using UpWork.Exceptions;

namespace UpWork.UserAccess
{
    public static class UserAccess
    {
        public static User Login(Credentials credentials, IList<User> users)
        {
            if (users == null)
                throw new DatabaseException("User list is null");
            if (users.Count == 0)
                throw new DatabaseException("There is no user!");


            var user = users.SingleOrDefault(u => u.Username.Equals(credentials.Username));


            if (user == null)
                throw new LoginException(
                    $"There is no user associated this username -> {credentials.Username}");


            if (!credentials.Password.Equals(user.Password))
                throw new LoginException("Password is wrong!");

            return user;
        }

        public static void Register(User newUser, IList<User> users)
        {
            if (newUser == null)
                throw new ArgumentNullException(nameof(newUser));
            if (users == null)
                throw new DatabaseException("User list is null");

            users.Add(newUser);
        }
    }
}