using DocumentFormat.OpenXml.Drawing.Charts;
using MauiApp_PracticaNotas.Models;
using MauiApp_PracticaNotas.Services;
using System.Diagnostics;

namespace MauiApp_PracticaNotas
{
    public partial class MainPage : ContentPage
    {
        // ============================================
        // VARIABLES ORIGINALES
        // ============================================
        private bool isMenuOpen = false;

        // ============================================
        // VARIABLES DE ESTADO (Requisito 3)
        // ============================================

        /// <summary>
        /// Contador de notas creadas en esta sesión
        /// Se preserva entre cambios de estado
        /// </summary>
        private int _notasCreadas = 0;

        /// <summary>
        /// Contador de veces que se abrió el menú
        /// </summary>
        private int _vecesMenuAbierto = 0;

        /// <summary>
        /// Timestamp de la última interacción del usuario
        /// </summary>
        private DateTime _ultimaInteraccion = DateTime.Now;

        /// <summary>
        /// Número de cambios de estado de la aplicación
        /// </summary>
        private int _cambiosDeEstado = 0;

        /// <summary>
        /// Indica si hay datos sin guardar (draft)
        /// </summary>
        private bool _hayDatosSinGuardar = false;

        public MainPage()
        {
            InitializeComponent();

            RegistrarEvento("Constructor", "Inicializando MainPage");
            LogToConsole("📱 CONSTRUCTOR - MainPage creada");
        }

        // ============================================
        // EVENTOS DEL CICLO DE VIDA (Requisito 2 y 4)
        // ============================================

        /// <summary>
        /// Se llama cuando la página está a punto de aparecer
        /// AQUÍ RECUPERAMOS EL ESTADO GUARDADO
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            _cambiosDeEstado++;

            LogToConsole("👁️ OnAppearing - La página está apareciendo");
            RegistrarEvento("OnAppearing", $"Página visible (Cambio #{_cambiosDeEstado})");

            // RECUPERAR ESTADO GUARDADO (Requisito 4)
            RecuperarEstadoGuardado();

            // Cargar notas como antes
            LoadNotes();

            // Actualizar el título con estadísticas
            ActualizarTituloConEstadisticas();
        }

        /// <summary>
        /// Se llama cuando la página desaparece
        /// AQUÍ GUARDAMOS EL ESTADO
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            LogToConsole("👋 OnDisappearing - La página está desapareciendo");
            RegistrarEvento("OnDisappearing", "Página oculta");

