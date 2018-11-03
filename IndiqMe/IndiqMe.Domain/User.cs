﻿using IndiqMe.Domain.Common;
using IndiqMe.Domain.Enums;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IndiqMe.Domain
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Linkedin { get; set; }
        public  string Phone{ get; set; }
        public Profile Profile { get;  set; } = Profile.User;
        public virtual Address Address { get; set; }

        public bool PasswordIsStrong()
        {
            Regex rgx = new Regex(@"(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])[A-Za-z0-9\d$@$!%_*_?&#.,-_:;]{8,}");
            if (string.IsNullOrEmpty(Password) || !rgx.IsMatch(Password)) return false;

            return true;
        }

        public User Cleanup()
        {
            this.Password = string.Empty;
            this.PasswordSalt = string.Empty;
            return this;
        }

        public void Change(string email, string name, string linkedin, string phone)
        {
            this.Email = email;
            this.Name = name;
            this.Linkedin = linkedin;
            this.Phone = phone;
        }

        public void ChangeAddress(Address address)
        {
            this.Address = address;
            this.Address.UserId = Id;
        }

        public void ChangePassword(string password)
        {
            this.Password = password;
        }
    }
}
