using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace AssemblyReferenceFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Private Members

        readonly IAssemblyManifestDetails _refFinder;
        List<AssemblyReferences> _refList; 

        #endregion

        #region Constructor
        
        public MainWindow()
        {
            _refFinder = new AssemblyManifestDetails();
            _refList = new List<AssemblyReferences>();
            InitializeComponent();
        } 

        #endregion

        #region Event Handlers
        
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            _refList.Clear();
            cmbAssemblies.Items.Clear();
            if (txtPath.Text.Length > 0)
            {
                if (txtPath.Text.EndsWith("dll", true, CultureInfo.CurrentCulture) |
                    txtPath.Text.EndsWith("exe", true, CultureInfo.CurrentCulture))
                {
                    AssemblyReferences references = _refFinder.GetAssemblyReferences(txtPath.Text);
                    _refList.Add(references);
                }
                else
                {
                    _refList = _refFinder.GetAllAssemblyDetailsFromPath(txtPath.Text);
                }

                PopulateComboBox();
            }
        }

        private void PopulateComboBox()
        {
            foreach (AssemblyReferences reference in _refList)
            {
                cmbAssemblies.Items.Add(reference.AssemblyName);
            }

            if (cmbAssemblies.Items.Count > 0)
            {
                cmbAssemblies.SelectedIndex = 0;
            }
        }

        private void cmbAssemblies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstResults.Items.Clear();
            foreach (AssemblyReferences reference in _refList)
            {
                if (reference.AssemblyName == cmbAssemblies.SelectedValue.ToString())
                {
                    foreach (AssemblyReferences internalRef in reference.AssembliesReferenced)
                    {
                        lstResults.Items.Add(internalRef.FullAssemblyName);
                    }
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _refList.Clear();
            cmbAssemblies.Items.Clear();
            if (txtPath.Text.Length > 0)
            {
                lstResults.Items.Clear();
                AssemblyReferences assembly = _refFinder.FindReferencedAssemblies(txtPath.Text,txtReferenceFind.Text);
                _refList.Add(assembly);
                PopulateComboBox();                
            }
        }

        private void lstResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstResults.SelectedValue == null)
                return;
            txtReferenceFind.Text = lstResults.SelectedValue.ToString().Split(',')[0];
        }

        #endregion
    }
}
