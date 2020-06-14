namespace SignGen.WpfApp.Other
{
    public class BindingWrapper<T> : PropertyChangedBase
    {
        private T _item;

        public BindingWrapper(T item)
        {
            Item = item;
        }

        public T Item
        {
            get => _item;
            set => Update(value, ref _item);
        }
    }
}
