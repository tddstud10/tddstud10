using Microsoft.VisualStudio.Text.Editor;

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App.Exports
{
    public class DefaultKeyProcessor : KeyProcessor
    {
        public override bool IsInterestedInHandledEvents
        {
            get
            {
                return true;
            }
        }

        public override void KeyUp(System.Windows.Input.KeyEventArgs args)
        {
            base.KeyUp(args);
        }

        public override void KeyDown(System.Windows.Input.KeyEventArgs args)
        {
            base.KeyDown(args);
        }
    }
}
