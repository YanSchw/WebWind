using WebWind.Core;

namespace WebWind.Components;

public class Button : Component
{
    private List<Action> m_OnClickedEvents = new List<Action>();
    
    public Button(string text)
    {
        JS.CreateDocumentElement("button", Id);
        RegisterClientToServerEvent("click", "", (json) =>
        {
            foreach (Action action in m_OnClickedEvents)
            {
                action();
            }
        });
        CreateChild<Paragraph>(text);
    }

    public void AddOnClickedEvent(Action callback)
    {
        m_OnClickedEvents.Add(callback);
    }

}