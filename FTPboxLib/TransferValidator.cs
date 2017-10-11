using System.IO;

namespace FTPboxLib
{
    public abstract class TransferValidator
    {
        protected AccountController Controller;

        public abstract bool Validate(ClientItem local, string remote);

        public abstract bool Validate(string local, ClientItem remote);

        public virtual bool TryValidate(SyncQueueItem item, string remote)
        {
            if (Controller.Client.Exists(item.CommonPath))
                return Validate(item.Item, item.CommonPath);
            else
                return false;
        }
    }

    public class SizeTransferValidator : TransferValidator
    {
        public SizeTransferValidator(AccountController controller)
        {
            Controller = controller;
        }

        public override bool Validate(ClientItem local, string remote)
        {
            var expectedSize = Controller.Client.SizeOf(remote);

            Log.Write(l.Debug, $"Validating size of {local.Name} : expected {expectedSize} was {local.Size}");

            return local.Size == expectedSize;
        }

        public override bool Validate(string local, ClientItem remote)
        {
            var expectedSize = new FileInfo(local).Length;

            Log.Write(l.Debug, $"Validating size of {remote.Name} : expected {expectedSize} was {remote.Size}");

            return remote.Size == expectedSize;
        }
    }
}
