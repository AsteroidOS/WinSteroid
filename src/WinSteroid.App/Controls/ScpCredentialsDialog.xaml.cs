using System.Text;
using Windows.UI.Xaml.Controls;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.Controls
{
    public sealed partial class ScpCredentialsDialog : ContentDialog
    {
        public string HostIP { get; set; }

        public string Username { get; set; }
        
        public string Password { get; set; }

        public string ValidationSummary { get; set; }

        public ScpCredentialsDialog()
        {
            this.HostIP = string.Empty;
            this.Username = string.Empty;
            this.Password = string.Empty;
            this.ValidationSummary = string.Empty;
            this.InitializeComponent();
        }

        private void OnPrimaryButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var validHostIP = IPHelper.IsValid(this.HostIP);
            var validUsername = !string.IsNullOrWhiteSpace(this.Username);
            var validPassword = true;

            var validForm = validHostIP && validUsername && validPassword;
            if (!validForm)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Invalid fields:");

                if (!validHostIP)
                {
                    stringBuilder.AppendLine("- Host / IP");
                }

                if (!validUsername)
                {
                    stringBuilder.AppendLine("- Username");
                }

                if (!validPassword)
                {
                    stringBuilder.AppendLine("- Password");
                }

                this.ValidationSummary = stringBuilder.ToString();
            }
            else
            {
                this.ValidationSummary = string.Empty;
            }

            args.Cancel = !validForm;
        }
    }
}
