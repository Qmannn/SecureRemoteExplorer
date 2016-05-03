namespace ExplorerClient.Core.Objects
{
    public class SentFile
    {
        public string Uuid { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Name { get; set; }

        private string _isRecived;

        public string Recived
        {
            get { return _isRecived; }
            set { _isRecived = value == true.ToString() ? "Да" : "Нет"; }
        }

        public string Comment { get; set; }

        public string SendTime { get; set; }
    }
}