using System;

namespace FTPboxLib
{
    public class TransferProgress
    {
        /// <summary>
        /// TransferProgress constructor.
        /// </summary>
        /// <param name="total">total bytes transferred</param>
        /// <param name="item">the transferred item</param>
        /// <param name="started">when the transfer started</param>
        public TransferProgress(long total, SyncQueueItem item, DateTime started)
        {
            TotalTransferred = total;
            Total = item.Item.Size;
            StartedOn = started;
        }

        // Total bytes transferred
        public long TotalTransferred;
        // The size of the transferred file
        public long Total;
        // Started on
        public DateTime StartedOn;

        // Total bytes to be transferred
        public int Progress => (int)(100 * TotalTransferred / Total);

        public string ProgressFormatted => $"{Progress,3}% - {Rate}";

        // Transfer Rate
        public string Rate
        {
            get
            {
                var elapsed = DateTime.Now.Subtract(StartedOn);
                var rate = (elapsed.TotalSeconds < 1 ? TotalTransferred : TotalTransferred / elapsed.TotalSeconds);
                var f = "bytes";
                if (rate > 1024 * 1024)
                {
                    rate /= (1024 * 1024);
                    f = "MB";
                }
                else if (rate > 1024)
                {
                    rate /= 1024;
                    f = "KB";
                }

                rate = Math.Round(rate, 2);

                Console.Write("\r Transferred {0:p} bytes @ {1} {2}/s", (double)TotalTransferred / Total, rate, f);

                return string.Format("{0} {1}/s", rate, f);
            }
        }
    }
}
