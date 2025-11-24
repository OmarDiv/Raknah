
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Raknah.Abstractions;
using Raknah.Authentications;
using Raknah.Consts.Errors;
using Raknah.Contracts.Authentication;
using Raknah.Extensions;
using Raknah.Persistence;
using Raknah.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Raknah.Services;

public class AuthService(UserManager<ApplicationUser> userManager,
    IJwtProvider jwtProvider,
    IEmailSendar emailSender
    ) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly int _refreshTokenExpires = 14;
    private readonly IEmailSendar _emailSender = emailSender;

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        //check email and password
        if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

        //generate token
        (string token, int expiresIn) = _jwtProvider.GenerateJwtTokenAsync(user);

        //generate refresh token
        user.RefreshTokens.ToList().ForEach(token => token.RevokedOn = DateTime.Now);

        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpires);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            Expires = refreshTokenExpiration,
        });

        await _userManager.UpdateAsync(user);

        return Result.Success( new AuthResponse(user.FullName, user.Email!, token, expiresIn, refreshToken, refreshTokenExpiration));
    }
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailIsExist = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);
        if (emailIsExist)
            return Result.Failure<AuthResponse>(UserError.EmailAlreadyExists);


        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return Result.Failure<AuthResponse>(UserError.UserCreationFailed);

        (string token, int expiresIn) = _jwtProvider.GenerateJwtTokenAsync(user);

        if (await _userManager.FindByEmailAsync(user.Email!) is not { } updateUser)
            return Result.Failure<AuthResponse>(UserError.UserNotFound);

        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpires);

        updateUser.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            Expires = refreshTokenExpiration
        });
        await _userManager.UpdateAsync(updateUser);
        return Result.Success(new AuthResponse(user.FullName, user.Email!, token, expiresIn, refreshToken, refreshTokenExpiration));

    }
    public async Task<AuthResponse?> GenerateRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellation)
    {
        var userId = _jwtProvider.ValidateToken(token);
        if (userId is null)
            return null;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return null;
        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);
        if (userRefreshToken is null)
            return null;
        (string newToken, int newExpiresIn) = _jwtProvider.GenerateJwtTokenAsync(user);


        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpires);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            Expires = refreshTokenExpiration
        });
        await _userManager.UpdateAsync(user);
        return new AuthResponse(user.FullName, user.Email!, newToken, newExpiresIn, newRefreshToken, refreshTokenExpiration);


    }
    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task<Result<string>> SendOtp(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return Result.Success("");

        var result = await GenerateCodeAndToken(user);


        await _emailSender.SendEmailAsync(email, "Your OTP Code", "OTP", new Dictionary<string, string>
        {
            { "{{otpCode}}", result.code! },
        });

        return Result.Success(result.token!);
    }
    public async Task<Result<string>> VerifyOtp(VerifyOtpDto request)
    {
        var data = Decode<OtpToken>(request.Token);

        if (await _userManager.FindByEmailAsync(data!.Email) is not { } user)
            return Result.Success("");

        var otp = user?.PasswordResetOtps.FirstOrDefault(o => o.Code == request.Code && !o.IsExpired);

        if (otp is null || otp.IsUsed || data.Expiration < DateTime.UtcNow || data.Code != request.Code)
            return Result.Failure<string>(UserError.InvalidOrExpiredOTP);

        otp.IsUsed = true;

        var result = await GenerateCodeAndToken(user!);

        return Result.Success(result.token!);

    }
    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var data = Decode<OtpToken>(request.Token);

        if (await _userManager.FindByEmailAsync(data!.Email) is not { } user)
            return Result.Success();

        var otp = user?.PasswordResetOtps.FirstOrDefault(o => o.Code == data.Code && !o.IsExpired);

        if (otp is null || otp.IsUsed || data.Expiration < DateTime.UtcNow || otp.Code != data.Code)
            return Result.Failure<string>(UserError.InvalidOrExpiredOTP);

        otp.IsUsed = true;
        await _userManager.UpdateAsync(user!);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user!);
        var result = await _userManager.ResetPasswordAsync(user!, token, request.NewPassword);

        return result.Succeeded ? Result.Success() : Result.Failure(UserError.InvalidCredentials);
    }


    private async Task<(string code, string token)> GenerateCodeAndToken(ApplicationUser user)
    {
        var code = new Random().Next(100000, 999999).ToString();
        var expiration = DateTime.UtcNow.AddMinutes(5);

        user.PasswordResetOtps.Add(new PasswordResetOtp()
        {
            Code = code,
            Expiration = expiration
        });

        await _userManager.UpdateAsync(user);

        var data = new OtpToken
        {
            UserId = user.Id,
            Email = user.Email!,
            Code = code,
            Expiration = expiration
        };

        var json = JsonSerializer.Serialize(data);
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        return (code, token);
    }

    public T? Decode<T>(string token)
    {
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        return JsonSerializer.Deserialize<T>(json);
    }
}
