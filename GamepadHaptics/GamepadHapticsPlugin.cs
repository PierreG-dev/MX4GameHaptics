namespace GamepadHaptics
{
    using System;
    using Loupedeck;

    public class GamepadHapticsPlugin : Plugin
    {
        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;

        public override void Load()
        {
            this.Log.Info("GamepadHaptics loading...");

            if (!ViGEmController.IsDriverInstalled())
            {
                this.Log.Warning(
                    "ViGEmBus driver not found. "
                    + "Install from: "
                    + "https://github.com/nefarius/ViGEmBus/releases"
                );
                return;
            }

            try
            {
                this.Controller = new ViGEmController(
                    msg => this.Log.Info(msg)
                );
                this.Controller.Start();
            }
            catch (Exception ex)
            {
                this.Log.Error(
                    $"Failed to start ViGEm: {ex.Message}"
                );
                this.Controller = null;
            }

            this.Log.Info("GamepadHaptics loaded");
        }

        public override void Unload()
        {
            try
            {
                this.Controller?.Stop();
            }
            catch (Exception ex)
            {
                this.Log.Error(
                    $"Error stopping controller: {ex.Message}"
                );
            }

            this.Controller = null;
            this.Log.Info("GamepadHaptics unloaded");
        }

        internal ViGEmController Controller { get; private set; }
    }
}