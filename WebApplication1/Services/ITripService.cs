using WebApplication1.DTOs;

namespace WebApplication1.Interfaces;

public interface ITripService
{
    Task<object> GetTripsAsync(int page, int pageSize);
    Task<string> AssignClientToTripAsync(int tripId, AssignClientDto dto);
}
