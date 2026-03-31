namespace GamepadHaptics
{
    using System;
    using Nefarius.ViGEm.Client;
    using Nefarius.ViGEm.Client.Targets;
    using Nefarius.ViGEm.Client.Targets.Xbox360;

    public class ViGEmController : IDisposable
    {
        private ViGEmClient _client;
        private IXbox360Controller _pad;
        private readonly Action<string> _log;

        public event Action<byte, byte> OnRumble;

        public ViGEmController(Action<string> log = null)
        {
            this._log = log ?? (_ => { });
        }

        public static bool IsDriverInstalled()
        {
            try
            {
                using var client = new ViGEmClient();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Start()
        {
            this._client = new ViGEmClient();
            this._pad = this._client.CreateXbox360Controller();

            this._pad.FeedbackReceived += (_, e) =>
                this.OnRumble?.Invoke(e.LargeMotor, e.SmallMotor);

            this._pad.Connect();
            this._log("Virtual Xbox360 controller connected");
        }

        public void Stop()
        {
            try
            {
                this._pad?.Disconnect();
            }
            catch
            {
                // Ignorer si déjà déconnecté
            }

            this._client?.Dispose();
            this._client = null;
            this._pad = null;
            this._log("Virtual controller disconnected");
        }

        public void Dispose() => this.Stop();
    }
}