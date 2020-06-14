using SignGen.WpfApp.Other;

namespace SignGen.WpfApp.ViewModel
{
    public abstract class ViewModelBase : PropertyChangedBase
    {
        /// <summary>
        /// Der anzuzeigende Titel der View
        /// </summary>
        public abstract string Title { get; }
    }
}
