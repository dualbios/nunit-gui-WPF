using ReactiveUI;

namespace NUnit3Gui.Extensions
{
    public static class ReactiveObjectExtensions
    {
        public static void PropertiesChanged(this ReactiveObject obj, params string[] properties)
        {
            foreach (string property in properties)
            {
                obj.RaisePropertyChanged(property);
            }
        }
    }
}