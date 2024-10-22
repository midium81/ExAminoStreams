namespace ExAminoStreams
{
    public static class Extensions
    {
        public static int TotalWholeMilliseconds(this TimeSpan timeSpan)
        {
            return (int)Math.Round(Math.Floor(timeSpan.TotalMilliseconds));
        }
    }
}
