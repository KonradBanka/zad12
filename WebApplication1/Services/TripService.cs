using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class TripService : ITripService
{
    private readonly ApbdContext _context;

    public TripService(ApbdContext context)
    {
        _context = context;
    }

    public async Task<object> GetTripsAsync(int page, int pageSize)
    {
        var totalTrips = await _context.Trips.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalTrips / pageSize);

        var trips = await _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.IdCountries)
            .Include(t => t.ClientTrips).ThenInclude(ct => ct.IdClientNavigation)
            .Select(t => new
            {
                t.Name,
                t.Description,
                t.DateFrom,
                t.DateTo,
                t.MaxPeople,
                Countries = t.IdCountries.Select(c => new { Name = c.Name }),
                Clients = t.ClientTrips.Select(ct => new
                {
                    ct.IdClientNavigation.FirstName,
                    ct.IdClientNavigation.LastName
                })
            })
            .ToListAsync();

        return new {
            pageNum = page,
            pageSize,
            allPages = totalPages,
            trips
        };
    }

    public async Task<string> AssignClientToTripAsync(int tripId, AssignClientDto dto)
    {
        if (await _context.Clients.AnyAsync(c => c.Pesel == dto.Pesel))
            throw new InvalidOperationException("Client with this PESEL already exists.");

        var trip = await _context.Trips
            .Include(t => t.ClientTrips)
            .FirstOrDefaultAsync(t => t.IdTrip == tripId);

        if (trip == null)
            throw new KeyNotFoundException("Trip not found.");

        if (trip.DateFrom <= DateTime.Now)
            throw new InvalidOperationException("Trip has already started.");

        var isAlreadyRegistered = await _context.ClientTrips
            .AnyAsync(ct => ct.IdClientNavigation.Pesel == dto.Pesel && ct.IdTrip == tripId);

        if (isAlreadyRegistered)
            throw new InvalidOperationException("Client already registered to this trip.");

        var client = new Client {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Telephone = dto.Telephone,
            Pesel = dto.Pesel
        };

        var clientTrip = new ClientTrip {
            IdClientNavigation = client,
            IdTrip = tripId,
            PaymentDate = dto.PaymentDate,
            RegisteredAt = DateTime.UtcNow
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return "Client successfully assigned to trip.";
    }
}