            // Guardar estado antes de desaparecer
            GuardarEstadoActual();
        }

        /// <summary>
        /// Se llama al navegar a esta página
        /// </summary>
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            LogToConsole("➡️ OnNavigatedTo - Navegación completada");
            RegistrarEvento("OnNavigatedTo", "Usuario llegó a la pantalla principal");
        }

        /// <summary>
        /// Se llama cuando el usuario está saliendo de esta página
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
        {
            base.OnNavigatingFrom(args);

            LogToConsole("⬅️ OnNavigatingFrom - Usuario saliendo");
            RegistrarEvento("OnNavigatingFrom", "Navegando a otra pantalla");

            // Guardar por seguridad antes de salir
            GuardarEstadoActual();

            // Si hay datos sin guardar, avisar
            if (_hayDatosSinGuardar && !string.IsNullOrWhiteSpace(ActivityEntry?.Text))
            {
                LogToConsole("⚠️ Advertencia: Hay texto en el campo sin guardar");
            }
        }

        // ============================================
        // GESTIÓN DE ESTADO (Requisito 3 y 4)
        // ============================================

        /// <summary>
        /// Guarda el estado actual usando Preferences
        /// </summary>
        private void GuardarEstadoActual()
        {
            try
            {
                // Guardar estadísticas de la sesión
                Preferences.Set("notas_creadas", _notasCreadas);
                Preferences.Set("veces_menu_abierto", _vecesMenuAbierto);
                Preferences.Set("ultima_interaccion", _ultimaInteraccion.ToString("o"));
                Preferences.Set("cambios_estado", _cambiosDeEstado);

                // Guardar draft si hay texto
                if (!string.IsNullOrWhiteSpace(ActivityEntry?.Text))
                {
                    Preferences.Set("draft_texto", ActivityEntry.Text);
                    Preferences.Set("draft_urgente", UrgentCheckBox?.IsChecked ?? false);
                    LogToConsole($"💾 Draft guardado: '{ActivityEntry.Text.Substring(0, Math.Min(20, ActivityEntry.Text.Length))}...'");
                }
                else
                {
                    Preferences.Remove("draft_texto");
                    Preferences.Remove("draft_urgente");
                }

                LogToConsole($"💾 Estado guardado - Notas creadas: {_notasCreadas}, Cambios: {_cambiosDeEstado}");
                RegistrarEvento("Guardado", $"Estado preservado: {_notasCreadas} notas");
            }
            catch (Exception ex)
            {
                LogToConsole($"❌ Error al guardar: {ex.Message}");
            }
        }

        /// <summary>
        /// Recupera el estado guardado previamente
        /// </summary>
        private void RecuperarEstadoGuardado()
        {
            try
            {
                // Recuperar estadísticas
                _notasCreadas = Preferences.Get("notas_creadas", 0);
                _vecesMenuAbierto = Preferences.Get("veces_menu_abierto", 0);
                _cambiosDeEstado = Preferences.Get("cambios_estado", _cambiosDeEstado);

                var ultimaInteraccionStr = Preferences.Get("ultima_interaccion", string.Empty);
                if (!string.IsNullOrEmpty(ultimaInteraccionStr))
                {
                    _ultimaInteraccion = DateTime.Parse(ultimaInteraccionStr);

                    // Calcular tiempo transcurrido
                    var tiempoTranscurrido = DateTime.Now - _ultimaInteraccion;
                    LogToConsole($"⏱️ Última interacción hace {tiempoTranscurrido.TotalMinutes:F1} minutos");
                }

                // Recuperar draft si existe
                var draftTexto = Preferences.Get("draft_texto", string.Empty);
                if (!string.IsNullOrEmpty(draftTexto) && ActivityEntry != null)
                {
                    ActivityEntry.Text = draftTexto;
                    UrgentCheckBox.IsChecked = Preferences.Get("draft_urgente", false);
                    _hayDatosSinGuardar = true;

                    LogToConsole($"📝 Draft recuperado: '{draftTexto.Substring(0, Math.Min(20, draftTexto.Length))}...'");
                    RegistrarEvento("Recuperación", "Draft de nota restaurado");
                }

                if (_notasCreadas > 0)
                {
                    LogToConsole($"♻️ Estado recuperado - Notas creadas en sesiones anteriores: {_notasCreadas}");
                    RegistrarEvento("Recuperación", $"Sesión restaurada: {_notasCreadas} notas totales");
                }
                else
                {
                    LogToConsole("ℹ️ Primera vez usando la app o sesión nueva");
                    RegistrarEvento("Primera vez", "Iniciando nueva sesión");
                }
            }
            catch (Exception ex)
            {
                LogToConsole($"❌ Error al recuperar: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza el título con las estadísticas de uso
        /// </summary>
        private void ActualizarTituloConEstadisticas()
        {
            // Podrías agregar un Label para mostrar estadísticas
            // Por ahora solo lo registramos en consola
            LogToConsole($"📊 Estadísticas - Notas: {_notasCreadas}, Menú abierto: {_vecesMenuAbierto}x");
        }

        // ============================================
        // REGISTRO Y LOGS (Requisito 2)
        // ============================================

        /// <summary>
        /// Registra un evento en consola con formato
        /// </summary>
        private void RegistrarEvento(string tipo, string descripcion)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.WriteLine($"[CICLO DE VIDA] [{timestamp}] {tipo}: {descripcion}");
        }

        /// <summary>
        /// Escribe en la consola de debug
        /// </summary>
        private void LogToConsole(string mensaje)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.WriteLine($"[NOTAS APP] {timestamp} - {mensaje}");
        }

        // ============================================
        // MÉTODOS ORIGINALES MEJORADOS
        // ============================================

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            string activity = ActivityEntry.Text;

            if (string.IsNullOrWhiteSpace(activity))
            {
                LogToConsole("⚠️ Usuario intentó guardar sin texto");
                RegistrarEvento("Validación", "Campo vacío detectado");
                await DisplayAlert("Error", "Por favor escribe una actividad", "OK");
                return;
            }

            bool isUrgent = UrgentCheckBox.IsChecked;

            var note = new Note
            {
                Activity = activity,
                IsUrgent = isUrgent
            };

            NotesService.Instance.AddOrUpdateNote(note);

            // ACTUALIZAR CONTADOR (Requisito 3)
            _notasCreadas++;
            _ultimaInteraccion = DateTime.Now;
            _hayDatosSinGuardar = false;

            // Limpiar campos
            ActivityEntry.Text = string.Empty;
            UrgentCheckBox.IsChecked = false;

            // GUARDAR ESTADO INMEDIATAMENTE
            GuardarEstadoActual();

            LogToConsole($"✅ Nota #{_notasCreadas} guardada - Urgente: {isUrgent}");
            RegistrarEvento("Nota guardada", $"Total de notas en sesión: {_notasCreadas}");

            await DisplayAlert("Éxito", $"Nota guardada correctamente\n\nNotas en esta sesión: {_notasCreadas}", "OK");

            LoadNotes();
        }

        private void LoadNotes()
        {
            var activeNotes = NotesService.Instance.GetActiveNotes();
            NotesCollectionView.ItemsSource = activeNotes;
            EmptyLabel.IsVisible = !activeNotes.Any();

            LogToConsole($"📋 Cargadas {activeNotes.Count()} notas activas");
            RegistrarEvento("Carga", $"{activeNotes.Count()} notas mostradas");
        }

        private async void OnDeleteNoteClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var noteId = button?.CommandParameter as string;

            if (noteId == null) return;

            LogToConsole($"🗑️ Usuario solicita eliminar nota: {noteId}");
            RegistrarEvento("Acción", "Solicitud de eliminación");

            bool confirm = await DisplayAlert(
                "Eliminar Nota",
                "¿Deseas mover esta nota a la papelera?",
                "Sí",
                "No");

            if (confirm)
            {
                NotesService.Instance.MoveToTrash(noteId);

                _ultimaInteraccion = DateTime.Now;
                GuardarEstadoActual();

                LoadNotes();

                LogToConsole($"✅ Nota {noteId} movida a papelera");
                RegistrarEvento("Eliminación", "Nota movida a papelera");

                await DisplayAlert("Éxito", "Nota movida a la papelera", "OK");
            }
            else
            {
                LogToConsole("❌ Eliminación cancelada por usuario");
            }
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            // INCREMENTAR CONTADOR DE MENÚ
            _vecesMenuAbierto++;
            _ultimaInteraccion = DateTime.Now;

            LogToConsole($"☰ Menú {(isMenuOpen ? "cerrado" : "abierto")} - Total: {_vecesMenuAbierto}x");
            RegistrarEvento("Menú", $"Toggle menú (abierto {_vecesMenuAbierto} veces)");

            if (isMenuOpen)
            {
                await CloseMenu();
            }
            else
            {
                await OpenMenu();
            }

            // Guardar el contador actualizado
            GuardarEstadoActual();
        }

        private async Task OpenMenu()
        {
            isMenuOpen = true;
            Overlay.IsVisible = true;
            Overlay.InputTransparent = false;

            var tasks = new List<Task>
            {
                SideMenu.TranslateTo(0, 0, 250, Easing.CubicOut),
                Overlay.FadeTo(0.5, 250)
            };

            await Task.WhenAll(tasks);

            LogToConsole("✅ Menú abierto completamente");
        }

        private async Task CloseMenu()
        {
            isMenuOpen = false;

            var tasks = new List<Task>
            {
                SideMenu.TranslateTo(-250, 0, 250, Easing.CubicIn),
                Overlay.FadeTo(0, 250)
            };

            await Task.WhenAll(tasks);

            Overlay.IsVisible = false;
            Overlay.InputTransparent = true;

            LogToConsole("✅ Menú cerrado completamente");
        }

        private async void OnOverlayTapped(object sender, EventArgs e)
        {
            LogToConsole("👆 Usuario cerró menú tocando overlay");
            await CloseMenu();
        }

        private async void OnActiveNotesClicked(object sender, EventArgs e)
        {
            LogToConsole("📝 Usuario seleccionó 'Notas Activas' (ya está aquí)");
            RegistrarEvento("Navegación", "Notas activas seleccionadas");
            await CloseMenu();
        }

        private async void OnTrashClicked(object sender, EventArgs e)
        {
            LogToConsole("🗑️ Usuario navegando a papelera");
            RegistrarEvento("Navegación", "Abriendo papelera");

            await CloseMenu();
            await Navigation.PushAsync(new TrashPage());
        }

        // ============================================
        // MONITOREO DE CAMBIOS EN CAMPOS (Nuevo)
        // ============================================

        /// <summary>
        /// Detecta cuando el usuario escribe en el campo
        /// Podrías conectar esto con el evento TextChanged del Entry
        /// </summary>
        private void OnActivityTextChanged(object sender, TextChangedEventArgs e)
        {
            _hayDatosSinGuardar = !string.IsNullOrWhiteSpace(e.NewTextValue);
            _ultimaInteraccion = DateTime.Now;

            if (_hayDatosSinGuardar)
            {
                // Guardar automáticamente el draft cada pocos segundos
                GuardarEstadoActual();
            }
        }
    }

    // ============================================
    // CONVERTIDORES ORIGINALES (Sin cambios)
    // ============================================

    public class UrgentColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isUrgent)
            {
                return isUrgent ? Color.FromArgb("#E74C3C") : Color.FromArgb("#3498DB");
            }
            return Color.FromArgb("#3498DB");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UrgentTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isUrgent)
            {
                return isUrgent ? "URGENTE" : "Normal";
            }
            return "Normal";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
