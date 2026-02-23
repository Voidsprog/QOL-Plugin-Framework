using System;
using System.Linq;
using System.Reflection;

namespace QOLFramework.Hybrid
{
    /// <summary>
    /// Bridge para deteção e uso de EXILED em runtime.
    /// O plugin funciona com LabAPI por defeito.
    /// Se EXILED estiver disponível, usa os seus recursos adicionais.
    /// Todas as chamadas são seguras — retornam false/null se EXILED não existir.
    /// </summary>
    public static class ExiledBridge
    {
        private static bool? _isExiledAvailable;
        private static Assembly _exiledApi;
        private static Assembly _exiledEvents;

        /// <summary>Verifica se EXILED está carregado no servidor.</summary>
        public static bool IsExiledAvailable
        {
            get
            {
                if (_isExiledAvailable.HasValue) return _isExiledAvailable.Value;

                try
                {
                    _exiledApi = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "Exiled.API");

                    _exiledEvents = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "Exiled.Events");

                    _isExiledAvailable = _exiledApi != null;

                    if (_isExiledAvailable.Value)
                        LabApi.Features.Console.Logger.Info("[QOL:Hybrid] EXILED detetado! Modo híbrido ativo (LabAPI + EXILED).");
                    else
                        LabApi.Features.Console.Logger.Info("[QOL:Hybrid] EXILED não encontrado. A usar apenas LabAPI.");
                }
                catch
                {
                    _isExiledAvailable = false;
                }

                return _isExiledAvailable.Value;
            }
        }

        /// <summary>Envia uma mensagem CASSIE via EXILED (se disponível). Retorna true se enviado.</summary>
        public static bool TryCassieMessage(string message, bool isHeld = false, bool isNoisy = true)
        {
            if (!IsExiledAvailable || _exiledApi == null) return false;

            try
            {
                var cassieType = _exiledApi.GetType("Exiled.API.Features.Cassie");
                if (cassieType == null) return false;

                var method = cassieType.GetMethod("Message",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string), typeof(bool), typeof(bool) },
                    null);

                if (method == null) return false;

                method.Invoke(null, new object[] { message, isHeld, isNoisy });
                return true;
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Warn($"[QOL:Hybrid] CASSIE via EXILED falhou: {ex.Message}");
                return false;
            }
        }

        /// <summary>Envia broadcast a todos os jogadores via EXILED. Retorna true se enviado.</summary>
        public static bool TryBroadcast(string message, ushort duration = 5)
        {
            if (!IsExiledAvailable || _exiledApi == null) return false;

            try
            {
                var mapType = _exiledApi.GetType("Exiled.API.Features.Map");
                if (mapType == null) return false;

                var method = mapType.GetMethod("Broadcast",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(ushort), typeof(string) },
                    null);

                if (method != null)
                {
                    method.Invoke(null, new object[] { duration, message });
                    return true;
                }
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Warn($"[QOL:Hybrid] Broadcast via EXILED falhou: {ex.Message}");
            }
            return false;
        }

        /// <summary>Obtém a versão do EXILED carregado (se disponível).</summary>
        public static string GetExiledVersion()
        {
            if (!IsExiledAvailable || _exiledApi == null) return null;

            try
            {
                return _exiledApi.GetName().Version?.ToString() ?? "desconhecida";
            }
            catch
            {
                return "desconhecida";
            }
        }

        /// <summary>Reseta o cache de deteção (usar quando o servidor reinicia).</summary>
        public static void Reset()
        {
            _isExiledAvailable = null;
            _exiledApi = null;
            _exiledEvents = null;
        }
    }
}
