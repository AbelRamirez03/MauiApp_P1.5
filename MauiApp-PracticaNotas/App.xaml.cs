using System.Diagnostics;

namespace MauiApp_PracticaNotas
{
    public partial class App : Application
    {
        // Contador de cambios de estado a nivel aplicación
        private int _estadosAppCambiados = 0;

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());

            LogToConsole("🚀 Constructor de App - Aplicación creada");
            RegistrarEvento("Constructor", "App inicializada");
        }

        // ============================================
        // EVENTOS DEL CICLO DE VIDA A NIVEL APLICACIÓN
        // (Requisito 2 - Mostrar cambios de estado)
        // ============================================

        /// <summary>
        /// Se ejecuta cuando la aplicación inicia por primera vez
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            _estadosAppCambiados++;

            LogToConsole("▶️ OnStart - La aplicación ha INICIADO");
            LogToConsole($"   📊 Estado: ACTIVA (Cambio #{_estadosAppCambiados})");
            RegistrarEvento("OnStart", "Aplicación iniciada");

            // Guardar timestamp del inicio
            Preferences.Set("app_ultimo_inicio", DateTime.Now.ToString("o"));
            Preferences.Set("app_total_inicios", Preferences.Get("app_total_inicios", 0) + 1);

            var totalInicios = Preferences.Get("app_total_inicios", 0);
            LogToConsole($"   📈 Esta es la vez #{totalInicios} que se inicia la app");
        }

        /// <summary>
        /// CRÍTICO: Se ejecuta cuando la app pasa a SEGUNDO PLANO
        /// Ejemplo: Usuario presiona botón Home, recibe llamada, cambia de app
        /// ⚠️ Aquí DEBEMOS guardar todo porque la app puede ser terminada
        /// </summary>
        protected override void OnSleep()
        {
            base.OnSleep();
            _estadosAppCambiados++;

            LogToConsole("💤 OnSleep - La aplicación pasa a SEGUNDO PLANO");
            LogToConsole($"   📊 Estado: EN PAUSA (Cambio #{_estadosAppCambiados})");
            LogToConsole("   ⚠️ ADVERTENCIA: La app puede ser terminada en cualquier momento");
            RegistrarEvento("OnSleep", "App en segundo plano");

            // CRÍTICO: Guardar el estado aquí (Requisito 4)
            Preferences.Set("app_ultimo_sleep", DateTime.Now.ToString("o"));
            Preferences.Set("app_estados_cambiados", _estadosAppCambiados);

            LogToConsole("   💾 Estado de la aplicación guardado exitosamente");
            LogToConsole("   ✅ Es seguro que el SO termine la app ahora");
        }

        /// <summary>
        /// Se ejecuta cuando la app VUELVE del segundo plano
        /// Ejemplo: Usuario regresa a la app desde el selector de tareas
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();
            _estadosAppCambiados++;

            LogToConsole("🔄 OnResume - La aplicación vuelve a PRIMER PLANO");
            LogToConsole($"   📊 Estado: ACTIVA DE NUEVO (Cambio #{_estadosAppCambiados})");
            RegistrarEvento("OnResume", "App reanudada");

            // Calcular cuánto tiempo estuvo en segundo plano
            var ultimoSleep = Preferences.Get("app_ultimo_sleep", string.Empty);
            if (!string.IsNullOrEmpty(ultimoSleep))
            {
                var tiempoSleep = DateTime.Parse(ultimoSleep);
                var duracion = DateTime.Now - tiempoSleep;

                LogToConsole($"   ⏱️ Tiempo en segundo plano: {duracion.TotalSeconds:F1} segundos");

                if (duracion.TotalMinutes > 5)
                {
                    LogToConsole($"   ℹ️ Usuario estuvo ausente {duracion.TotalMinutes:F0} minutos");
                }
            }

            // Recuperar contador de estados
            _estadosAppCambiados = Preferences.Get("app_estados_cambiados", _estadosAppCambiados);
        }

        // NOTA: OnUnhandledException no está disponible en todas las versiones de MAUI
        // Para manejo de excepciones, considera usar try-catch en métodos críticos

        // ============================================
        // MÉTODOS AUXILIARES
        // ============================================

        /// <summary>
        /// Escribe en la consola de debug con formato consistente
        /// </summary>
        private void LogToConsole(string mensaje)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.WriteLine($"[APP LIFECYCLE] {timestamp} - {mensaje}");
        }

        /// <summary>
        /// Registra eventos del ciclo de vida
        /// </summary>
        private void RegistrarEvento(string tipo, string descripcion)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.WriteLine($"[CICLO DE VIDA - APP] [{timestamp}] {tipo}: {descripcion}");
        }
    }
}
