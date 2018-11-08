using System.Diagnostics;
using System.Windows;

namespace NUnit3GUIWPF.Utils
{
    public class MessageListener : DependencyObject
    {
        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(MessageListener), new UIPropertyMetadata(null));

        /// <summary>
        ///
        /// </summary>
        private static MessageListener mInstance;

        /// <summary>
        ///
        /// </summary>
        private MessageListener()
        {
        }

        /// <summary>
        /// Get MessageListener instance
        /// </summary>
        public static MessageListener Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new MessageListener();
                return mInstance;
            }
        }

        /// <summary>
        /// Get or set received message
        /// </summary>
        public string Message
        {
            get { return (string) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        public void ReceiveMessage(string message)
        {
            Message = message;
            Debug.WriteLine(Message);
            DispatcherHelper.DoEvents();
        }
    }
}