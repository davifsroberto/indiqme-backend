﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using IndiqMe.Api.Filters;
using IndiqMe.Api.ViewModels;
using IndiqMe.Domain;
using IndiqMe.Domain.Common;
using IndiqMe.Repository.Infra.CrossCutting.Identity;
using IndiqMe.Repository.Infra.CrossCutting.Identity.Configurations;
using IndiqMe.Repository.Infra.CrossCutting.Identity.Interfaces;
using IndiqMe.Service;
using System;
using System.Threading;

namespace IndiqMe.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    [GetClaimsFilter]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IApplicationSignInManager _signManager;

        public AccountController(IUserService userService, IApplicationSignInManager signManager)
        {
            _userService = userService;
            _signManager = signManager;
        }

        #region GET
        [Authorize("Bearer")]
        [HttpGet]
        public UserVM Get()
        {
            var id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var user = _userService.Find(id);

            var userVM = Mapper.Map<User, UserVM>(user);
            return userVM;
        }


        [Authorize("Bearer")]
        [HttpGet("Profile")]
        public object Profile()
        {
            var id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return new { profile = _userService.Find(id).Profile.ToString() };
        }
        #endregion


        #region POST
        [HttpPost("Register")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(409)]
        public IActionResult Post([FromBody]RegisterUserVM registerUserVM,
            [FromServices]SigningConfigurations signingConfigurations,
            [FromServices]TokenConfigurations tokenConfigurations)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = Mapper.Map<RegisterUserVM, User>(registerUserVM);

            var result = _userService.Insert(user);

            if (result.Success)
                return Ok(_signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations));

            return Conflict(result);
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public IActionResult Login([FromBody]LoginUserVM loginUserVM,
            [FromServices]SigningConfigurations signingConfigurations,
            [FromServices]TokenConfigurations tokenConfigurations)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = Mapper.Map<LoginUserVM, User>(loginUserVM);

            var result = _userService.AuthenticationByEmailAndPassword(user);

            if (result.Success)
            {
                var response = new Result
                {
                    Value = _signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations)
                };

                return Ok(response);
            }

            return NotFound(result);
        }
        #endregion

        #region PUT
        [Authorize("Bearer")]
        [HttpPut]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(409)]
        public IActionResult Update([FromBody]UpdateUserVM updateUserVM,
           [FromServices]SigningConfigurations signingConfigurations,
           [FromServices]TokenConfigurations tokenConfigurations)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = Mapper.Map<UpdateUserVM, User>(updateUserVM);

            user.Id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            var result = _userService.Update(user);

            if (result.Success)
                return Ok(_signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations));

            return Conflict(result);
        }


        [Authorize("Bearer")]
        [HttpPut("ChangePassword")]
        public Result<User> ChangePassword([FromBody]ChangePasswordUserVM changePasswordUserVM)
        {
            var user = new User() { Password = changePasswordUserVM.OldPassword };

            return _userService.ChangeUserPassword(user, changePasswordUserVM.NewPassword);
        }
        #endregion


    }
}
