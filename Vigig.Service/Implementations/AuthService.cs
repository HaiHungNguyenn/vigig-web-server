﻿using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Vigig.Common.Constants.Validations;
using Vigig.Common.Exceptions;
using Vigig.Common.Helpers;
using Vigig.Common.Settings;
using Vigig.DAL.Interfaces;
using Vigig.Domain.Models;
using Vigig.Service.Enums;
using Vigig.Service.Exceptions;
using Vigig.Service.Exceptions.NotFound;
using Vigig.Service.Interfaces;
using Vigig.Service.Models;
using Vigig.Service.Models.Request.Authentication;
using Vigig.Service.Models.Response.Authentication;

namespace Vigig.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly ICustomerTokenRepository _customerTokenRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IJwtService _jwtService;
    private readonly JwtSetting _jwtSetting;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AuthService(ICustomerRepository customerRepository, IMapper mapper, IUnitOfWork unitOfWork, IBuildingRepository buildingRepository, IJwtService jwtService, IConfiguration configuration, ICustomerTokenRepository customerTokenRepository)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _buildingRepository = buildingRepository;
        _jwtService = jwtService;
        _customerTokenRepository = customerTokenRepository;
        _jwtSetting = configuration.GetSection(nameof(JwtSetting)).Get<JwtSetting>() ?? throw new MissingJwtSettingsException();
    }
    public async Task<ServiceActionResult> RegisterAsync(RegisterRequest request)
    {
        var retrivedUser = await _customerRepository.GetAsync(user => user.Email!.ToLower() == request.Email.ToLower());
        // if (retrivedUser is { EmailConfirmed: true })
        // {
        //     throw new UserAlreadyExistException(request.Email);
        // }
        if (retrivedUser is not null)
            throw new UserAlreadyExistException(request.Email);

        if (!Regex.IsMatch(request.Email, UserProfileValidation.Email.EmailPattern))
            throw new EmailNotMatchedException();
        
        if (!Regex.IsMatch(request.Password, UserProfileValidation.Password.PasswordPattern))
            throw new PasswordTooWeakException();
        
        
        if (retrivedUser is null)
        {
            if (request.Role == UserRole.Client)
            {
                retrivedUser = _mapper.Map<Customer>(request);
                var hashedPassword = PasswordHashHelper.HashPassword(request.Password);
                retrivedUser.Password = hashedPassword;
                retrivedUser.CreatedDate = DateTime.Now;
                retrivedUser.NormalizedEmail = request.Email.ToUpper();
                retrivedUser.UserName = request.Email.Split("@")[0];
                retrivedUser.NormalizedUserName = retrivedUser.UserName.Split("@")[0].ToUpper();
                retrivedUser.Building =  (await _buildingRepository.FindAsync(b => b.Id == new Guid("50b84998-328c-4d80-97a6-445399d18f63"))).FirstOrDefault() 
                                         ?? throw new BuildingNotFoundException("50b84998-328c-4d80-97a6-445399d18f63");
                await _customerRepository.AddAsync(retrivedUser);
            }

            if (request.Role == UserRole.Provider)
            {
                throw new NotImplementedException();
            }
        }

        await _unitOfWork.CommitAsync();
        
        
        return new ServiceActionResult(true) { Data = _mapper.Map<RegisterResponse>(retrivedUser)};

    }

    public async Task<ServiceActionResult> LoginAsync(LoginRequest request)
    {
        if (request.Role == UserRole.Client)
        {

            var retrivedUser = await _customerRepository.GetAsync(c => c.Email.Equals(request.Email));
            if (retrivedUser is null)
                throw new CustomerNotFoundException(request.Email);
            var isValidPassword = PasswordHashHelper.VerifyPassword(request.Password, retrivedUser.Password);
            if (!isValidPassword)
                throw new InvalidPasswordException();
            var authResponse = await GenerateAuthResponseAsync(retrivedUser);
            return new ServiceActionResult(true)
            {
                Data = new{
                    UserInfo = new{
                        Name = retrivedUser.UserName,
                        Email = retrivedUser.Email
                    },
                    token = authResponse 
                }
            };
        }

        if (request.Role == UserRole.Provider)
        {
            throw new NotImplementedException();
        }

        throw new NotImplementedException();
    }

    public async Task<ServiceActionResult> RefreshTokenAsync(RefreshTokenRequest token)
    {
        var refreshToken = await _customerTokenRepository.GetAsync(t => t.Value == token.RefreshToken);
        if (refreshToken is null)
            throw new RefreshTokenNotFoundException(token.RefreshToken);
        var customer = await _customerRepository.GetAsync(c => c.Id == refreshToken.CustomerId);
        if (customer is null)
            throw new CustomerNotFoundException(refreshToken.CustomerId.ToString());
        var tokenResponse = GenerateAuthResponseAsync(customer);
        return new ServiceActionResult()
        {
            Data = tokenResponse
        };
    }

    public async Task<AuthResponse> GenerateAuthResponseAsync(Customer customer)
    {
        var reponse = new AuthResponse()
        {
            Name = customer.UserName ?? customer.Email ?? String.Empty,
            Role = UserRole.Client.ToString(),
            Token = new TokenResponse()
            {
                AccessToken = _jwtService.GenerateAccessToken(customer),
                RefreshToken = await _jwtService.GenerateRefreshToken(customer.Id),
                ExpiresAt = DateTimeOffset.Now.AddHours(_jwtSetting.RefreshTokenLifetimeInMinutes)
            }
        };
        return reponse;
    }
}