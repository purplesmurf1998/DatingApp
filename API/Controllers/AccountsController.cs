using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AccountsController : BaseApiController
  {
    public readonly DataContext _context;
    private readonly ITokenService _tokenService;
    public AccountsController(DataContext context, ITokenService tokenService)
    {
      _context = context;
      _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
    {
      if (await UserExists(registerDto.Username)) return BadRequest("Account with this username already exists");

      using var hmac = new HMACSHA512();

      var user = new AppUser
      {
        UserName = registerDto.Username.ToLower(),
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
        PasswordSalt = hmac.Key
      };

      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      return new UserDTO
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
      };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
    {
      var user = await _context.Users.SingleOrDefaultAsync(user => user.UserName == loginDto.Username);

      if (user == null) return Unauthorized("Invalid username or password");

      using var hmac = new HMACSHA512(user.PasswordSalt);

      // compute the hash provided in the request
      var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
      // loop over every character and verify the password matches
      for (int i = 0; i < computedHash.Length; i++)
      {
        if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid username or password");
      }

      return new UserDTO
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
      }; ;
    }

    private async Task<bool> UserExists(string username)
    {
      return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
    }

  }
}