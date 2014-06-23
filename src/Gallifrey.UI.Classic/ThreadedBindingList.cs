using System.ComponentModel;
using System.Threading;

namespace Gallifrey.UI.Classic
{
    public class ThreadedBindingList<T> : BindingList<T>
    {
        SynchronizationContext ctx = SynchronizationContext.Current;
        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            if (ctx == null)
            {
                BaseAddingNew(e);
            }
            else
            {
                ctx.Send(delegate
                { BaseAddingNew(e); }, null);
            }
        }

        void BaseAddingNew(AddingNewEventArgs e)
        {
            base.OnAddingNew(e);
        }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (ctx == null)
            {
                BaseListChanged(e);
            }
            else
            {
                ctx.Send(delegate
                { BaseListChanged(e); }, null);
            }
        }

        void BaseListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);
        }
    }
}
