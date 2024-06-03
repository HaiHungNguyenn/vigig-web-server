﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vigig.Api.Controllers.Base;
using Vigig.Service.Constants;
using Vigig.Service.Interfaces;
using Vigig.Service.Models.Common;

namespace Vigig.Api.Controllers;
public class ProvideServiceController : BaseApiController
{
    private readonly IProviderServiceService _providerServiceService;

    public ProvideServiceController(IProviderServiceService providerServiceService)
    {
        _providerServiceService = providerServiceService;
    }
    [HttpGet("/ac-cleaning-services")]
    [AllowAnonymous]
    public async Task<IActionResult> GetACCleaningServices([FromQuery] BasePaginatedRequest request)
    {
        return await ExecuteServiceLogic(async () =>
                await _providerServiceService
                    .GetAirConditionerServicesByTypeAsync(GigServiceConstant.AirConditioner.Cleaning,request)
                    .ConfigureAwait(false))
            .ConfigureAwait(false);
    }
    [HttpGet("/ac-repair-services")]
    [AllowAnonymous]
    public async Task<IActionResult> GetACRepairServices([FromQuery] BasePaginatedRequest request)
    {
        return await ExecuteServiceLogic(async () =>
                await _providerServiceService
                    .GetAirConditionerServicesByTypeAsync(GigServiceConstant.AirConditioner.Repair,request)
                    .ConfigureAwait(false))
            .ConfigureAwait(false);
    }
    
    [HttpGet("/ac-gas-refill-services")]
    [AllowAnonymous]
    public async Task<IActionResult> GetACGasRepairServices([FromQuery] BasePaginatedRequest request)
    {
        return await ExecuteServiceLogic(async () =>
                await _providerServiceService
                    .GetAirConditionerServicesByTypeAsync(GigServiceConstant.AirConditioner.GasRefill,request)
                    .ConfigureAwait(false))
            .ConfigureAwait(false);
    }
    
    [HttpGet("/ac-inspection-services")]
    [AllowAnonymous]
    public async Task<IActionResult> GetACInspectionServices([FromQuery] BasePaginatedRequest request)
    {
        return await ExecuteServiceLogic(async () =>
                await _providerServiceService
                    .GetAirConditionerServicesByTypeAsync(GigServiceConstant.AirConditioner.Inspection,request)
                    .ConfigureAwait(false))
            .ConfigureAwait(false);
    }
    
    [HttpGet("/provider-services/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProviderService(Guid id)
    {
        return await ExecuteServiceLogic(async () =>
                await _providerServiceService
                    .GetProviderServiceByIdAsync(id)
                    .ConfigureAwait(false))
            .ConfigureAwait(false);
    }

    [HttpGet("/my-services")]
    [Authorize(Roles = UserRoleConstant.Provider)]
    public async Task<IActionResult> GetOwnProviderService()
    {
        return await ExecuteServiceLogic(async () =>
                await _providerServiceService.GetOwnProviderServiceAsync(GetJwtToken()).ConfigureAwait(false))
            .ConfigureAwait(false);
    }
    private string GetJwtToken()
    {
        var authorizationHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authorizationHeader.Replace("bearer", "", StringComparison.OrdinalIgnoreCase).Trim();
        return token;
    }
}