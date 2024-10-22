namespace StatsPerform.CollectionClients.Utilities.IO.Devices.ShuttleXPress
{
    public class ShuttleEventArgs
    {
        #region Private class members


        #endregion Private class members

        #region Enums

        public enum DirectionTurned : byte
        {
            Left = 0,
            Right = 1
        }

        #endregion Enums

        #region Property accessors

        public int AmountTurned { get; set; }

        public DirectionTurned Direction { get; set; }

        #endregion Property accessors
    }
}