using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WebWind.Core;

public abstract class Component
{
    public string Id => $"Id{IdNumber}";

    internal Int64 IdNumber;

    public JsSnippet JS => ClientInterface.Current.JS;
    
    internal List<Component> children = new List<Component>();
    internal Component parent = null;

    internal Dictionary<string, Action<string>> m_RegisteredEvents = new Dictionary<string, Action<string>>();

    public Component()
    {
        IdNumber = ClientInterface.Current.NextComponentId();
        ClientInterface.Current.m_ComponentRegistry[IdNumber] = this;
    }

    public virtual void DestructFrontEnd() { }

    public virtual string GetSocketSelector()
    {
        return $"#{Id}";
    }

    public List<Component> GetChildren()
    {
        return new List<Component>(children);
    }

    public void SetParent(Component newParent)
    {
        if (this.parent != null)
        {
            this.parent.children.Remove(this);
            JS.Eval($"{Id}.remove();");
        }

        this.parent = newParent;
        
        if (this.parent != null)
        {
            this.parent.children.Add(this);
            JS.Eval($"{this.parent.Id}.appendChild({Id});");
        }
    }
    public T CreateChild<T>(params object[] args) where T : Component
    {
        // Get the type of T
        Type type = typeof(T);
        
        // Get the constructor that matches the parameters
        ConstructorInfo constructor = type.GetConstructor(args.Select(arg => arg.GetType()).ToArray());
        
        if (constructor == null)
        {
            throw new ArgumentException("No matching constructor found.");
        }

        // Create an instance using the constructor and parameters
        T child = (T)constructor.Invoke(args);
        child.SetParent(this);
        return child;
    }

    public void RegisterClientToServerEvent(string eventName, string js, Action<string> callback)
    {
        JS.Eval($"{Id}.addEventListener('{eventName}', (context) => {{let data = {{}}; {js}; socket.send(`event {IdNumber} {eventName} ${{JSON.stringify(data)}}`);}});");
        m_RegisteredEvents[eventName] = callback;
    }

    public void AddClassName(string className)
    {
        JS.Eval($"{Id}.classList.add(`{className}`);");
    }
    public void RemoveClassName(string className)
    {
        JS.Eval($"{Id}.classList.remove(`{className}`);");
    }

}