using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Interfaces;

namespace WebApplication1.Services;

public class ClientService : IClientService
{
    private readonly ApbdContext _context;

    public ClientService(ApbdContext context)
    {
        _context = context;
    }

    public async Task<bool> DeleteClientAsync(int idClient)
    {
        var client = await _context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == idClient);

        if (client == null)
            throw new KeyNotFoundException("Client not found.");

        if (client.ClientTrips.Any())
            throw new InvalidOperationException("Client has assigned trips.");

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        return true;
    }
}
