namespace Windows.ApplicationModel
{
    public static class PackageExtensions
    {
        public static string GetVersion(this PackageId packageId)
        {
            return $"{packageId.Version.Major}.{packageId.Version.Minor}.{packageId.Version.Build}.{packageId.Version.Revision}";
        }
    }
}
