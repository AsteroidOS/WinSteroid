namespace WinSteroid.Common.Models
{
    public class PairingResult
    {
        public PairingResult(string errorMessage) : this(false, errorMessage) { }

        private PairingResult(bool isSuccess, string errorMessage)
        {
            this.IsSuccess = isSuccess;
            this.ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }

        public string ErrorMessage { get; }

        public static PairingResult Success => new PairingResult(true, null);
    }
}
