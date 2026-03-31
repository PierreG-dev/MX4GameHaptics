namespace GamepadHaptics
{
    using System;
    using Loupedeck;

    public class GamepadHapticsCommand : PluginDynamicCommand
    {
        private GamepadHapticsPlugin _plugin;
        private bool _subscribed;

        public GamepadHapticsCommand()
            : base(
                "Gamepad Haptics",
                "Monitors rumble from games",
                "Gamepad"
            )
        {
        }

        protected override void RunCommand(string actionParameter)
        {
            this._plugin ??= this.Plugin as GamepadHapticsPlugin;
            this.EnsureSubscribed();
            this._plugin?.Log.Info("Haptics command activated");
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(
            string actionParameter,
            PluginImageSize imageSize
        )
        {
            var builder = new BitmapBuilder(imageSize);
            builder.Clear(BitmapColor.Black);

            var active = this._subscribed
                && this._plugin?.Controller != null;

            builder.FillRectangle(
                0, 0,
                builder.Width, builder.Height,
                active
                    ? new BitmapColor(0, 80, 0)
                    : new BitmapColor(80, 0, 0)
            );

            builder.DrawText(
                active ? "Haptics\nON" : "Haptics\nOFF",
                BitmapColor.White
            );

            return builder.ToImage();
        }

        private void EnsureSubscribed()
        {
            if (this._subscribed)
                return;

            this._plugin ??= this.Plugin as GamepadHapticsPlugin;
            if (this._plugin?.Controller == null)
                return;

            this._plugin.Controller.OnRumble += this.HandleRumble;
            this._subscribed = true;
            this._plugin.Log.Info("Subscribed to rumble events");
        }

        private void HandleRumble(byte large, byte small)
        {
            int intensity = Math.Max(large, small);
            if (intensity < 10)
                return;

            this._plugin?.Log.Info(
                $"Rumble: large={large} small={small}"
            );

            // TODO: Déclencher haptique MX Master ici
            // Pour l'instant on log pour confirmer que ça marche
        }
    }
}