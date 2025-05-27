namespace WebApplication1.Interfaces;

public interface IClientService
{
    Task<bool> DeleteClientAsync(int idClient);
}
