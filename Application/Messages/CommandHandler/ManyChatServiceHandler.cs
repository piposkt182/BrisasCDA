using Application.Utilities.Dtos;
using Application.Utilities.Interfaces;

namespace Application.Messages.CommandHandler
{
    public class ManyChatServiceHandler
    {
        private readonly IManyChatService _manyChatService;

        public ManyChatServiceHandler(IManyChatService manyChatService)
        {
            _manyChatService = manyChatService;
        }

        public async Task ProcesarUsuariosPorFasesAsync(List<UserDto> usuarios, string flowNs)
        {
            var semaphore = new SemaphoreSlim(5); 

            // 🔹 FASE 1: Crear usuarios
            Console.WriteLine("🚀 Creando usuarios en ManyChat...");
            var crearTareas = usuarios.Select(async usuario =>
            {
                await semaphore.WaitAsync();
                try
                {
                    usuario.SubscriberId = await _manyChatService.CrearUsuarioAsync(usuario);
                    Console.WriteLine($"✅ Usuario creado: {usuario.first_name} {usuario.last_name}({usuario.SubscriberId})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error creando usuario {usuario.first_name} {usuario.last_name}: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            });
            await Task.WhenAll(crearTareas);

            // 🔹 FASE 2: Asignar características
            Console.WriteLine("\n⚙️ Asignando características...");
            var caracteristicasTareas = usuarios
                .Where(u => !string.IsNullOrEmpty(u.SubscriberId))
                .Select(async usuario =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await _manyChatService.AgregarCaracteristicasAsync(usuario);
                        Console.WriteLine($"✅ Campos personalizados agregados a {usuario.first_name} {usuario.last_name}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            await Task.WhenAll(caracteristicasTareas);

            // 🔹 FASE 3: Ejecutar flow
            Console.WriteLine("\n💬 Ejecutando flow para cada usuario...");
            var flowTareas = usuarios
                .Where(u => !string.IsNullOrEmpty(u.SubscriberId))
                .Select(async usuario =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await _manyChatService.EjecutarFlowAsync(usuario, flowNs);
                        Console.WriteLine($"✅ Flow ejecutado para {usuario.first_name}   {usuario.last_name}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            await Task.WhenAll(flowTareas);

            Console.WriteLine("\n🎯 Proceso completado con éxito.");
        }
    }
}
