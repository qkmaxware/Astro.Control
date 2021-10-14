using System.Collections.Generic;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Base class for any source that can notify subscribers in case of changes
/// </summary>
/// <typeparam name="T">type of subscriber</typeparam>
public class Notifier<T> {
    protected List<T> Subscribers {get; private set;} = new List<T>();
    
    /// <summary>
    /// Have a listener subscribe to notifications from this source
    /// </summary>
    /// <param name="subscriber">object to subscribe</param>
    public void Subscribe(T subscriber) {
        if (subscriber != null)
            this.Subscribers.Add(subscriber);
    }

    /// <summary>
    /// Have a listener stop listening to notifications from this source
    /// </summary>
    /// <param name="subscriber">object to unsubscribe</param>
    public void Unsubscribe(T subscriber) {
        this.Subscribers.Remove(subscriber);
    }

    /// <summary>
    /// Have this source forcefully unsubscribe all listeners
    /// </summary>
    public void UnsubscribeAll() {
        this.Subscribers.Clear();
    }
}

}