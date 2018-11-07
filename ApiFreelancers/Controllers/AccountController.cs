﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Model;
using Service.Interface;

namespace ApiTokenJWT.Controllers
{
    [Produces("application/json")]
    [Route("accounts")]
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;
        private readonly DateTime _dateTime;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IAuthService authService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this._configuration = configuration;
            _authService = authService;
            _dateTime = DateTime.Now;
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserVm model)
        {
            if (ModelState.IsValid)
            {

                var user = new ApplicationUser {
                    UserName = model.Email,
                    Email = model.Email,
                    LastName = model.LastName,
                    Name = model.Name,
                    Role = 1,
                    Address = "No especificada",
                    CreatedAt = _dateTime
                };


                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (_authService.SendEmail(model))
                    {
                        return BuildToken(model);
                    }
                    else
                    {
                        return BadRequest("Email invalid");
                    }
                }
                else
                {
                    return BadRequest("Username or password invalid");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }

        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserVm userInfo)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(userInfo.Email, userInfo.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var model = _authService.GetByEmail(userInfo.Email);
                    userInfo.Name = model.Name;
                    userInfo.LastName = model.LastName;

                    if (_authService.ValidateUser(model.Email))
                    {
                        return BuildToken(userInfo);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Pleace confirm you email");
                        return BadRequest(ModelState);
                    }

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }


        [HttpGet("{id}")]
        [Route("email")]
        public IActionResult Email(string key)
        {
            //creamos una vista que al momento de aceptar el id lo envie por aqui
            return Ok(_authService.ValidateEmail(key));
        }


        //con este metodo creamos el token
        private IActionResult BuildToken(UserVm model)
        {
            var claims = new[]
            {
                //utilizamos los claims para enviar algunas informaciones
                new Claim(JwtRegisteredClaimNames.UniqueName, model.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
               //creanos una llave secreta y la configuramos como variable de entorno
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Secret_Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //le colocamos un tiempo de expiracion en este caso 1 hora
            var expiration = DateTime.UtcNow.AddHours(1);


            //preparamos el modelo
              model.Password = null;
              

            JwtSecurityToken token = new JwtSecurityToken(
               issuer: "yourdomain.com",
               audience: "yourdomain.com",
               claims: claims,
               expires: expiration,
               signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = expiration,
                User = model
            });

        }

    }
}
