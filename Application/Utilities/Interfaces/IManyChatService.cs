
using Application.Utilities.Dtos;

namespace Application.Utilities.Interfaces
{
    public interface IManyChatService
    {
        Task<string> CrearUsuarioAsync(UserDto user);
        Task AgregarCaracteristicasAsync(UserDto usuario);
        Task EjecutarFlowAsync(UserDto usuario, string flowNs);
    }
}
