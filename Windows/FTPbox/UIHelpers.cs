using FTPboxLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FTPbox
{
    class UIHelpers
    {
        #region TreeNode Helpers

        /// <summary>
        /// Recursively add child nodes
        /// </summary>
        public static TreeNode ConstructNodeFrom(List<ClientItem> li, ClientItem d)
        {
            var parent = new TreeNode(d.Name);

            var folders = li
                .Where(x => x.FullPath != d.FullPath)
                .Where(x => x.Type == ClientItemType.Folder && x.FullPath.StartsWith(d.FullPath));

            var files = li
                .Where(x => x.Type == ClientItemType.File && x.FullPath.StartsWith(d.FullPath))
                .Select(x => new TreeNode(x.Name))
                .ToArray();

            foreach (var f in folders)
            {
                parent.Nodes.Add(ConstructNodeFrom(li, f));
            }

            parent.Nodes.AddRange(files);

            return parent;
        }

        /// <summary>
        /// Checks every parent node of tn
        /// </summary>
        public static void CheckSingleRoute(TreeNode tn)
        {
            while (true)
            {
                if (tn.Checked && tn.Parent != null)
                    if (!tn.Parent.Checked)
                    {
                        tn.Parent.Checked = true;
                        tn = tn.Parent;
                        continue;
                    }
                break;
            }
        }

        /// <summary>
        /// Recursively uncheck ignored files/folders
        /// </summary>
        public static void EditNodeCheckboxesRecursive(TreeNodeCollection nodes, List<string> refList = null)
        {
            refList = refList ?? new List<string>();
            foreach (TreeNode t in nodes)
            {
                t.Checked = !refList.Contains(t.FullPath);

                if (t.Checked)
                {
                    EditNodeCheckboxesRecursive(t.Nodes, refList);
                }
            }
        }

        /// <summary>
        /// Returns a list of all unchecked items
        /// </summary>
        public static IEnumerable<string> GetUncheckedItems(TreeNodeCollection t)
        {
            foreach (TreeNode node in t)
            {
                if (!node.Checked)
                    yield return node.FullPath;

                if (node.Checked && node.Nodes.Count > 0)
                {
                    foreach (var child in GetUncheckedItems(node.Nodes))
                        yield return child;
                }
            }
        }

        /// <summary>
        /// Check/uncheck all child nodes
        /// </summary>
        /// <param name="t">The parent node</param>
        /// <param name="c"><c>True</c> to check, <c>False</c> to uncheck</param>
        public static void CheckUncheckChildNodes(TreeNode t, bool c)
        {
            t.Checked = c;
            foreach (TreeNode tn in t.Nodes)
                CheckUncheckChildNodes(tn, c);
        }

        #endregion

        /// <summary>
        ///     Display a messagebox with the certificate details, ask user to approve/decline it.
        /// </summary>
        public static void CheckCertificate(object sender, ValidateCertificateEventArgs n)
        {
            // Do we trust the server's certificate?
            var certificateTrustedResult = MessageBox.Show(n.ValidationMessage(), "Do you trust this certificate?", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            var trusted = (certificateTrustedResult == DialogResult.Yes);

            n.IsTrusted = trusted;

            if (trusted)
            {
                Settings.TrustedCertificates.Add(n.Fingerprint);
                Settings.SaveCertificates();
            }
        }
    }
}
