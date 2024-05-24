﻿using Microsoft.AspNetCore.SignalR;
using Vigig.Api.Hubs.Models;
using Vigig.DAL.Interfaces;
using Vigig.Service.Exceptions.NotFound;
using Vigig.Service.Interfaces;
using Vigig.Service.Models.Request.Booking;
using Hub = Microsoft.AspNetCore.SignalR.Hub;

namespace Vigig.Api.Hubs;

public class BookingHub : Hub
{
    private readonly BookingConnectionPool _pool;
    private readonly IBookingService _bookingService;
    private readonly IBookingMessageService _messageService;
    private readonly IProviderServiceService _providerServiceService;
    private readonly IVigigUserRepository _vigigUserRepository;

    public BookingHub(BookingConnectionPool pool, IBookingService bookingService, IBookingMessageService messageService, IVigigUserRepository vigigUserRepository, IProviderServiceService providerServiceService)
    {
        _pool = pool;
        _bookingService = bookingService;
        _messageService = messageService;
        _vigigUserRepository = vigigUserRepository;
        _providerServiceService = providerServiceService;
    }

    public override Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext().Request.Query["userId"].ToString();
        var accessToken = Context.GetHttpContext().Request.Headers["Authorization"];
        var connectionId = Context.ConnectionId;

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(connectionId))
        {
            if (!_pool.connectionPool.ContainsKey(userId))
            {
                _pool.connectionPool[userId] = new List<string>();
            }
            _pool.connectionPool[userId].Add(connectionId);
        }

        return base.OnConnectedAsync();
    }
    // /booking-hub?bookingid=...
    public async Task PlaceBooking(BookingPlaceRequest request)
    {
        var accessToken = Context.GetHttpContext()?.Request.Query["access_token"].ToString();
        var providerService = await _providerServiceService.RetrieveProviderServiceByIdAsync(request.ProviderServiceId);

        var provider = await _vigigUserRepository.GetAsync(x => x.Id == providerService.ProviderId)
                       ?? throw new UserNotFoundException("providerService.ProviderId");
        var dtoPlacedBooking = await _bookingService.RetrievedPlaceBookingAsync(accessToken, request);
        var providerConnectionId = _pool.connectionPool[provider.Id.ToString()].LastOrDefault();
        if (providerConnectionId is not null)
            Clients.Client(providerConnectionId)?.SendAsync("triggerBooking",dtoPlacedBooking);
    }
}