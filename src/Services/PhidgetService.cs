// Services/PhidgetService.cs
using Phidget22;
using Phidget22.Events;
using MicroJack.API.Services.Interfaces;
using ErrorEventArgs = Phidget22.Events.ErrorEventArgs;

namespace MicroJack.API.Services
{
    public class PhidgetService : IPhidgetService, IDisposable
    {
        private readonly ILogger<PhidgetService> _logger;
        private readonly Dictionary<int, DigitalOutput> _relays = new();
        private bool _initialized = false;
        private readonly object _lock = new object();

        public PhidgetService(ILogger<PhidgetService> logger)
        {
            _logger = logger;
        }

        public bool IsInitialized => _initialized;

        public async Task<bool> InitializeAsync()
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (_initialized)
                    {
                        _logger.LogInformation("Phidget ya está inicializado");
                        return true;
                    }

                    try
                    {
                        _logger.LogInformation("Inicializando PhidgetInterfaceKit 0/0/4...");

                        // Inicializar los 4 relés
                        for (int i = 0; i < 4; i++)
                        {
                            var relay = new DigitalOutput();
                            relay.Channel = i;
                            
                            // Agregar manejadores de eventos para logging
                            relay.Attach += OnPhidgetAttach;
                            relay.Detach += OnPhidgetDetach;
                            relay.Error += OnPhidgetError;

                            relay.Open(5000); // 5000ms timeout
                            _relays[i] = relay;
                        }

                        _initialized = true;
                        _logger.LogInformation("PhidgetInterfaceKit 0/0/4 inicializado exitosamente");
                        return true;
                    }
                    catch (PhidgetException ex)
                    {
                        _logger.LogError(ex, $"Error de Phidget: {ex.Description}");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error inesperado al inicializar Phidget");
                        return false;
                    }
                }
            });
        }

        public async Task<bool> SetRelayStateAsync(int channel, bool state)
        {
            if (!_initialized)
            {
                _logger.LogWarning("Intento de cambiar estado del relé sin inicializar el Phidget");
                return false;
            }

            if (channel < 0 || channel > 3)
            {
                _logger.LogWarning($"Canal inválido: {channel}. Debe ser 0-3.");
                return false;
            }

            return await Task.Run(() =>
            {
                try
                {
                    if (_relays.TryGetValue(channel, out var relay))
                    {
                        relay.State = state;
                        _logger.LogInformation($"Relé {channel} cambiado a: {(state ? "ON" : "OFF")}");
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"Relé {channel} no encontrado");
                        return false;
                    }
                }
                catch (PhidgetException ex)
                {
                    _logger.LogError(ex, $"Error de Phidget al cambiar estado del relé {channel}");
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error inesperado al cambiar estado del relé {channel}");
                    return false;
                }
            });
        }

        public bool? GetRelayState(int channel)
        {
            if (!_initialized) return null;
            if (channel < 0 || channel > 3) return null;

            try
            {
                if (_relays.TryGetValue(channel, out var relay))
                {
                    return relay.State;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo estado del relé {channel}");
                return null;
            }
        }

        public Dictionary<int, bool> GetAllRelayStates()
        {
            var states = new Dictionary<int, bool>();
            if (!_initialized) return states;

            try
            {
                foreach (var kvp in _relays)
                {
                    states[kvp.Key] = kvp.Value.State;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estados de los relés");
            }

            return states;
        }

        public void Close()
        {
            lock (_lock)
            {
                try
                {
                    foreach (var relay in _relays.Values)
                    {
                        if (relay.Attached)
                        {
                            relay.State = false; // Apagar antes de cerrar
                            relay.Close();
                        }
                    }
                    _relays.Clear();
                    _initialized = false;
                    _logger.LogInformation("Phidget cerrado exitosamente");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cerrando Phidget");
                }
            }
        }

        public void Dispose()
        {
            Close();
        }

        private void OnPhidgetAttach(object sender, AttachEventArgs e)
        {
            var relay = (DigitalOutput)sender;
            _logger.LogInformation($"Relé {relay.Channel} conectado");
        }

        private void OnPhidgetDetach(object sender, DetachEventArgs e)
        {
            var relay = (DigitalOutput)sender;
            _logger.LogInformation($"Relé {relay.Channel} desconectado");
        }

        private void OnPhidgetError(object sender, ErrorEventArgs e)
        {
            _logger.LogError($"Error en Phidget: {e.Description}");
        }
    }
}